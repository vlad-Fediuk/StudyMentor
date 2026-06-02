namespace StudyMentorApi.ChatSessions;

public record ChatSessionResponse(
    string Id,
    string UserId,
    string LectureId,
    string Title,
    string Topic,
    DateTime CreatedAt,
    DateTime UpdatedAt);
    
