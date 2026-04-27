using MongoDB.Bson.Serialization.Attributes;

namespace StudyMentorApi.Data.Models;

public class Major : BaseEntity<string>
{
    [BsonElement("major_name")]
    public required string Name { get; set; }
}
