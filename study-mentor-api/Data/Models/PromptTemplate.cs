using StudyMentorApi.Services.Ai;

namespace StudyMentorApi.Data.Models;

public class PromptTemplate : BaseEntity
{
    public required string Key { get; set; }

    public AiTaskType TaskType { get; set; }

    public required string Version { get; set; }

    public required string Template { get; set; }

    public bool IsActive { get; set; } = true;

    public string? Language { get; set; }

    public DateTime CreatedAt { get; set; }
}
