using MongoDB.Bson.Serialization.Attributes;

namespace StudyMentorApi.Data.Models;

public class Lecture : BaseEntity<string>
{
    [BsonElement("lecture_name")]
    public required string Name { get; set; }

    [BsonElement("lecture_subject_id")]
    public required string SubjectId { get; set; }
}
