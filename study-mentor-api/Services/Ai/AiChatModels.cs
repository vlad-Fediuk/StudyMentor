namespace StudyMentorApi.Services.Ai;

public sealed record AiChatMessage(
    string Role,
    string Content);

public sealed record AiChatRequest
{
    public string? Model { get; init; }

    public IReadOnlyCollection<AiChatMessage> Messages { get; init; } =
        Array.Empty<AiChatMessage>();
}

public sealed record AiChatResponse(
    string Provider,
    string Model,
    string Content);
