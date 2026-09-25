namespace StudyMentorApi.Tests;

public record TestQuestionRequest(
    string? Prompt,
    IReadOnlyCollection<TestAnswerVariantRequest>? AnswerVariants);
