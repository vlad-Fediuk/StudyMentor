using MongoDB.Driver;
using StudyMentorApi.Data.Models;
using StudyMentorApi.Services;
using StudyMentorApi.Subjects;

namespace StudyMentorApi.Lectures;

public class LectureService : BaseCrudService<Lecture, string>
{
    private const string CollectionName = "lectures";
    private readonly IMongoCollection<Lecture> _collection;
    private readonly SubjectService _subjectService;

    public LectureService(MongoDbService dbService, SubjectService subjectService)
    {
        _collection = dbService.GetCollection<Lecture>(CollectionName);
        _subjectService = subjectService;
    }

    protected override IQueryable<Lecture> Query()
    {
        return _collection.AsQueryable();
    }

    protected override async Task<Lecture?> FindByIdAsync(string id, CancellationToken cancellationToken)
    {
        return await _collection
            .Find(l => l.Id == id)
            .FirstOrDefaultAsync(cancellationToken);
    }

    protected override async Task<Lecture> AddEntityAsync(Lecture entity, CancellationToken cancellationToken)
    {
        await _collection.InsertOneAsync(entity, cancellationToken: cancellationToken);
        return entity;
    }

    protected override async Task<Lecture> SaveUpdatedEntityAsync(Lecture entity, CancellationToken cancellationToken)
    {
        await _collection.ReplaceOneAsync(
            l => l.Id == entity.Id,
            entity,
            cancellationToken: cancellationToken);
        return entity;
    }

    protected override async Task DeleteEntityAsync(Lecture entity, CancellationToken cancellationToken)
    {
        await _collection.DeleteOneAsync(l => l.Id == entity.Id, cancellationToken);
    }

    protected override void UpdateEntityValues(Lecture existingEntity, Lecture updatedEntity)
    {
        existingEntity.Name = updatedEntity.Name;
        existingEntity.SubjectId = updatedEntity.SubjectId;
    }

    protected override async Task ValidateCreateAsync(Lecture entity, CancellationToken cancellationToken)
    {
        await _subjectService.GetByIdAsync(entity.SubjectId, cancellationToken);
    }

    protected override async Task ValidateUpdateAsync(
        Lecture existingEntity,
        Lecture updatedEntity,
        CancellationToken cancellationToken)
    {
        await _subjectService.GetByIdAsync(updatedEntity.SubjectId, cancellationToken);
    }
}
