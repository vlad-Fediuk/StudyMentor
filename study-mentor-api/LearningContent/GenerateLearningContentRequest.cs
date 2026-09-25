namespace StudyMentorApi.LearningContent;

public record GenerateLearningContentRequest(
    string? Type,
    string? Message,
    int? Count,
    Guid? LectureId,
    string? Context);
