using StudyMentorApi.Common;
using StudyMentorApi.Lectures;
using StudyMentorApi.Services.Ai;
using StudyMentorApi.Services.Ai.StructuredOutput;

namespace StudyMentorApi.LearningContent;

public class LearningContentGenerationService(
    IAiGenerationService aiGenerationService,
    LectureService lectureService)
{
    private const string TestType = "test";
    private const string FlashcardType = "flashcard";

    private const string TestResponseSchema = """
        {
          "title": "string",
          "questions": [
            {
              "text": "string",
              "type": "single_choice",
              "options": ["string"],
              "correctAnswer": "string",
              "explanation": "string"
            }
          ]
        }
        """;

    private const string FlashcardResponseSchema = """
        {
          "cards": [
            {
              "front": "string",
              "back": "string"
            }
          ]
        }
        """;

    public async Task<GenerateLearningContentResponse> GenerateAsync(
        GenerateLearningContentRequest request,
        CancellationToken cancellationToken)
    {
        var type = NormalizeType(request.Type);
        Validate(request, type);

        var context = await BuildContextAsync(request, cancellationToken);

        return type switch
        {
            TestType => new GenerateLearningContentResponse(
                TestType,
                await GenerateTestAsync(request, context, cancellationToken)),
            FlashcardType => new GenerateLearningContentResponse(
                FlashcardType,
                await GenerateFlashcardsAsync(request, context, cancellationToken)),
            _ => throw new ValidationException("type must be either 'test' or 'flashcard'.")
        };
    }

    private async Task<GeneratedTestDto> GenerateTestAsync(
        GenerateLearningContentRequest request,
        string context,
        CancellationToken cancellationToken)
    {
        return await aiGenerationService.GenerateStructuredAsync<GeneratedTestDto>(
            new AiGenerationRequest
            {
                TaskType = AiTaskType.TestGeneration,
                UserMessage = BuildUserMessage(request, "questions"),
                Context = context,
                OutputFormat = AiOutputFormat.Json,
                ResponseSchema = TestResponseSchema
            },
            cancellationToken);
    }

    private async Task<GeneratedFlashcardsDto> GenerateFlashcardsAsync(
        GenerateLearningContentRequest request,
        string context,
        CancellationToken cancellationToken)
    {
        return await aiGenerationService.GenerateStructuredAsync<GeneratedFlashcardsDto>(
            new AiGenerationRequest
            {
                TaskType = AiTaskType.FlashcardGeneration,
                UserMessage = BuildUserMessage(request, "flashcards"),
                Context = context,
                OutputFormat = AiOutputFormat.Json,
                ResponseSchema = FlashcardResponseSchema
            },
            cancellationToken);
    }

    private async Task<string> BuildContextAsync(
        GenerateLearningContentRequest request,
        CancellationToken cancellationToken)
    {
        var contextParts = new List<string>();

        if (!string.IsNullOrWhiteSpace(request.Context))
        {
            contextParts.Add(request.Context.Trim());
        }

        if (request.LectureId is { } lectureId)
        {
            var lecture = await lectureService.GetByIdAsync(lectureId, cancellationToken);
            contextParts.Add($"""
                Lecture:
                Id: {lecture.Id}
                Name: {lecture.Name}
                SubjectId: {lecture.SubjectId}
                """);
        }

        return string.Join(Environment.NewLine + Environment.NewLine, contextParts);
    }

    private static string BuildUserMessage(
        GenerateLearningContentRequest request,
        string itemName)
    {
        return $"""
            {request.Message!.Trim()}

            Generate exactly {request.Count!.Value} {itemName}.
            """;
    }

    private static void Validate(GenerateLearningContentRequest request, string type)
    {
        if (string.IsNullOrWhiteSpace(type))
        {
            throw new ValidationException("type is required.");
        }

        if (type is not TestType and not FlashcardType)
        {
            throw new ValidationException("type must be either 'test' or 'flashcard'.");
        }

        if (string.IsNullOrWhiteSpace(request.Message))
        {
            throw new ValidationException("message is required.");
        }

        if (request.Count is null or <= 0)
        {
            throw new ValidationException("count must be greater than zero.");
        }
    }

    private static string NormalizeType(string? type)
    {
        var normalized = type?.Trim().ToLowerInvariant() ?? string.Empty;
        return normalized is "flashcards" or "card" or "cards"
            ? FlashcardType
            : normalized;
    }
}
