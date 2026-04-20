namespace study_mentor_api.Data.Models;

public class ChatSession
{
    public int Id { get; set; }
    public int UserId { get; set; }
    public User User { get; set; } = null!;
    public int LectureId { get; set; }
    public Lecture Lecture { get; set; } = null!;
}