namespace StudyMentorApi.Services.Ai;

public sealed record AiProviderRequest
{
    public required string ProviderName { get; init; }

    public required string ProviderType { get; init; }

    public required string BaseUrl { get; init; }

    public string? ApiKeyEnvironmentVariable { get; init; }

    public required string Model { get; init; }

    public IReadOnlyCollection<AiChatMessage> Messages { get; init; } =
        Array.Empty<AiChatMessage>();

    public int MaxOutputTokens { get; init; }

    public int? ReasoningBudget { get; init; }

    public int TimeoutSeconds { get; init; }

    public double Temperature { get; init; }

    public double TopP { get; init; }

    public bool EnableThinking { get; init; }
}

public sealed record AiProviderResponse(
    string Provider,
    string Model,
    string Content);
