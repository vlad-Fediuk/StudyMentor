namespace StudyMentorApi.Extensions;

public sealed class NvidiaAiSettings
{
    public const string SectionName = "AiProviders:Nvidia";

    public string InvokeUrl { get; set; } =
        "https://integrate.api.nvidia.com/v1/chat/completions";

    public string ApiKey { get; set; } = string.Empty;

    public string DefaultModel { get; set; } = "nvidia/nemotron-3-super-120b-a12b";

    public string[] SupportedModels { get; set; } =
    [
        "nvidia/nemotron-3-super-120b-a12b"
    ];

    public int MaxOutputTokens { get; set; } = 16384;

    public int ReasoningBudget { get; set; } = 16384;

    public double Temperature { get; set; } = 1.0;

    public double TopP { get; set; } = 0.95;

    public bool EnableThinking { get; set; } = true;
}
