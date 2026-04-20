namespace study_mentor_api.Data.Models;

public class Lecture
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public int SubjectId { get; set; }
    public Subject Subject { get; set; } = null!;
}