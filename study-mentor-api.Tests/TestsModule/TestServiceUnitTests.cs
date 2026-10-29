using StudyMentorApi.ChatMessages;
using StudyMentorApi.Data;
using StudyMentorApi.Data.Models;
using StudyMentorApi.Flashcards;
using StudyMentorApi.Tests;
using study_mentor_api.Tests.TestHelpers;

namespace study_mentor_api.Tests.TestsModule;

[TestFixture]
public class TestServiceUnitTests
{
    private AppDbContext _dbContext = null!;
    private ChatMessageService _chatMessageService = null!;
    private FlashcardService _flashcardService = null!;
    private TestableTestService _service = null!;

    private sealed class TestableTestService(
        AppDbContext dbContext,
        ChatMessageService chatMessageService,
        FlashcardService flashcardService)
        : TestService(dbContext, chatMessageService, flashcardService)
    {
        public void CallUpdateEntityValues(Test existingEntity, Test updatedEntity)
        {
            UpdateEntityValues(existingEntity, updatedEntity);
        }
    }

    [SetUp]
    public void SetUp()
    {
        _dbContext = TestDbContextFactory.Create();
        _chatMessageService = new ChatMessageService(_dbContext);
        _flashcardService = new FlashcardService(_dbContext, _chatMessageService);
        _service = new TestableTestService(_dbContext, _chatMessageService, _flashcardService);
    }

    [TearDown]
    public void TearDown()
    {
        _dbContext.Database.EnsureDeleted();
        _dbContext.Dispose();
    }

    [Test]
    public async Task CheckAnswerAsync_ShouldReturnCorrectResult_WhenAnswerIsCorrect()
    {
        var testId = Guid.NewGuid();
        var questionId = Guid.NewGuid();
        var correctAnswerVariantId = Guid.NewGuid();
        var wrongVariant1Id = Guid.NewGuid();
        var wrongVariant2Id = Guid.NewGuid();

        var test = new Test
        {
            Name = "Sample Test",
            Questions =
            [
                new TestQuestion
                {
                    Prompt = "Sample Question",
                    Order = 0,
                    AnswerVariants =
                    [
                        new TestAnswerVariant
                        {
                            Text = "Wrong 1",
                            IsCorrect = false,
                            Order = 0
                        }.WithId(wrongVariant1Id),
                        new TestAnswerVariant
                        {
                            Text = "Correct",
                            IsCorrect = true,
                            Order = 1
                        }.WithId(correctAnswerVariantId),
                        new TestAnswerVariant
                        {
                            Text = "Wrong 2",
                            IsCorrect = false,
                            Order = 2
                        }.WithId(wrongVariant2Id)
                    ]
                }.WithId(questionId)
            ]
        }.WithId(testId);

        _dbContext.Tests.Add(test);
        await _dbContext.SaveChangesAsync();

        var result = await _service.CheckAnswerAsync(testId, questionId, correctAnswerVariantId);

        Assert.Multiple(() =>
        {
            Assert.That(result.IsCorrect, Is.True);
            Assert.That(result.SelectedAnswerVariantId, Is.EqualTo(correctAnswerVariantId));
            Assert.That(result.CorrectAnswerVariantId, Is.EqualTo(correctAnswerVariantId));
            Assert.That(result.SelectedAnswerVariantId, Is.EqualTo(result.CorrectAnswerVariantId));
        });
    }

    [Test]
    public async Task CheckAnswerAsync_ShouldReturnIncorrectResult_WhenAnswerIsWrong()
    {
        var testId = Guid.NewGuid();
        var questionId = Guid.NewGuid();
        var correctAnswerVariantId = Guid.NewGuid();
        var wrongAnswerVariantId = Guid.NewGuid();

        var test = new Test
        {
            Name = "Biology Test",
            Questions =
            [
                new TestQuestion
                {
                    Prompt = "Select correct option",
                    Order = 0,
                    AnswerVariants =
                    [
                        new TestAnswerVariant
                        {
                            Text = "Correct Variant",
                            IsCorrect = true,
                            Order = 0
                        }.WithId(correctAnswerVariantId),
                        new TestAnswerVariant
                        {
                            Text = "Wrong Variant",
                            IsCorrect = false,
                            Order = 1
                        }.WithId(wrongAnswerVariantId)
                    ]
                }.WithId(questionId)
            ]
        }.WithId(testId);

        _dbContext.Tests.Add(test);
        await _dbContext.SaveChangesAsync();

        var result = await _service.CheckAnswerAsync(testId, questionId, wrongAnswerVariantId);

        Assert.Multiple(() =>
        {
            Assert.That(result.IsCorrect, Is.False);
            Assert.That(result.SelectedAnswerVariantId, Is.EqualTo(wrongAnswerVariantId));
            Assert.That(result.CorrectAnswerVariantId, Is.EqualTo(correctAnswerVariantId));
            Assert.That(result.SelectedAnswerVariantId, Is.Not.EqualTo(result.CorrectAnswerVariantId));
        });
    }

    [Test]
    public void UpdateEntityValues_ShouldReplaceQuestionsCollection_WhenUpdatedWithNewQuestions()
    {
        var existingEntity = new Test
        {
            Name = "Initial Test Name",
            Questions =
            [
                new TestQuestion { Prompt = "Old Question 1" }.WithId(Guid.NewGuid()),
                new TestQuestion { Prompt = "Old Question 2" }.WithId(Guid.NewGuid())
            ]
        }.WithId(Guid.NewGuid());

        var updatedEntity = new Test
        {
            Name = "Updated Test Name",
            Questions =
            [
                new TestQuestion { Prompt = "New Question 1" }.WithId(Guid.NewGuid()),
                new TestQuestion { Prompt = "New Question 2" }.WithId(Guid.NewGuid()),
                new TestQuestion { Prompt = "New Question 3" }.WithId(Guid.NewGuid())
            ]
        }.WithId(Guid.NewGuid());

        _service.CallUpdateEntityValues(existingEntity, updatedEntity);

        Assert.Multiple(() =>
        {
            Assert.That(existingEntity.Name, Is.EqualTo("Updated Test Name"));
            Assert.That(existingEntity.Questions, Has.Count.EqualTo(3));
            Assert.That(existingEntity.Questions.Select(q => q.Prompt), Is.EqualTo(new[] { "New Question 1", "New Question 2", "New Question 3" }));
        });
    }

    [Test]
    public async Task GetAllAsync_ShouldFilterBySourceFlashcardId_WhenSourceFlashcardIdSpecified()
    {
        var targetSourceFlashcardId = Guid.NewGuid();
        var anotherSourceFlashcardId = Guid.NewGuid();

        var test1 = new Test
        {
            Name = "Target Test 1",
            SourceFlashcardId = targetSourceFlashcardId
        }.WithId(Guid.NewGuid());

        var test2 = new Test
        {
            Name = "Target Test 2",
            SourceFlashcardId = targetSourceFlashcardId
        }.WithId(Guid.NewGuid());

        var test3 = new Test
        {
            Name = "Other Test",
            SourceFlashcardId = anotherSourceFlashcardId
        }.WithId(Guid.NewGuid());

        _dbContext.Tests.AddRange(test1, test2, test3);
        await _dbContext.SaveChangesAsync();

        var allTests = await _service.GetAllAsync();
        var filteredTests = allTests.Where(t => t.SourceFlashcardId == targetSourceFlashcardId).ToList();

        Assert.Multiple(() =>
        {
            Assert.That(filteredTests, Has.Count.EqualTo(2));
            Assert.That(filteredTests.All(t => t.SourceFlashcardId == targetSourceFlashcardId), Is.True);
        });
    }
}
