namespace StudyMentorApi.LectureChunks;

public record LectureChunkRequest(Guid SubjectId, Guid LectureId, IFormFile? File);
