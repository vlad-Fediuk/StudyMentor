namespace StudyMentorApi.Data.Models;

public class Subject : BaseEntity
{
    public required string Name { get; set; } 

    public Guid MajorId { get; set; }

    public Major Major { get; set; } = null!;

    public ICollection<Lecture> Lectures { get; set; } = [];
}
