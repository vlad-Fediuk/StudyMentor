using StudyMentorApi.Data.Models;

namespace StudyMentorApi.ChatMessages;

public record ChatMessageResponse(
    Guid Id,
    Guid ChatSessionId,
    string Content,
    DateTime Timestamp,
    MessageRole Role,
    int SequenceNumber,
    string Status);
