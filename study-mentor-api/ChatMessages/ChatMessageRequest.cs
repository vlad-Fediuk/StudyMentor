using StudyMentorApi.Data.Models;

namespace StudyMentorApi.ChatMessages;

public record ChatMessageRequest(
    string ChatSessionId,
    string Content,
    DateTime? Timestamp,
    MessageRole Role,
    int SequenceNumber);
