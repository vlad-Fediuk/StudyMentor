namespace StudyMentorApi.Data.Models;

public class Lecture : BaseEntity
{
    public required string Name { get; set; }

    public Guid SubjectId { get; set; }

    public Subject Subject { get; set; } = null!;

    public ICollection<ChatSession> ChatSessions { get; set; } = [];
}
