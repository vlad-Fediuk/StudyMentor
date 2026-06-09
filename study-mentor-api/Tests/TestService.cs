using Microsoft.EntityFrameworkCore;
using StudyMentorApi.ChatMessages;
using StudyMentorApi.Common;
using StudyMentorApi.Data;
using StudyMentorApi.Data.Models;
using StudyMentorApi.Flashcards;
using StudyMentorApi.Services;

namespace StudyMentorApi.Tests;

public class TestService(
    AppDbContext dbContext,
    ChatMessageService chatMessageService,
    FlashcardService flashcardService)
    : BaseCrudService<Test>
{
    protected override IQueryable<Test> Query()
    {
        return dbContext.Tests
            .Include(t => t.Questions.OrderBy(q => q.Order))
            .ThenInclude(q => q.AnswerVariants.OrderBy(a => a.Order))
            .AsQueryable();
    }

    protected override async Task<Test?> FindByIdAsync(Guid id, CancellationToken cancellationToken)
    {
        return await dbContext.Tests
            .Include(t => t.Questions.OrderBy(q => q.Order))
            .ThenInclude(q => q.AnswerVariants.OrderBy(a => a.Order))
            .FirstOrDefaultAsync(t => t.Id == id, cancellationToken);
    }

    protected override async Task<Test> AddEntityAsync(Test entity, CancellationToken cancellationToken)
    {
        dbContext.Tests.Add(entity);
        await dbContext.SaveChangesAsync(cancellationToken);
        return entity;
    }

    protected override async Task<Test> SaveUpdatedEntityAsync(Test entity, CancellationToken cancellationToken)
    {
        await dbContext.SaveChangesAsync(cancellationToken);
        return entity;
    }

    protected override async Task DeleteEntityAsync(Test entity, CancellationToken cancellationToken)
    {
        dbContext.Tests.Remove(entity);
        await dbContext.SaveChangesAsync(cancellationToken);
    }

    protected override void UpdateEntityValues(Test existingEntity, Test updatedEntity)
    {
        existingEntity.Name = updatedEntity.Name;
        existingEntity.ChatMessageId = updatedEntity.ChatMessageId;
        existingEntity.SourceFlashcardId = updatedEntity.SourceFlashcardId;
        existingEntity.Questions.Clear();

        foreach (var question in updatedEntity.Questions)
        {
            existingEntity.Questions.Add(question);
        }
    }

    protected override async Task ValidateCreateAsync(Test entity, CancellationToken cancellationToken)
    {
        await ValidateAsync(entity, cancellationToken);
    }

    protected override async Task ValidateUpdateAsync(
        Test existingEntity,
        Test updatedEntity,
        CancellationToken cancellationToken)
    {
        await ValidateAsync(updatedEntity, cancellationToken);
    }

    public async Task<TestAnswerResultResponse> CheckAnswerAsync(
        Guid testId,
        Guid questionId,
        Guid answerVariantId,
        CancellationToken cancellationToken = default)
    {
        var test = await FindByIdAsync(testId, cancellationToken)
            ?? throw new NotFoundException($"Entity 'Test' with id '{testId}' was not found.");

        var question = test.Questions.FirstOrDefault(q => q.Id == questionId)
            ?? throw new NotFoundException(
                $"Question '{questionId}' was not found in test '{testId}'.");

        var selectedAnswer = question.AnswerVariants.FirstOrDefault(a => a.Id == answerVariantId)
            ?? throw new NotFoundException(
                $"Answer variant '{answerVariantId}' was not found in question '{questionId}'.");

        var correctAnswer = question.AnswerVariants.Single(a => a.IsCorrect);

        return new TestAnswerResultResponse(
            question.Id,
            selectedAnswer.Id,
            correctAnswer.Id,
            selectedAnswer.Id == correctAnswer.Id);
    }

    private async Task ValidateAsync(Test entity, CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(entity.Name))
        {
            throw new ValidationException("Test name is required.");
        }

        if (entity.Questions.Count == 0)
        {
            throw new ValidationException("Test must contain at least one question.");
        }

        if (entity.Questions.Any(q => string.IsNullOrWhiteSpace(q.Prompt)))
        {
            throw new ValidationException("Every question must have a prompt.");
        }

        foreach (var question in entity.Questions)
        {
            if (question.AnswerVariants.Count < 2)
            {
                throw new ValidationException("Every question must have at least two answer variants.");
            }

            if (question.AnswerVariants.Any(a => string.IsNullOrWhiteSpace(a.Text)))
            {
                throw new ValidationException("Every answer variant must have text.");
            }

            if (question.AnswerVariants.Count(a => a.IsCorrect) != 1)
            {
                throw new ValidationException("Every question must have exactly one correct answer.");
            }
        }

        await chatMessageService.GetByIdAsync(entity.ChatMessageId, cancellationToken);

        if (entity.SourceFlashcardId.HasValue)
        {
            await flashcardService.GetByIdAsync(entity.SourceFlashcardId.Value, cancellationToken);
        }
    }
}
