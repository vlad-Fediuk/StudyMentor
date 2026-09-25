namespace StudyMentorApi.Services.Ai.StructuredOutput;

public sealed record GeneratedTestDto
{
    public string? Title { get; init; }

    public IReadOnlyCollection<GeneratedQuestionDto>? Questions { get; init; }
}

public sealed record GeneratedQuestionDto
{
    public string? Text { get; init; }

    public string? Type { get; init; }

    public IReadOnlyCollection<string>? Options { get; init; }

    public string? CorrectAnswer { get; init; }

    public string? Explanation { get; init; }
}

public sealed record GeneratedFlashcardsDto
{
    public IReadOnlyCollection<GeneratedFlashcardDto>? Cards { get; init; }
}

public sealed record GeneratedFlashcardDto
{
    public string? Front { get; init; }

    public string? Back { get; init; }
}
