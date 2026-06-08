using Microsoft.EntityFrameworkCore;
using StudyMentorApi.Data;
using StudyMentorApi.Services.Ai;

namespace StudyMentorApi.Services.Ai.Prompts;

public sealed class PromptTemplateProvider(AppDbContext dbContext) : IPromptTemplateProvider
{
    public async Task<string?> GetActiveTemplateAsync(
        AiTaskType taskType,
        string key,
        string? version = null,
        string? language = null,
        CancellationToken cancellationToken = default)
    {
        var query = dbContext.PromptTemplates
            .AsNoTracking()
            .Where(template =>
                template.IsActive
                && template.TaskType == taskType
                && template.Key == key);

        if (!string.IsNullOrWhiteSpace(version))
        {
            query = query.Where(template => template.Version == version);
        }

        if (!string.IsNullOrWhiteSpace(language))
        {
            query = query.Where(template =>
                template.Language == language || template.Language == null);
        }

        var template = await query
            .OrderByDescending(template => template.Language == language)
            .ThenByDescending(template => template.CreatedAt)
            .ThenByDescending(template => template.Version)
            .FirstOrDefaultAsync(cancellationToken);

        return template?.Template;
    }
}
