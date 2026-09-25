namespace StudyMentorApi.Tests;

public record TestQuestionResponse(
    Guid Id,
    string Prompt,
    IReadOnlyCollection<TestAnswerVariantResponse> AnswerVariants);
