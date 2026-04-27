using MongoDB.Driver;
using StudyMentorApi.Data.Models;
using StudyMentorApi.Majors;
using StudyMentorApi.Services;

namespace StudyMentorApi.Subjects;

public class SubjectService : BaseCrudService<Subject, string>
{
    private const string CollectionName = "subjects";
    private readonly IMongoCollection<Subject> _collection;
    private readonly MajorService _majorService;

    public SubjectService(MongoDbService dbService, MajorService majorService)
    {
        _collection = dbService.GetCollection<Subject>(CollectionName);
        _majorService = majorService;
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
        existingEntity.MajorId = updatedEntity.MajorId;
    }

    protected override async Task ValidateCreateAsync(Subject entity, CancellationToken cancellationToken)
    {
        await _majorService.GetByIdAsync(entity.MajorId, cancellationToken);
    }

    protected override async Task ValidateUpdateAsync(
        Subject existingEntity,
        Subject updatedEntity,
        CancellationToken cancellationToken)
    {
        await _majorService.GetByIdAsync(updatedEntity.MajorId, cancellationToken);
    }
}
