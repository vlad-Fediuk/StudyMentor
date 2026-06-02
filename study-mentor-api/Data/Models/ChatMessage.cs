namespace StudyMentorApi.Data.Models;

public class ChatMessage : BaseEntity
{
    public Guid ChatSessionId { get; set; }

    public required string Content { get; set; }

    public DateTime Timestamp { get; set; }

    public MessageRole Role { get; set; }

    public int SequenceNumber { get; set; }

    public ICollection<Exercise> Exercises { get; set; } = [];
}

public enum MessageRole
{
    User,
    Assistant,
    System
}
