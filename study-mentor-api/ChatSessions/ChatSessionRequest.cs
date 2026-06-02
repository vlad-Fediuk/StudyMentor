namespace StudyMentorApi.ChatSessions;

public record ChatSessionRequest(
    string UserId,
    string LectureId,
    string? Title,
    string? Topic);
    
