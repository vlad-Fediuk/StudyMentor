namespace StudyMentorApi.LectureChunks;

public record LectureChunkResponse(Guid Id, string Content, int Order, Guid LectureId);
