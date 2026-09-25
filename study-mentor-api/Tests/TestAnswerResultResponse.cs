namespace StudyMentorApi.Tests;

public record TestAnswerResultResponse(
    Guid QuestionId,
    Guid SelectedAnswerVariantId,
    Guid CorrectAnswerVariantId,
    bool IsCorrect);
