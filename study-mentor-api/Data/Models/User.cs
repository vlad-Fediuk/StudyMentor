using MongoDB.Bson.Serialization.Attributes;

namespace StudyMentorApi.Data.Models;

public class User : BaseEntity<string>
{
    [BsonElement("name")]
    public required string Name { get; set; }

    [BsonElement("email")]
    public required string Email { get; set; }

    [BsonElement("password_hash")]
    public required string PasswordHash { get; set; }

    [BsonElement("group_id")]
    public required string GroupId { get; set; }
}
