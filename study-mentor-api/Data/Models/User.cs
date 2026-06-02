using MongoDB.Bson.Serialization.Attributes;

namespace StudyMentorApi.Data.Models;

public class User : BaseEntity<string>
{
    [BsonElement("name")]
    public required string Name { get; set; }

    [BsonElement("password")]
    public required string Password { get; set; }

    [BsonElement("group_id")]
    public required string GroupId { get; set; }

    [BsonElement("learning_level")]
    public string LearningLevel { get; set; } = "beginner";

    [BsonElement("preferred_language")]
    public string PreferredLanguage { get; set; } = "uk";

    [BsonElement("current_progress")]
    public string CurrentProgress { get; set; } = "No progress data yet.";

    [BsonElement("created_at")]
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}
