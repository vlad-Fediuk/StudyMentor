namespace StudyMentorApi.Data.Models;

public class ChatSession : BaseEntity
{
    public Guid UserId { get; set; }

    public User User { get; set; } = null!;

    public Guid LectureId { get; set; }

    public Lecture Lecture { get; set; } = null!;

    public ICollection<ChatMessage> Messages { get; set; } = [];
}
