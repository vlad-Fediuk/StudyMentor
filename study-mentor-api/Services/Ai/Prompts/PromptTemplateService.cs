namespace StudyMentorApi.Services.Ai.Prompts;

public class PromptTemplateService(IWebHostEnvironment environment)
{
    private static readonly IReadOnlyDictionary<PromptType, string> TemplateFiles =
        new Dictionary<PromptType, string>
        {
            [PromptType.CHAT_ANSWER] = "chat-answer.md"
        };

    public async Task<string> BuildPromptAsync(
        PromptType type,
        PromptContext context,
        CancellationToken cancellationToken)
    {
        var template = await LoadTemplateAsync(type, cancellationToken);

        return template
            .Replace("{{user_message}}", Safe(context.UserMessage))
            .Replace("{{conversation_history}}", Safe(context.ConversationHistory))
            .Replace("{{retrieved_context}}", Safe(context.RetrievedContext))
            .Replace("{{user_profile}}", Safe(context.UserProfile))
            .Replace("{{user_memory}}", Safe(context.UserMemory))
            .Replace("{{response_style}}", Safe(context.ResponseStyle, "глибокий аналіз дозволеного контексту"))
            .Replace("{{language}}", Safe(context.Language, "українська"))
            .Replace("{{answer_rules}}", Safe(context.AnswerRules));
    }

    private async Task<string> LoadTemplateAsync(
        PromptType type,
        CancellationToken cancellationToken)
    {
        if (!TemplateFiles.TryGetValue(type, out var fileName))
        {
            throw new InvalidOperationException($"Prompt template for type '{type}' is not configured.");
        }

        var path = Path.Combine(
            environment.ContentRootPath,
            "Services",
            "Ai",
            "Prompts",
            "Templates",
            fileName);

        if (!File.Exists(path))
        {
            throw new FileNotFoundException($"Prompt template file was not found: {path}");
        }

        return await File.ReadAllTextAsync(path, cancellationToken);
    }

    private static string Safe(string? value, string defaultValue = "") =>
        string.IsNullOrWhiteSpace(value) ? defaultValue : value.Trim();
}
