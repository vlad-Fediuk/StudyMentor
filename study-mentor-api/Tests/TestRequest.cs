namespace StudyMentorApi.Tests;

public record TestRequest(
    string? Name,
    Guid ChatMessageId,
    Guid? SourceFlashcardId,
    IReadOnlyCollection<TestQuestionRequest>? Questions);
