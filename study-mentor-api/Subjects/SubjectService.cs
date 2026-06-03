using Microsoft.EntityFrameworkCore;
using StudyMentorApi.Data;
using StudyMentorApi.Data.Models;
using StudyMentorApi.Majors;
using StudyMentorApi.Services;

namespace StudyMentorApi.Subjects;

public class SubjectService : BaseCrudService<Subject>
{
    private readonly AppDbContext _dbContext;
    private readonly MajorService _majorService;

    public SubjectService(AppDbContext dbContext, MajorService majorService)
    {
        _dbContext = dbContext;
        _majorService = majorService;
    }

    protected override IQueryable<Subject> Query()
    {
        return _dbContext.Subjects.AsQueryable();
    }

    protected override async Task<Subject?> FindByIdAsync(Guid id, CancellationToken cancellationToken)
    {
        return await _dbContext.Subjects.FirstOrDefaultAsync(s => s.Id == id, cancellationToken);
    }

    protected override async Task<Subject> AddEntityAsync(Subject entity, CancellationToken cancellationToken)
    {
        _dbContext.Subjects.Add(entity);
        await _dbContext.SaveChangesAsync(cancellationToken);
        return entity;
    }

    protected override async Task<Subject> SaveUpdatedEntityAsync(Subject entity, CancellationToken cancellationToken)
    {
        await _dbContext.SaveChangesAsync(cancellationToken);
        return entity;
    }

    protected override async Task DeleteEntityAsync(Subject entity, CancellationToken cancellationToken)
    {
        _dbContext.Subjects.Remove(entity);
        await _dbContext.SaveChangesAsync(cancellationToken);
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
