namespace StudyMentorApi.Data.Models;

public class AiModel : BaseEntity
{
    public Guid ProviderId { get; set; }

    public AiProvider Provider { get; set; } = null!;

    public required string ModelName { get; set; }

    public required string DisplayName { get; set; }

    public bool IsEnabled { get; set; } = true;

    public int Priority { get; set; }

    public double Temperature { get; set; } = 1.0;

    public double TopP { get; set; } = 1.0;

    public int MaxOutputTokens { get; set; } = 2048;

    public int? ReasoningBudget { get; set; }

    public bool EnableThinking { get; set; }

    public string? CapabilitiesJson { get; set; }

    public string? SettingsJson { get; set; }
}
