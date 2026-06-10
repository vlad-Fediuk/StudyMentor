using StudyMentorApi.AiChat;

namespace StudyMentorApi.LearningContent;

public record GenerateLearningContentChatResponse(
    string Status,
    string Type,
    AiChatMessageDto UserMessage,
    AiChatMessageDto SystemMessage,
    object? CreatedContent);
