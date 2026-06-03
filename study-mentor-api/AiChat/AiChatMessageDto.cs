namespace StudyMentorApi.AiChat;

public record AiChatMessageDto(
    string Id,
    string ChatId,
    string Role,
    string Content,
    DateTime CreatedAt,
    string Status);
