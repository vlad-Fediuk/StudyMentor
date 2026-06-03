using StudyMentorApi.Data.Models;

namespace StudyMentorApi.ChatMessages;

public record ChatMessageRequest(
    Guid ChatSessionId,
    string Content,
    DateTime? Timestamp,
    MessageRole Role,
    int SequenceNumber);
