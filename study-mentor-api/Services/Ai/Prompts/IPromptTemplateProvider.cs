using StudyMentorApi.Services.Ai;

namespace StudyMentorApi.Services.Ai.Prompts;

public interface IPromptTemplateProvider
{
    Task<string?> GetActiveTemplateAsync(
        AiTaskType taskType,
        string key,
        string? version = null,
        string? language = null,
        CancellationToken cancellationToken = default);
}
