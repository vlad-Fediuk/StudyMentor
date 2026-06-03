namespace StudyMentorApi.Data.Models;

public class Exercise : BaseEntity
{
    public required string Name { get; set; }

    public Guid ChatMessageId { get; set; }

    public ChatMessage ChatMessage { get; set; } = null!;
}
