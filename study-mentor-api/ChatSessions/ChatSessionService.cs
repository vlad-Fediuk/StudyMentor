using Microsoft.EntityFrameworkCore;
using StudyMentorApi.Data;
using StudyMentorApi.Data.Models;
using StudyMentorApi.Services;

namespace StudyMentorApi.ChatSessions;

public class ChatSessionService(AppDbContext dbContext) : BaseCrudService<ChatSession>
{
    public async Task<IEnumerable<ChatSession>> GetByUserAsync(Guid userId, CancellationToken ct)
    {
        return await dbContext.ChatSessions
            .Where(s => s.UserId == userId)
            .ToListAsync(ct);
    }

    public async Task<bool> ExistsAsync(Guid id, CancellationToken ct)
    {
        return await dbContext.ChatSessions.AnyAsync(s => s.Id == id, ct);
    }

    protected override IQueryable<ChatSession> Query()
        => dbContext.ChatSessions
            .Include(s => s.Lecture)
            .ThenInclude(l => l.Subject)
            .AsQueryable();

    protected override async Task<ChatSession?> FindByIdAsync(Guid id, CancellationToken ct)
        => await Query().FirstOrDefaultAsync(s => s.Id == id, ct);

    protected override async Task<ChatSession> AddEntityAsync(ChatSession entity, CancellationToken ct)
    {
        dbContext.ChatSessions.Add(entity);
        await dbContext.SaveChangesAsync(ct);
        return entity;
    }

    protected override async Task<ChatSession> SaveUpdatedEntityAsync(ChatSession entity, CancellationToken ct)
    {
        await dbContext.SaveChangesAsync(ct);
        return entity;
    }

    protected override async Task DeleteEntityAsync(ChatSession entity, CancellationToken ct)
    {
        dbContext.ChatSessions.Remove(entity);
        await dbContext.SaveChangesAsync(ct);
    }

    protected override void UpdateEntityValues(ChatSession existing, ChatSession updated)
    {
        existing.UserId = updated.UserId;
        existing.LectureId = updated.LectureId;
    }
}
