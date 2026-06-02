using Microsoft.EntityFrameworkCore;
using StudyMentorApi.Data;
using StudyMentorApi.Data.Models;
using StudyMentorApi.Services;

namespace StudyMentorApi.ChatMessages;

public class ChatMessageService(AppDbContext dbContext) : BaseCrudService<ChatMessage>
{
    public async Task<IEnumerable<ChatMessage>> GetBySessionAsync(
        Guid sessionId,
        CancellationToken cancellationToken)
    {
        return await dbContext.ChatMessages
            .Where(m => m.ChatSessionId == sessionId)
            .OrderBy(m => m.SequenceNumber)
            .ToListAsync(cancellationToken);
    }

    protected override IQueryable<ChatMessage> Query()
        => dbContext.ChatMessages.AsQueryable();

    protected override async Task<ChatMessage?> FindByIdAsync(Guid id, CancellationToken cancellationToken)
        => await dbContext.ChatMessages.FirstOrDefaultAsync(m => m.Id == id, cancellationToken);

    protected override async Task<ChatMessage> AddEntityAsync(ChatMessage entity, CancellationToken cancellationToken)
    {
        dbContext.ChatMessages.Add(entity);
        await dbContext.SaveChangesAsync(cancellationToken);
        return entity;
    }

    protected override async Task<ChatMessage> SaveUpdatedEntityAsync(ChatMessage entity, CancellationToken cancellationToken)
    {
        await dbContext.SaveChangesAsync(cancellationToken);
        return entity;
    }

    protected override async Task DeleteEntityAsync(ChatMessage entity, CancellationToken cancellationToken)
    {
        dbContext.ChatMessages.Remove(entity);
        await dbContext.SaveChangesAsync(cancellationToken);
    }

    protected override void UpdateEntityValues(ChatMessage existingEntity, ChatMessage updatedEntity)
    {
        existingEntity.ChatSessionId = updatedEntity.ChatSessionId;
        existingEntity.Content = updatedEntity.Content;
        existingEntity.Timestamp = updatedEntity.Timestamp;
        existingEntity.Role = updatedEntity.Role;
        existingEntity.SequenceNumber = updatedEntity.SequenceNumber;
    }
}
