namespace StudyMentorApi.AiChat;

public record AiChatSendMessageResponse(
    string Status,
    AiChatMessageDto UserMessage,
    AiChatMessageDto AssistantMessage);
