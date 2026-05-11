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
}