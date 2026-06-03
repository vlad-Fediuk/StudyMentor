using Microsoft.EntityFrameworkCore;
using StudyMentorApi.Data;
using StudyMentorApi.Data.Models;
using StudyMentorApi.Services;
using StudyMentorApi.Subjects;

namespace StudyMentorApi.Lectures;

public class LectureService : BaseCrudService<Lecture>
{
    private readonly AppDbContext _dbContext;
    private readonly SubjectService _subjectService;

    public LectureService(AppDbContext dbContext, SubjectService subjectService)
    {
        _dbContext = dbContext;
        _subjectService = subjectService;
    }

    protected override IQueryable<Lecture> Query()
    {
        return _dbContext.Lectures.AsQueryable();
    }

    protected override async Task<Lecture?> FindByIdAsync(Guid id, CancellationToken cancellationToken)
    {
        return await _dbContext.Lectures.FirstOrDefaultAsync(l => l.Id == id, cancellationToken);
    }

    protected override async Task<Lecture> AddEntityAsync(Lecture entity, CancellationToken cancellationToken)
    {
        _dbContext.Lectures.Add(entity);
        await _dbContext.SaveChangesAsync(cancellationToken);
        return entity;
    }

    protected override async Task<Lecture> SaveUpdatedEntityAsync(Lecture entity, CancellationToken cancellationToken)
    {
        await _dbContext.SaveChangesAsync(cancellationToken);
        return entity;
    }

    protected override async Task DeleteEntityAsync(Lecture entity, CancellationToken cancellationToken)
    {
        _dbContext.Lectures.Remove(entity);
        await _dbContext.SaveChangesAsync(cancellationToken);
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
