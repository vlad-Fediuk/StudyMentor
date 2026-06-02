namespace StudyMentorApi.Data.Models;

public class User : BaseEntity
{
    public required string Name { get; set; }

    public required string Password { get; set; }

    public Guid GroupId { get; set; }
}
