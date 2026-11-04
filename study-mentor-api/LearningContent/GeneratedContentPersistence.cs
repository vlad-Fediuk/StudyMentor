using StudyMentorApi.Common;
using StudyMentorApi.Data.Models;
using StudyMentorApi.Flashcards;
using StudyMentorApi.Services.Ai.StructuredOutput;
using StudyMentorApi.Tests;

namespace StudyMentorApi.LearningContent;

public sealed class GeneratedContentPersistence(FlashcardService flashcardService, TestService testService)
{
    private const string TestType = "test";
    private const string FlashcardType = "flashcard";
    private const int MaxExerciseTitleLength = 48;

    public async Task<object> PersistAsync(
        GenerateLearningContentResponse generated,
        Guid userMessageId,
        string sourceMessage,
        CancellationToken cancellationToken)
    {
        return generated.Type switch
        {
            FlashcardType => await PersistFlashcardsAsync(
                (GeneratedFlashcardsDto)generated.GeneratedContent,
                userMessageId,
                sourceMessage,
                cancellationToken),
            TestType => await PersistTestAsync(
                (GeneratedTestDto)generated.GeneratedContent,
                userMessageId,
                sourceMessage,
                cancellationToken),
            _ => throw new ValidationException("Unsupported generated content type.")
        };
    }

    private async Task<FlashcardResponse> PersistFlashcardsAsync(
        GeneratedFlashcardsDto generated,
        Guid userMessageId,
        string sourceMessage,
        CancellationToken cancellationToken)
    {
        var flashcard = await flashcardService.CreateAsync(new Flashcard
        {
            Name = BuildExerciseName("Картки", sourceMessage),
            ChatMessageId = userMessageId,
            Cards = generated.Cards!
                .Select(card => new Card
                {
                    Term = card.Front!.Trim(),
                    Definition = card.Back!.Trim()
                })
                .ToList()
        }, cancellationToken);

        return flashcard.ToResponse();
    }

    private async Task<TestResponse> PersistTestAsync(
        GeneratedTestDto generated,
        Guid userMessageId,
        string sourceMessage,
        CancellationToken cancellationToken)
    {
        var test = await testService.CreateAsync(new StudyMentorApi.Data.Models.Test
        {
            Name = string.IsNullOrWhiteSpace(generated.Title)
                ? BuildExerciseName("Тест", sourceMessage)
                : generated.Title.Trim(),
            ChatMessageId = userMessageId,
            Questions = generated.Questions!
                .Select((question, questionIndex) => new TestQuestion
                {
                    Prompt = question.Text!.Trim(),
                    Order = questionIndex,
                    AnswerVariants = BuildAnswerVariants(question)
                })
                .ToList()
        }, cancellationToken);

        return new TestResponse(
            test.Id,
            test.Name,
            test.ChatMessageId,
            test.SourceFlashcardId,
            test.Questions
                .OrderBy(question => question.Order)
                .Select(question => new TestQuestionResponse(
                    question.Id,
                    question.Prompt,
                    question.AnswerVariants
                        .OrderBy(answer => answer.Order)
                        .Select(answer => new TestAnswerVariantResponse(answer.Id, answer.Text))
                        .ToList()))
                .ToList());
    }

    private static List<TestAnswerVariant> BuildAnswerVariants(GeneratedQuestionDto question)
    {
        var correctAnswer = question.CorrectAnswer!.Trim();
        var options = (question.Options ?? [])
            .Select(option => option.Trim())
            .Where(option => !string.IsNullOrWhiteSpace(option))
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .ToList();

        if (!options.Any(option => string.Equals(option, correctAnswer, StringComparison.OrdinalIgnoreCase)))
        {
            options.Insert(0, correctAnswer);
        }

        while (options.Count < 2)
        {
            options.Add($"Варіант {options.Count + 1}");
        }

        return options
            .Select((option, index) => new TestAnswerVariant
            {
                Text = option,
                IsCorrect = string.Equals(option, correctAnswer, StringComparison.OrdinalIgnoreCase),
                Order = index
            })
            .ToList();
    }

    private static string BuildExerciseName(string prefix, string sourceMessage)
    {
        var normalized = sourceMessage.Trim();
        var title = normalized.Length > MaxExerciseTitleLength ? normalized[..MaxExerciseTitleLength].Trim() + "..." : normalized;
        return $"{prefix}: {title}";
    }

}
