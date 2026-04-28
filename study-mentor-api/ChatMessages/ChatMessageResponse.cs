using StudyMentorApi.Data.Models;

namespace StudyMentorApi.ChatMessages;

public record ChatMessageResponse(
    string Id,
    string ChatSessionId,
    string Content,
    DateTime Timestamp,
    string? ExerciseId,
    MessageRole Role,
    int SequenceNumber);
