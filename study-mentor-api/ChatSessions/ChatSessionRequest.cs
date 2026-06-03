namespace StudyMentorApi.ChatSessions;

public record ChatSessionRequest(
    Guid UserId,
    Guid LectureId);
    
