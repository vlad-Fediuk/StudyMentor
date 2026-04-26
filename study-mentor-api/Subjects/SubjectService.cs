using MongoDB.Driver;
using study_mentor_api.Data.Models;
using study_mentor_api.Services;
namespace study_mentor_api.Subjects;

public class SubjectService : BaseCrudService<Subject, string>
{
    private const string CollectionName = "subjects";
    private readonly IMongoCollection<Subject> _collection;

    public SubjectService(MongoDbService dbService)
    {
        _collection = dbService.GetCollection<Subject>(CollectionName);
    }

    protected override IQueryable<Subject> Query()
    {
        return _collection.AsQueryable();
    }

    protected override async Task<Subject?> FindByIdAsync(string id, CancellationToken cancellationToken)
    {
        return await _collection
            .Find(s => s.Id == id)
            .FirstOrDefaultAsync(cancellationToken);
    }

    protected override async Task<Subject> AddEntityAsync(Subject entity, CancellationToken cancellationToken)
    {
        await _collection.InsertOneAsync(entity, cancellationToken: cancellationToken);
        return entity;
    }

    protected override async Task<Subject> SaveUpdatedEntityAsync(Subject entity, CancellationToken cancellationToken)
    {
        await _collection.ReplaceOneAsync(
            s => s.Id == entity.Id,
            entity,
            cancellationToken: cancellationToken);
        return entity;
    }

    protected override async Task DeleteEntityAsync(Subject entity, CancellationToken cancellationToken)
    {
        await _collection.DeleteOneAsync(s => s.Id == entity.Id, cancellationToken);
    }

    protected override void UpdateEntityValues(Subject existingEntity, Subject updatedEntity)
    {
        existingEntity.Name = updatedEntity.Name;
        existingEntity.Major = updatedEntity.Major;
    }
}