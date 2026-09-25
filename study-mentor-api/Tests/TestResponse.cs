namespace StudyMentorApi.Tests;

public record TestResponse(
    Guid Id,
    string Name,
    Guid ChatMessageId,
    Guid? SourceFlashcardId,
    IReadOnlyCollection<TestQuestionResponse> Questions);
