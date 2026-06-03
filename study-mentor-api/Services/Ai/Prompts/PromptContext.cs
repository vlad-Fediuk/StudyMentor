namespace StudyMentorApi.Services.Ai.Prompts;

public record PromptContext(
    string? UserMessage,
    string? ConversationHistory,
    string? RetrievedContext,
    string? UserProfile,
    string? UserMemory,
    string? ResponseStyle,
    string? Language,
    string? AnswerRules);
