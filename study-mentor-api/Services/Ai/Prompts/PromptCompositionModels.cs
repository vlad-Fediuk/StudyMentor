using StudyMentorApi.Services.Ai;

namespace StudyMentorApi.Services.Ai.Prompts;

public sealed record PromptCompositionRequest
{
    public required AiTaskType TaskType { get; init; }

    public required string UserMessage { get; init; }

    public IReadOnlyCollection<AiChatMessage> ConversationHistory { get; init; } = [];

    public string Context { get; init; } = string.Empty;

    public string UserProfile { get; init; } = string.Empty;

    public AiOutputFormat OutputFormat { get; init; } = AiOutputFormat.Text;

    public string? ResponseSchema { get; init; }
}

public sealed record ComposedPrompt(
    string Content,
    AiTaskType TaskType,
    AiOutputFormat OutputFormat);
