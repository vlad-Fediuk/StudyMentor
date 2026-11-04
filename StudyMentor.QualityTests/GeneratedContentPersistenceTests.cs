using StudyMentorApi.Data.Models;
using StudyMentorApi.Flashcards;
using StudyMentorApi.LearningContent;
using StudyMentorApi.Services.Ai.StructuredOutput;
using StudyMentorApi.Tests;

namespace StudyMentor.QualityTests;

public class GeneratedContentPersistenceTests
{
    private static readonly int[] ExpectedAnswerOrder = [0, 1];
    private static readonly string[] ExpectedFallbackAnswers = ["Answer", "Варіант 2"];
    [Fact]
    public async Task FlashcardPersistenceKeepsTrimmingTitleAndMessageLink()
    {
        var flashcards = new CapturingFlashcards();
        var persistence = new GeneratedContentPersistence(flashcards, new CapturingTests());
        var messageId = Guid.NewGuid();
        var result = await persistence.PersistAsync(new("flashcard", new GeneratedFlashcardsDto
        {
            Cards = [new() { Front = " Term ", Back = " Definition " }]
        }), messageId, "  " + new string('x', 60) + "  ", default);
        var response = Assert.IsType<FlashcardResponse>(result);
        Assert.Equal("Картки: " + new string('x', 48) + "...", response.Name);
        Assert.Equal(messageId, response.ChatMessageId);
        var card = Assert.Single(response.Cards);
        Assert.Equal("Term", card.Term);
        Assert.Equal("Definition", card.Definition);
    }

    [Theory]
    [InlineData(true)]
    [InlineData(false)]
    public async Task TestPersistenceKeepsAnswerDeduplicationAndCorrectAnswer(bool includeCorrect)
    {
        var tests = new CapturingTests();
        var persistence = new GeneratedContentPersistence(new CapturingFlashcards(), tests);
        await persistence.PersistAsync(new("test", new GeneratedTestDto
        {
            Title = " Title ",
            Questions = [new()
            {
                Text = " Question ", CorrectAnswer = " Right ",
                Options = includeCorrect ? [" right ", "RIGHT", " Wrong ", " "] : [" Wrong ", "wrong", " "]
            }]
        }), Guid.NewGuid(), "Topic", default);
        Assert.NotNull(tests.Created);
        Assert.Equal("Title", tests.Created.Name);
        var question = Assert.Single(tests.Created.Questions);
        Assert.Equal("Question", question.Prompt);
        Assert.Equal(2, question.AnswerVariants.Count);
        Assert.Single(question.AnswerVariants, answer => answer.IsCorrect);
        Assert.Equal(ExpectedAnswerOrder, question.AnswerVariants.Select(answer => answer.Order));
    }

    [Fact]
    public async Task MissingOptionsPreserveFallbackVariantAndTitle()
    {
        var tests = new CapturingTests();
        var persistence = new GeneratedContentPersistence(new CapturingFlashcards(), tests);
        await persistence.PersistAsync(new("test", new GeneratedTestDto
        {
            Questions = [new() { Text = "Question", CorrectAnswer = "Answer" }]
        }), Guid.NewGuid(), " Topic ", default);
        Assert.Equal("Тест: Topic", tests.Created!.Name);
        Assert.Equal(ExpectedFallbackAnswers, tests.Created.Questions.Single().AnswerVariants.Select(a => a.Text));
    }

    private sealed class CapturingFlashcards() : FlashcardService(null!, null!)
    {
        public override Task<Flashcard> CreateAsync(Flashcard entity, CancellationToken cancellationToken = default) => Task.FromResult(entity);
    }

    [Fact]
    public async Task UnknownGeneratedContentTypeIsRejected()
    {
        var persistence = new GeneratedContentPersistence(new CapturingFlashcards(), new CapturingTests());
        await Assert.ThrowsAsync<StudyMentorApi.Common.ValidationException>(() =>
            persistence.PersistAsync(new("unknown", new object()), Guid.NewGuid(), "Topic", default));
    }

    private sealed class CapturingTests() : TestService(null!, null!, null!)
    {
        public StudyMentorApi.Data.Models.Test? Created { get; private set; }
        public override Task<StudyMentorApi.Data.Models.Test> CreateAsync(StudyMentorApi.Data.Models.Test entity, CancellationToken cancellationToken = default)
        {
            Created = entity;
            return Task.FromResult(entity);
        }
    }
}
