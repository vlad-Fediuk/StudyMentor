namespace StudyMentorApi.ChatSessions;

public record ChatSessionResponse(
    Guid Id,
    Guid UserId,
    Guid LectureId);
    
