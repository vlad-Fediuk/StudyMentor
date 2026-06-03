using Microsoft.EntityFrameworkCore;
using StudyMentorApi.Data;
using StudyMentorApi.Data.Models;
using StudyMentorApi.Services;

namespace StudyMentorApi.Users;

public class UserService(AppDbContext dbContext) : BaseCrudService<User>
{
    public async Task<User?> GetByGroupAsync(Guid groupId, CancellationToken ct)
    {
        return await dbContext.Users.FirstOrDefaultAsync(u => u.GroupId == groupId, ct);
    }

    protected override IQueryable<User> Query()
        => dbContext.Users.AsQueryable();

    protected override async Task<User?> FindByIdAsync(Guid id, CancellationToken ct)
        => await dbContext.Users.FirstOrDefaultAsync(u => u.Id == id, ct);

    protected override async Task<User> AddEntityAsync(User entity, CancellationToken ct)
    {
        dbContext.Users.Add(entity);
        await dbContext.SaveChangesAsync(ct);
        return entity;
    }

    protected override async Task<User> SaveUpdatedEntityAsync(User entity, CancellationToken ct)
    {
        await dbContext.SaveChangesAsync(ct);
        return entity;
    }

    protected override async Task DeleteEntityAsync(User entity, CancellationToken ct)
    {
        dbContext.Users.Remove(entity);
        await dbContext.SaveChangesAsync(ct);
    }

    protected override void UpdateEntityValues(User existing, User updated)
    {
        existing.Name = updated.Name;
        existing.Password = updated.Password;
        existing.GroupId = updated.GroupId;
    }
}
