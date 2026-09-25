namespace StudyMentorApi.Data.Models;

public class TestQuestion : BaseEntity
{
    public required string Prompt { get; set; }

    public int Order { get; set; }

    public Guid TestId { get; set; }

    public Test Test { get; set; } = null!;

    public List<TestAnswerVariant> AnswerVariants { get; set; } = [];
}
