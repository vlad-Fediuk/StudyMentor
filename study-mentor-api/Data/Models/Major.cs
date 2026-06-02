namespace StudyMentorApi.Data.Models;

public class Major : BaseEntity
{
    public required string Name { get; set; }

    public ICollection<Subject> Subjects { get; set; } = [];
}
