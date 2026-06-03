using Microsoft.EntityFrameworkCore;
using StudyMentorApi.Data;
using StudyMentorApi.Data.Models;
using StudyMentorApi.Services;

namespace StudyMentorApi.Groups;

public class GroupService(AppDbContext dbContext) : BaseCrudService<Group>
{
    protected override IQueryable<Group> Query()
    {
        return dbContext.Groups.AsQueryable();
    }

    protected override async Task<Group?> FindByIdAsync(Guid id, CancellationToken cancellationToken)
    {
        return await dbContext.Groups.FirstOrDefaultAsync(g => g.Id == id, cancellationToken);
    }

    protected override async Task<Group> AddEntityAsync(Group entity, CancellationToken cancellationToken)
    {
        dbContext.Groups.Add(entity);
        await dbContext.SaveChangesAsync(cancellationToken);
        return entity;
    }

    protected override async Task<Group> SaveUpdatedEntityAsync(Group entity, CancellationToken cancellationToken)
    {
        await dbContext.SaveChangesAsync(cancellationToken);
        return entity;
    }

    protected override async Task DeleteEntityAsync(Group entity, CancellationToken cancellationToken)
    {
        dbContext.Groups.Remove(entity);
        await dbContext.SaveChangesAsync(cancellationToken);
    }

    protected override void UpdateEntityValues(Group existingEntity, Group updatedEntity)
    {
        existingEntity.Name = updatedEntity.Name;
    }
}
