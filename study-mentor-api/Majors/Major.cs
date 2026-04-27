using MongoDB.Bson.Serialization.Attributes;
using StudyMentorApi.Data.Models;

namespace StudyMentorApi.Majors;

public class Major : BaseEntity<string>
{
    [BsonElement("major_name")]
    public required string Name { get; set; }
}
