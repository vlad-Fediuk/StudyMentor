namespace StudyMentorApi.ChatSessions;

public record ChatSessionResponse(
    string Id,
    string UserId,
    string LectureId);