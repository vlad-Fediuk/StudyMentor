using System.Text;
using StudyMentorApi.Services.Ai;

namespace StudyMentorApi.Services.Ai.Prompts;

public sealed class PromptBuilder
{
    private string _systemPrompt = string.Empty;
    private string _userMessage = string.Empty;
    private string _conversationHistory = string.Empty;
    private string _context = string.Empty;
    private string _personalization = string.Empty;
    private string _taskRules = string.Empty;
    private string _outputFormat = string.Empty;
    private AiTaskType _taskType = AiTaskType.ChatAnswer;
    private AiOutputFormat _format = AiOutputFormat.Text;

    public PromptBuilder WithSystemPrompt(string value)
    {
        _systemPrompt = value;
        return this;
    }

    public PromptBuilder WithUserMessage(string value)
    {
        _userMessage = value;
        return this;
    }

    public PromptBuilder WithConversationHistory(string value)
    {
        _conversationHistory = value;
        return this;
    }

    public PromptBuilder WithContext(string value)
    {
        _context = value;
        return this;
    }

    public PromptBuilder WithPersonalization(string value)
    {
        _personalization = value;
        return this;
    }

    public PromptBuilder WithTaskRules(string value)
    {
        _taskRules = value;
        return this;
    }

    public PromptBuilder WithOutputFormat(string value)
    {
        _outputFormat = value;
        return this;
    }

    public PromptBuilder WithMetadata(AiTaskType taskType, AiOutputFormat format)
    {
        _taskType = taskType;
        _format = format;
        return this;
    }

    public ComposedPrompt Build()
    {
        var content = new StringBuilder();

        AppendBlock(content, "System prompt", _systemPrompt);
        AppendBlock(content, "Conversation history", _conversationHistory);
        AppendBlock(content, "Context", _context);
        AppendBlock(content, "Personalization", _personalization);
        AppendBlock(content, "Task rules", _taskRules);
        AppendBlock(content, "Output format", _outputFormat);
        AppendBlock(content, "User message", _userMessage);

        return new ComposedPrompt(content.ToString().Trim(), _taskType, _format);
    }

    private static void AppendBlock(StringBuilder content, string title, string value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            return;
        }

        if (content.Length > 0)
        {
            content.AppendLine();
            content.AppendLine();
        }

        content.AppendLine($"{title}:");
        content.AppendLine(value.Trim());
    }
}
