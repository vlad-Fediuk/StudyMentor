using MongoDB.Bson.Serialization.Attributes;

namespace StudyMentorApi.Data.Models;

public class ChatSession : BaseEntity<string>
{
    [BsonElement("user_id")]
    public required string UserId { get; set; }

    [BsonElement("lecture_id")]
    public required string LectureId { get; set; }
}
