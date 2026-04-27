using MongoDB.Driver;
using StudyMentorApi.Services;

namespace StudyMentorApi.Majors;

public class MajorService : BaseCrudService<Major, string>
{
    private const string CollectionName = "majors";
    private readonly IMongoCollection<Major> _collection;

    public MajorService(MongoDbService dbService)
    {
        _collection = dbService.GetCollection<Major>(CollectionName);
    }

    protected override IQueryable<Major> Query()
    {
        return _collection.AsQueryable();
    }

    protected override async Task<Major?> FindByIdAsync(string id, CancellationToken cancellationToken)
    {
        return await _collection
            .Find(m => m.Id == id)
            .FirstOrDefaultAsync(cancellationToken);
    }

    protected override async Task<Major> AddEntityAsync(Major entity, CancellationToken cancellationToken)
    {
        await _collection.InsertOneAsync(entity, cancellationToken: cancellationToken);
        return entity;
    }

    protected override async Task<Major> SaveUpdatedEntityAsync(Major entity, CancellationToken cancellationToken)
    {
        await _collection.ReplaceOneAsync(
            m => m.Id == entity.Id,
            entity,
            cancellationToken: cancellationToken);
        return entity;
    }

    protected override async Task DeleteEntityAsync(Major entity, CancellationToken cancellationToken)
    {
        await _collection.DeleteOneAsync(m => m.Id == entity.Id, cancellationToken);
    }

    protected override void UpdateEntityValues(Major existingEntity, Major updatedEntity)
    {
        existingEntity.Name = updatedEntity.Name;
    }
}
