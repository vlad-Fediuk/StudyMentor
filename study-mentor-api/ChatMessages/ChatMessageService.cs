using MongoDB.Driver;
using StudyMentorApi.Data.Models;
using StudyMentorApi.Services;

namespace StudyMentorApi.ChatMessages;

public class ChatMessageService(MongoDbService dbService) : BaseCrudService<ChatMessage, string>
{
    private const string CollectionName = "chat_messages";
    private readonly IMongoCollection<ChatMessage> _collection = 
        dbService.GetCollection<ChatMessage>(CollectionName);

    public async Task<IEnumerable<ChatMessage>> GetBySessionAsync(
        string sessionId,
        CancellationToken cancellationToken)
    {
        return await _collection
            .Find(m => m.ChatSessionId == sessionId)
            .SortBy(m => m.SequenceNumber)
            .ToListAsync(cancellationToken);
    }

    protected override IQueryable<ChatMessage> Query()
        => _collection.AsQueryable();

    protected override async Task<ChatMessage?> FindByIdAsync(string id, CancellationToken cancellationToken)
        => await _collection
            .Find(m => m.Id == id)
            .FirstOrDefaultAsync(cancellationToken);

    protected override async Task<ChatMessage> AddEntityAsync(ChatMessage entity, CancellationToken cancellationToken)
    {
        await _collection.InsertOneAsync(entity, cancellationToken: cancellationToken);
        return entity;
    }

    protected override async Task<ChatMessage> SaveUpdatedEntityAsync(ChatMessage entity, CancellationToken cancellationToken)
    {
        await _collection.ReplaceOneAsync(
            m => m.Id == entity.Id,
            entity,
            cancellationToken: cancellationToken);
        return entity;
    }

    protected override async Task DeleteEntityAsync(ChatMessage entity, CancellationToken cancellationToken)
        => await _collection.DeleteOneAsync(m => m.Id == entity.Id, cancellationToken);

    protected override void UpdateEntityValues(ChatMessage existingEntity, ChatMessage updatedEntity)
    {
        existingEntity.ChatSessionId = updatedEntity.ChatSessionId;
        existingEntity.Content = updatedEntity.Content;
        existingEntity.Timestamp = updatedEntity.Timestamp;
        existingEntity.ExerciseId = updatedEntity.ExerciseId;
        existingEntity.Role = updatedEntity.Role;
        existingEntity.SequenceNumber = updatedEntity.SequenceNumber;
    }
}
