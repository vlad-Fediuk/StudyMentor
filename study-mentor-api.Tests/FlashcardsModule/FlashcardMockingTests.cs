using Moq;
using StudyMentorApi.ChatMessages;
using StudyMentorApi.Common;
using StudyMentorApi.Data;
using StudyMentorApi.Data.Models;
using StudyMentorApi.Flashcards;
using study_mentor_api.Tests.TestHelpers;

namespace study_mentor_api.Tests.FlashcardsModule;

[TestFixture]
public class FlashcardMockingTests
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
    public async Task CreateAsync_ShouldCallChatMessageServiceGetById_WhenChatMessageIdProvided()
    {
        var chatMessageId = Guid.NewGuid();
        _chatMessageServiceMock
            .Setup(s => s.GetByIdAsync(chatMessageId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new ChatMessage { Content = "Test content", Role = MessageRole.Assistant }.WithId(chatMessageId));

        var flashcard = new Flashcard
        {
            Name = "Algorithms",
            ChatMessageId = chatMessageId,
            Cards =
            [
                new Card { Term = "BFS", Definition = "Breadth-first search" }
            ]
        };

        var result = await _service.CreateAsync(flashcard);

        Assert.That(result, Is.Not.Null);
        _chatMessageServiceMock.Verify(
            s => s.GetByIdAsync(chatMessageId, It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Test]
    public async Task CreateAsync_ShouldThrowNotFoundException_WhenChatMessageNotFound()
    {
        var chatMessageId = Guid.NewGuid();
        _chatMessageServiceMock
            .Setup(s => s.GetByIdAsync(chatMessageId, It.IsAny<CancellationToken>()))
            .ThrowsAsync(new NotFoundException("Chat message not found"));

        var flashcard = new Flashcard
        {
            Name = "Data Structures",
            ChatMessageId = chatMessageId,
            Cards =
            [
                new Card { Term = "Tree", Definition = "Hierarchical data structure" }
            ]
        };

        var ex = Assert.ThrowsAsync<NotFoundException>(() => _service.CreateAsync(flashcard));
        Assert.That(ex!.Message, Is.EqualTo("Chat message not found"));

        var allFlashcards = await _service.GetAllAsync();
        Assert.That(allFlashcards, Is.Empty);
    }

    [Test]
    public async Task DeleteAsync_ShouldCallRemoveOnDbContext_WhenValidIdProvided()
    {
        var flashcard = new Flashcard
        {
            Name = "Temporary Flashcard",
            ChatMessageId = Guid.NewGuid(),
            Cards =
            [
                new Card { Term = "Temp", Definition = "Temporary definition" }
            ]
        };

        _dbContext.Flashcards.Add(flashcard);
        await _dbContext.SaveChangesAsync();

        var existingId = flashcard.Id;
        await _service.DeleteAsync(existingId);

        var entityInDb = await _dbContext.Flashcards.FindAsync(existingId);
        Assert.That(entityInDb, Is.Null);
    }

    [Test]
    public void CreateAsync_ShouldNotCallChatMessageService_WhenValidationFailsDueToEmptyName()
    {
        var flashcard = new Flashcard
        {
            Name = "",
            ChatMessageId = Guid.NewGuid(),
            Cards =
            [
                new Card { Term = "Term", Definition = "Definition" }
            ]
        };

        Assert.ThrowsAsync<ValidationException>(() => _service.CreateAsync(flashcard));

        _chatMessageServiceMock.Verify(
            s => s.GetByIdAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()),
            Times.Never);
    }
}
