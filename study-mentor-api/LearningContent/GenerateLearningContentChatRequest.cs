namespace StudyMentorApi.LearningContent;

public record GenerateLearningContentChatRequest(
    string? Type,
    string? Message,
    int? Count,
    string? ChatId,
    Guid? LectureId,
    string? Context,
    bool ExplicitGeneration = false);
