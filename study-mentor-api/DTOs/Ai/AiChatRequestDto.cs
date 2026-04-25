namespace StudyMentorApi.DTOs.Ai;

public sealed record AiChatRequestDto
{
    public string? Model { get; init; }

    public string Message { get; init; } = string.Empty;

    public string? SystemPrompt { get; init; }
}
