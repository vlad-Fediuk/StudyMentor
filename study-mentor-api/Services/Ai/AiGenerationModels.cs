namespace StudyMentorApi.Services.Ai;

public sealed record AiGenerationRequest
{
    public required AiTaskType TaskType { get; init; }

    public required string UserMessage { get; init; }

    public IReadOnlyCollection<AiChatMessage> ConversationHistory { get; init; } = [];

    public string Context { get; init; } = string.Empty;

    public string UserProfile { get; init; } = string.Empty;

    public string? PreferredProvider { get; init; }

    public string? PreferredModel { get; init; }

    public AiOutputFormat OutputFormat { get; init; } = AiOutputFormat.Text;

    public string? ResponseSchema { get; init; }

    public AiGenerationOptions? Options { get; init; }
}

public sealed record AiGenerationResponse(
    string Content,
    string Provider,
    string Model,
    bool FallbackUsed,
    AiTaskType TaskType);

public enum AiTaskType
{
    ChatAnswer,
    TestGeneration,
    FlashcardGeneration,
    LectureSummary,
    ExerciseGeneration
}

public enum AiOutputFormat
{
    Text,
    Json
}

public sealed record AiGenerationOptions(
    double? Temperature,
    double? TopP,
    int? MaxOutputTokens,
    int? ReasoningBudget,
    bool? EnableThinking);
