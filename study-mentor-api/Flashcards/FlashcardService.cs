using Microsoft.EntityFrameworkCore;
using StudyMentorApi.ChatMessages;
using StudyMentorApi.Common;
using StudyMentorApi.Data;
using StudyMentorApi.Data.Models;
using StudyMentorApi.Services;

namespace StudyMentorApi.Flashcards;

public class FlashcardService(AppDbContext dbContext, ChatMessageService chatMessageService)
    : BaseCrudService<Flashcard>
{
    protected override IQueryable<Flashcard> Query()
    {
        return dbContext.Flashcards
            .Include(f => f.Cards)
            .AsQueryable();
    }

    protected override async Task<Flashcard?> FindByIdAsync(Guid id, CancellationToken cancellationToken)
    {
        return await dbContext.Flashcards
            .Include(f => f.Cards)
            .FirstOrDefaultAsync(f => f.Id == id, cancellationToken);
    }

    protected override async Task<Flashcard> AddEntityAsync(
        Flashcard entity,
        CancellationToken cancellationToken)
    {
        dbContext.Flashcards.Add(entity);
        await dbContext.SaveChangesAsync(cancellationToken);
        return entity;
    }

    protected override async Task<Flashcard> SaveUpdatedEntityAsync(
        Flashcard entity,
        CancellationToken cancellationToken)
    {
        await dbContext.SaveChangesAsync(cancellationToken);
        return entity;
    }

    protected override async Task DeleteEntityAsync(
        Flashcard entity,
        CancellationToken cancellationToken)
    {
        dbContext.Flashcards.Remove(entity);
        await dbContext.SaveChangesAsync(cancellationToken);
    }

    protected override void UpdateEntityValues(Flashcard existingEntity, Flashcard updatedEntity)
    {
        existingEntity.Name = updatedEntity.Name;
        existingEntity.ChatMessageId = updatedEntity.ChatMessageId;
        existingEntity.Cards.Clear();

        foreach (var card in updatedEntity.Cards)
        {
            existingEntity.Cards.Add(card);
        }
    }

    protected override async Task ValidateCreateAsync(
        Flashcard entity,
        CancellationToken cancellationToken)
    {
        await ValidateAsync(entity, cancellationToken);
    }

    protected override async Task ValidateUpdateAsync(
        Flashcard existingEntity,
        Flashcard updatedEntity,
        CancellationToken cancellationToken)
    {
        await ValidateAsync(updatedEntity, cancellationToken);
    }

    private async Task ValidateAsync(Flashcard entity, CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(entity.Name))
        {
            throw new ValidationException("Flashcard name is required.");
        }

        if (entity.Cards.Count == 0)
        {
            throw new ValidationException("Flashcard must contain at least one card.");
        }

        if (entity.Cards.Any(c =>
                string.IsNullOrWhiteSpace(c.Term) ||
                string.IsNullOrWhiteSpace(c.Definition)))
        {
            throw new ValidationException("Every card must have a term and definition.");
        }

        await chatMessageService.GetByIdAsync(entity.ChatMessageId, cancellationToken);
    }
}
