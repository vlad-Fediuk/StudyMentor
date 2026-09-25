using Moq;
using StudyMentorApi.ChatMessages;
using StudyMentorApi.Data;
using StudyMentorApi.Data.Models;
using StudyMentorApi.Flashcards;
using study_mentor_api.Tests.TestHelpers;

namespace study_mentor_api.Tests.FlashcardsModule;

[TestFixture]
public class FlashcardUnitTests
{
    private AppDbContext _dbContext = null!;
    private Mock<ChatMessageService> _chatMessageServiceMock = null!;
    private FlashcardService _service = null!;

    [SetUp]
    public void SetUp()
    {
        _dbContext = TestDbContextFactory.Create();
        _chatMessageServiceMock = new Mock<ChatMessageService>(_dbContext);
        _service = new FlashcardService(_dbContext, _chatMessageServiceMock.Object);
    }

    [TearDown]
    public void TearDown()
    {
        _dbContext.Dispose();
    }

    [Test]
    public async Task ValidateAsync_ShouldPass_WhenFlashcardHasValidCardsWithTermsAndDefinitions()
    {
        var chatMessageId = Guid.NewGuid();
        _chatMessageServiceMock
            .Setup(s => s.GetByIdAsync(chatMessageId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new ChatMessage { Content = "History note", Role = MessageRole.Assistant }.WithId(chatMessageId));

        var validFlashcard = new Flashcard
        {
            Name = "History Terms",
            ChatMessageId = chatMessageId,
            Cards =
            [
                new Card { Term = "Renaissance", Definition = "Cultural rebirth" },
                new Card { Term = "Reformation", Definition = "Religious reform" }
            ]
        };

        var result = await _service.CreateAsync(validFlashcard);

        Assert.That(result, Is.Not.Null);
        Assert.That(result.Name, Is.EqualTo("History Terms"));
        Assert.That(result.Cards, Has.Count.EqualTo(2));
    }

    [Test]
    public async Task UpdateEntityValues_ShouldReplaceCardsCollection_WhenUpdatedWithNewCards()
    {
        var chatMessageId = Guid.NewGuid();
        _chatMessageServiceMock
            .Setup(s => s.GetByIdAsync(chatMessageId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new ChatMessage { Content = "Math note", Role = MessageRole.Assistant }.WithId(chatMessageId));

        var existingEntity = new Flashcard
        {
            Name = "Algebra Old",
            ChatMessageId = chatMessageId,
            Cards =
            [
                new Card { Term = "Old Term 1", Definition = "Old Def 1" },
                new Card { Term = "Old Term 2", Definition = "Old Def 2" }
            ]
        };

        _dbContext.Flashcards.Add(existingEntity);
        await _dbContext.SaveChangesAsync();

        var updatedEntity = new Flashcard
        {
            Name = "Algebra New",
            ChatMessageId = chatMessageId,
            Cards =
            [
                new Card { Term = "New Variable", Definition = "Symbol representing quantity" }
            ]
        };

        var result = await _service.UpdateAsync(existingEntity.Id, updatedEntity);

        Assert.That(result.Cards, Has.Count.EqualTo(1));
        Assert.That(result.Cards.First().Term, Is.EqualTo("New Variable"));
        Assert.That(result.Name, Is.EqualTo("Algebra New"));
    }

    [Test]
    public async Task GetByIdAsync_ShouldReturnFlashcardWithCards_WhenEntityExists()
    {
        var chatMessageId = Guid.NewGuid();
        var flashcard = new Flashcard
        {
            Name = "Biology Fundamentals",
            ChatMessageId = chatMessageId,
            Cards =
            [
                new Card { Term = "Mitosis", Definition = "Process of cell division" }
            ]
        };

        _dbContext.Flashcards.Add(flashcard);
        await _dbContext.SaveChangesAsync();

        var result = await _service.GetByIdAsync(flashcard.Id);

        Assert.That(result, Is.Not.Null);
        Assert.That(result.Id, Is.EqualTo(flashcard.Id));
        Assert.That(result.Cards, Has.Count.EqualTo(1));
        Assert.That(result.Cards[0].Term, Is.EqualTo("Mitosis"));
    }

    [Test]
    public async Task GetAllAsync_ShouldReturnAllFlashcardSets_WhenRecordsExistInDatabase()
    {
        var chatMessageId = Guid.NewGuid();
        _dbContext.Flashcards.AddRange(
            new Flashcard
            {
                Name = "Chemistry Set",
                ChatMessageId = chatMessageId,
                Cards = [new Card { Term = "Acid", Definition = "Proton donor" }]
            },
            new Flashcard
            {
                Name = "Physics Set",
                ChatMessageId = chatMessageId,
                Cards = [new Card { Term = "Force", Definition = "Mass times acceleration" }]
            }
        );
        await _dbContext.SaveChangesAsync();

        var result = await _service.GetAllAsync();

        Assert.That(result, Has.Count.EqualTo(2));
    }
}
