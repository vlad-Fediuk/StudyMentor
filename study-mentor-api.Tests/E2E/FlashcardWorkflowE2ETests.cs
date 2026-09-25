using Microsoft.EntityFrameworkCore;
using StudyMentorApi.ChatMessages;
using StudyMentorApi.Common;
using StudyMentorApi.Data;
using StudyMentorApi.Data.Models;
using StudyMentorApi.Flashcards;
using study_mentor_api.Tests.TestHelpers;

namespace study_mentor_api.Tests.E2E;

[TestFixture]
public class FlashcardWorkflowE2ETests
{
    private AppDbContext _dbContext = null!;
    private ChatMessageService _chatMessageService = null!;
    private FlashcardService _flashcardService = null!;

    [SetUp]
    public void SetUp()
    {
        _dbContext = TestDbContextFactory.Create();
        _chatMessageService = new ChatMessageService(_dbContext);
        _flashcardService = new FlashcardService(_dbContext, _chatMessageService);
    }

    [TearDown]
    public void TearDown()
    {
        _dbContext.Dispose();
    }

    [Test]
    public async Task StudentCardMemorizationScenario_ShouldCreateReviewAndCycleCards()
    {
        var chatMessage = new ChatMessage
        {
            Content = "Computer Networks Exam Prep",
            Role = MessageRole.Assistant,
            Timestamp = DateTime.UtcNow,
            SequenceNumber = 1
        };
        _dbContext.ChatMessages.Add(chatMessage);
        await _dbContext.SaveChangesAsync();

        var flashcardDeck = new Flashcard
        {
            Name = "Network Protocols",
            ChatMessageId = chatMessage.Id,
            Cards =
            [
                new Card { Term = "OSI", Definition = "Open Systems Interconnection 7-layer model" },
                new Card { Term = "TCP", Definition = "Transmission Control Protocol - reliable connection" },
                new Card { Term = "UDP", Definition = "User Datagram Protocol - connectionless" },
                new Card { Term = "IP", Definition = "Internet Protocol - packet addressing" },
                new Card { Term = "DNS", Definition = "Domain Name System - domain to IP resolution" }
            ]
        };

        var createdDeck = await _flashcardService.CreateAsync(flashcardDeck);
        Assert.That(createdDeck, Is.Not.Null);
        Assert.That(createdDeck.Cards, Has.Count.EqualTo(5));

        var allDecks = await _flashcardService.GetAllAsync();
        var studentDeck = allDecks.FirstOrDefault(d => d.Id == createdDeck.Id);
        Assert.That(studentDeck, Is.Not.Null);
        Assert.That(studentDeck!.Cards, Has.Count.EqualTo(5));

        var updatedDeckRequest = new Flashcard
        {
            Name = "Network Protocols - Mastered",
            ChatMessageId = chatMessage.Id,
            Cards =
            [
                new Card { Term = "TCP", Definition = "Reliable, connection-oriented with 3-way handshake" },
                new Card { Term = "UDP", Definition = "Fast, unreliable, low latency" }
            ]
        };

        var updatedDeck = await _flashcardService.UpdateAsync(createdDeck.Id, updatedDeckRequest);
        Assert.That(updatedDeck.Name, Is.EqualTo("Network Protocols - Mastered"));
        Assert.That(updatedDeck.Cards, Has.Count.EqualTo(2));
        Assert.That(updatedDeck.Cards.Any(c => c.Term == "TCP" && c.Definition.Contains("handshake")), Is.True);

        await _flashcardService.DeleteAsync(createdDeck.Id);

        var ex = Assert.ThrowsAsync<NotFoundException>(() => _flashcardService.GetByIdAsync(createdDeck.Id));
        Assert.That(ex!.Message, Does.Contain("was not found"));

        var remainingDecks = await _flashcardService.GetAllAsync();
        Assert.That(remainingDecks.Any(d => d.Id == createdDeck.Id), Is.False);
    }

    [Test]
    public async Task ChatToFlashcardsConversionScenario_ShouldLinkCardsToChatMessageHistory()
    {
        var chatMessage = new ChatMessage
        {
            Content = "Artificial Intelligence and Machine Learning definitions summary",
            Role = MessageRole.Assistant,
            Timestamp = DateTime.UtcNow,
            SequenceNumber = 1
        };
        _dbContext.ChatMessages.Add(chatMessage);
        await _dbContext.SaveChangesAsync();

        var aiGeneratedCards = new Flashcard
        {
            Name = "AI & ML Key Concepts",
            ChatMessageId = chatMessage.Id,
            Cards =
            [
                new Card { Term = "Overfitting", Definition = "Model learns training noise instead of pattern" },
                new Card { Term = "Regularization", Definition = "Technique to prevent overfitting" }
            ]
        };

        var createdDeck = await _flashcardService.CreateAsync(aiGeneratedCards);
        Assert.That(createdDeck.Id, Is.Not.EqualTo(Guid.Empty));

        var messageWithExercises = await _dbContext.ChatMessages
            .Include(m => m.Exercises)
            .FirstOrDefaultAsync(m => m.Id == chatMessage.Id);

        Assert.That(messageWithExercises, Is.Not.Null);
        Assert.That(messageWithExercises!.Exercises, Has.Count.EqualTo(1));

        var linkedExercise = messageWithExercises.Exercises.First();
        Assert.That(linkedExercise.Id, Is.EqualTo(createdDeck.Id));
        Assert.That(linkedExercise.Name, Is.EqualTo("AI & ML Key Concepts"));
        Assert.That(linkedExercise.ChatMessageId, Is.EqualTo(chatMessage.Id));

        var fetchedDeck = await _flashcardService.GetByIdAsync(linkedExercise.Id);
        Assert.That(fetchedDeck, Is.Not.Null);
        Assert.That(fetchedDeck.Cards, Has.Count.EqualTo(2));
        Assert.That(fetchedDeck.ChatMessageId, Is.EqualTo(chatMessage.Id));
    }
}
