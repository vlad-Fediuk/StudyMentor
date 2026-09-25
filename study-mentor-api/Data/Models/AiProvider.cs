namespace StudyMentorApi.Data.Models;

public class AiProvider : BaseEntity
{
    public required string Name { get; set; }

    public required string Type { get; set; }

    public required string BaseUrl { get; set; }

    public string? ApiKeyEnvironmentVariable { get; set; }

    public bool IsEnabled { get; set; } = true;

    public int Priority { get; set; }

    public int TimeoutSeconds { get; set; } = 300;

    public string? SettingsJson { get; set; }

    public ICollection<AiModel> Models { get; set; } = [];
}
