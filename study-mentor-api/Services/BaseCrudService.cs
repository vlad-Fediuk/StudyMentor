using Microsoft.EntityFrameworkCore;
using StudyMentorApi.Common;
using StudyMentorApi.Data.Models;

namespace StudyMentorApi.Services;

public abstract class BaseCrudService<TEntity>
    where TEntity : class, IEntity
{
    public virtual async Task<IReadOnlyCollection<TEntity>> GetAllAsync(
        CancellationToken cancellationToken = default)
    {
        return await Query().ToListAsync(cancellationToken);
    }

    public virtual async Task<TEntity> GetByIdAsync(
        Guid id,
        CancellationToken cancellationToken = default)
    {
        var entity = await FindByIdAsync(id, cancellationToken);
        return entity ?? throw new NotFoundException(
            $"Entity '{typeof(TEntity).Name}' with id '{id}' was not found.");
    }

    public virtual async Task<TEntity> CreateAsync(
        TEntity entity,
        CancellationToken cancellationToken = default)
    {
        await ValidateCreateAsync(entity, cancellationToken);
        return await AddEntityAsync(entity, cancellationToken);
    }

    public virtual async Task<TEntity> UpdateAsync(
        Guid id,
        TEntity updatedEntity,
        CancellationToken cancellationToken = default)
    {
        var existingEntity = await FindByIdAsync(id, cancellationToken)
            ?? throw new NotFoundException(
                $"Entity '{typeof(TEntity).Name}' with id '{id}' was not found.");

        await ValidateUpdateAsync(existingEntity, updatedEntity, cancellationToken);
        UpdateEntityValues(existingEntity, updatedEntity);
        return await SaveUpdatedEntityAsync(existingEntity, cancellationToken);
    }

    public virtual async Task DeleteAsync(
        Guid id,
        CancellationToken cancellationToken = default)
    {
        var entity = await FindByIdAsync(id, cancellationToken)
            ?? throw new NotFoundException(
                $"Entity '{typeof(TEntity).Name}' with id '{id}' was not found.");

        await DeleteEntityAsync(entity, cancellationToken);
    }

    protected virtual Task ValidateCreateAsync(
        TEntity entity,
        CancellationToken cancellationToken)
    {
        return Task.CompletedTask;
    }

    protected virtual Task ValidateUpdateAsync(
        TEntity existingEntity,
        TEntity updatedEntity,
        CancellationToken cancellationToken)
    {
        return Task.CompletedTask;
    }

    protected abstract IQueryable<TEntity> Query();

    protected abstract Task<TEntity?> FindByIdAsync(
        Guid id,
        CancellationToken cancellationToken);

    protected abstract Task<TEntity> AddEntityAsync(
        TEntity entity,
        CancellationToken cancellationToken);

    protected abstract Task<TEntity> SaveUpdatedEntityAsync(
        TEntity entity,
        CancellationToken cancellationToken);

    protected abstract Task DeleteEntityAsync(
        TEntity entity,
        CancellationToken cancellationToken);

    protected abstract void UpdateEntityValues(
        TEntity existingEntity,
        TEntity updatedEntity);
}
