namespace StudyMentorApi.Data.Models;

public class TestAnswerVariant : BaseEntity
{
    public required string Text { get; set; }

    public bool IsCorrect { get; set; }

    public int Order { get; set; }

    public Guid TestQuestionId { get; set; }

    public TestQuestion TestQuestion { get; set; } = null!;
}
