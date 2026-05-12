using MongoDB.Driver;
using StudyMentorApi.Data.Models;
using StudyMentorApi.Services;

namespace StudyMentorApi.Users;

public class UserService(MongoDbService dbService) : BaseCrudService<User, string>
{
    private const string CollectionName = "users";
    private readonly IMongoCollection<User> _collection =
        dbService.GetCollection<User>(CollectionName);

    public async Task<User?> GetByGroupAsync(string groupId, CancellationToken ct)
    {
        return await _collection
            .Find(u => u.GroupId == groupId)
            .FirstOrDefaultAsync(ct);
    }

    protected override IQueryable<User> Query()
        => _collection.AsQueryable();

    protected override async Task<User?> FindByIdAsync(string id, CancellationToken ct)
        => await _collection
            .Find(u => u.Id == id)
            .FirstOrDefaultAsync(ct);

    protected override async Task<User> AddEntityAsync(User entity, CancellationToken ct)
    {
        await _collection.InsertOneAsync(entity, cancellationToken: ct);
        return entity;
    }

    protected override async Task<User> SaveUpdatedEntityAsync(User entity, CancellationToken ct)
    {
        await _collection.ReplaceOneAsync(
            u => u.Id == entity.Id,
            entity,
            cancellationToken: ct);
        return entity;
    }

    protected override async Task DeleteEntityAsync(User entity, CancellationToken ct)
        => await _collection.DeleteOneAsync(u => u.Id == entity.Id, ct);

    protected override void UpdateEntityValues(User existing, User updated)
    {
        existing.Name = updated.Name;
        existing.Password = updated.Password;
        existing.GroupId = updated.GroupId;
    }
}
