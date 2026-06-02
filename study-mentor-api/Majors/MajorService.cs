using Microsoft.EntityFrameworkCore;
using StudyMentorApi.Data;
using StudyMentorApi.Data.Models;
using StudyMentorApi.Services;

namespace StudyMentorApi.Majors;

public class MajorService(AppDbContext dbContext) : BaseCrudService<Major>
{
    protected override IQueryable<Major> Query()
    {
        return dbContext.Majors.AsQueryable();
    }

    protected override async Task<Major?> FindByIdAsync(Guid id, CancellationToken cancellationToken)
    {
        return await dbContext.Majors.FirstOrDefaultAsync(m => m.Id == id, cancellationToken);
    }

    protected override async Task<Major> AddEntityAsync(Major entity, CancellationToken cancellationToken)
    {
        dbContext.Majors.Add(entity);
        await dbContext.SaveChangesAsync(cancellationToken);
        return entity;
    }

    protected override async Task<Major> SaveUpdatedEntityAsync(Major entity, CancellationToken cancellationToken)
    {
        await dbContext.SaveChangesAsync(cancellationToken);
        return entity;
    }

    protected override async Task DeleteEntityAsync(Major entity, CancellationToken cancellationToken)
    {
        dbContext.Majors.Remove(entity);
        await dbContext.SaveChangesAsync(cancellationToken);
    }

    protected override void UpdateEntityValues(Major existingEntity, Major updatedEntity)
    {
        existingEntity.Name = updatedEntity.Name;
    }
}
