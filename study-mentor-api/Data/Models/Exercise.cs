using MongoDB.Bson.Serialization.Attributes;
namespace StudyMentorApi.Data.Models;

public abstract class Exercise : BaseEntity<string>
{
    [BsonElement("exercise_name")]
    public required string Name { get; set; }
    
    [BsonElement("exercise_message_id")]
    public required string MessageID { get; set; }
}
