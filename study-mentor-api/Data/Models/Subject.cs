using MongoDB.Bson.Serialization.Attributes;

namespace StudyMentorApi.Data.Models;

public class Subject : BaseEntity<string>
{
    [BsonElement("subject_name")]
    public required string Name { get; set; } 
    
    [BsonElement("subject_major")]
    public required Major Major { get; set; }
}
