using MongoDB.Bson.Serialization.Attributes;
using MongoDB.Bson;
namespace study_mentor_api.Data.Models;

public abstract class BaseEntity<TKey> : IEntity<TKey>  where TKey : notnull
{
    [BsonId]
    [BsonRepresentation(BsonType.ObjectId)]
    public TKey Id { get; protected set; } = default!;
}
