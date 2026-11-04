using Moq;
using StudyMentorApi.ChatMessages;
using StudyMentorApi.Common;
using StudyMentorApi.Data;
using StudyMentorApi.Data.Models;
using StudyMentorApi.Flashcards;
using study_mentor_api.Tests.TestHelpers;

namespace study_mentor_api.Tests.FlashcardsModule;

[TestFixture]
public class FlashcardExceptionTests
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

    [TestCase("")]
    [TestCase("   ")]
    public void ValidateAsync_ShouldThrowValidationException_WhenFlashcardNameIsEmpty(string invalidName)
    {
        var invalidSet = new Flashcard
        {
            Name = invalidName,
            ChatMessageId = Guid.NewGuid(),
            Cards =
            [
                new Card { Term = "Syntax", Definition = "Rules of language" }
            ]
        };

        var ex = Assert.ThrowsAsync<ValidationException>(() => _service.CreateAsync(invalidSet));
        Assert.That(ex!.Message, Is.EqualTo("Flashcard name is required."));
    }

    [Test]
    public void ValidateAsync_ShouldThrowValidationException_WhenCardTermOrDefinitionIsEmpty()
    {
        var invalidSet = new Flashcard
        {
            Name = "Terminology",
            ChatMessageId = Guid.NewGuid(),
            Cards =
            [
                new Card { Term = "Термін", Definition = "" }
            ]
        };

        var ex = Assert.ThrowsAsync<ValidationException>(() => _service.CreateAsync(invalidSet));
        Assert.That(ex!.Message, Is.EqualTo("Every card must have a term and definition."));
    }

    [Test]
    public void GetByIdAsync_ShouldThrowNotFoundException_WhenFlashcardIdDoesNotExist()
    {
        var randomId = Guid.NewGuid();

        var ex = Assert.ThrowsAsync<NotFoundException>(() => _service.GetByIdAsync(randomId));
        Assert.That(ex!.Message, Does.Contain("was not found"));
    }

    [Test]
    public void ValidateAsync_ShouldThrowValidationException_WhenCardsCollectionIsEmpty()
    {
        var invalidSet = new Flashcard
        {
            Name = "Empty Set",
            ChatMessageId = Guid.NewGuid(),
            Cards = []
        };

        var ex = Assert.ThrowsAsync<ValidationException>(() => _service.CreateAsync(invalidSet));
        Assert.That(ex!.Message, Is.EqualTo("Flashcard must contain at least one card."));
    }
}
