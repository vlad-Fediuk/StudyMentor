namespace StudyMentorApi.Data.Models;

public class User : BaseEntity
{
    public required string Name { get; set; }

    public string? Email { get; set; }

    public required string Password { get; set; }

    public Guid GroupId { get; set; }

    public ICollection<string> Roles { get; set; } = ["User"];

    public ICollection<ChatSession> ChatSessions { get; set; } = [];
}
