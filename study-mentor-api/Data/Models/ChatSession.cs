using MongoDB.Bson.Serialization.Attributes;

namespace StudyMentorApi.Data.Models;

public class ChatSession : BaseEntity<string>
{
    [BsonElement("user_id")]
    public required string UserId { get; set; }

    [BsonElement("lecture_id")]
    public required string LectureId { get; set; }

    [BsonElement("title")]
    public string Title { get; set; } = "New chat";

    [BsonElement("topic")]
    public string Topic { get; set; } = "General learning";

    [BsonElement("created_at")]
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    [BsonElement("updated_at")]
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
}
