using MongoDB.Driver;
using MongoDB.Bson;
using StudyMentorApi.Data.Models;
using StudyMentorApi.Services;

namespace StudyMentorApi.ChatSessions;

public class ChatSessionService(MongoDbService dbService) : BaseCrudService<ChatSession, string>
{
    private const string CollectionName = "chat_sessions";
    private readonly IMongoCollection<ChatSession> _collection =
        dbService.GetCollection<ChatSession>(CollectionName);

    public async Task<IEnumerable<ChatSession>> GetByUserAsync(string userId, CancellationToken ct)
    {
        return await _collection
            .Find(s => s.UserId == userId)
            .ToListAsync(ct);
    }

    public async Task<bool> ExistsAsync(string id, CancellationToken ct)
    {
        if (!ObjectId.TryParse(id, out _))
        {
            return false;
        }

        return await _collection
            .Find(s => s.Id == id)
            .AnyAsync(ct);
    }

    protected override IQueryable<ChatSession> Query()
        => _collection.AsQueryable();

    protected override async Task<ChatSession?> FindByIdAsync(string id, CancellationToken ct)
        => await _collection
            .Find(s => s.Id == id)
            .FirstOrDefaultAsync(ct);

    protected override async Task<ChatSession> AddEntityAsync(ChatSession entity, CancellationToken ct)
    {
        await _collection.InsertOneAsync(entity, cancellationToken: ct);
        return entity;
    }

    protected override async Task<ChatSession> SaveUpdatedEntityAsync(ChatSession entity, CancellationToken ct)
    {
        await _collection.ReplaceOneAsync(
            s => s.Id == entity.Id,
            entity,
            cancellationToken: ct);
        return entity;
    }

    protected override async Task DeleteEntityAsync(ChatSession entity, CancellationToken ct)
        => await _collection.DeleteOneAsync(s => s.Id == entity.Id, ct);

    protected override void UpdateEntityValues(ChatSession existing, ChatSession updated)
    {
        existing.UserId = updated.UserId;
        existing.LectureId = updated.LectureId;
        existing.Title = updated.Title;
        existing.Topic = updated.Topic;
        existing.UpdatedAt = updated.UpdatedAt;
    }
}
