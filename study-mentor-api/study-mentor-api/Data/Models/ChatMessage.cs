namespace study_mentor_api.Data.Models;

public class ChatMessage
{
    public int Id { get; set; }
    public int ChatSessionId { get; set; }
    public ChatSession ChatSession { get; set; } = null!;
    public string Content { get; set; } = string.Empty;
    public DateTime Timestamp { get; set; }
    public int? ExerciseId { get; set; }
    public int? MessageParentId { get; set; }
    public ChatMessage? MessageParent { get; set; }
}