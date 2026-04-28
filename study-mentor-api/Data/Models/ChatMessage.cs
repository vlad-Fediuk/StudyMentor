using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace StudyMentorApi.Data.Models;

public class ChatMessage : BaseEntity<string>
{
    [BsonElement("chat_session_id")]
    public required string ChatSessionId { get; set; }

    [BsonElement("content")]
    public required string Content { get; set; }

    [BsonElement("timestamp")]
    public DateTime Timestamp { get; set; }

    [BsonElement("exercise_id")]
    public string? ExerciseId { get; set; }

    [BsonElement("role")]
    [BsonRepresentation(BsonType.String)]
    public MessageRole Role { get; set; }

    [BsonElement("sequence_number")]
    public int SequenceNumber { get; set; }
}

public enum MessageRole
{
    User,
    Assistant,
    System
}