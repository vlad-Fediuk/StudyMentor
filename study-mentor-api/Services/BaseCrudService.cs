using StudyMentorApi.Data.Models;

namespace StudyMentorApi.Services;

public abstract class BaseCrudService<TEntity, TKey>
    where TEntity : class, IEntity<TKey>
    where TKey : notnull
{
    public virtual Task<IReadOnlyCollection<TEntity>> GetAllAsync(
        CancellationToken cancellationToken = default)
    {
        IReadOnlyCollection<TEntity> items = Query().ToList();
        return Task.FromResult(items);
    }

    public virtual async Task<TEntity> GetByIdAsync(
        TKey id,
        CancellationToken cancellationToken = default)
    {
        var entity = await FindByIdAsync(id, cancellationToken);
        return entity ?? throw new KeyNotFoundException(
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
        TKey id,
        TEntity updatedEntity,
        CancellationToken cancellationToken = default)
    {
        var existingEntity = await FindByIdAsync(id, cancellationToken)
            ?? throw new KeyNotFoundException(
                $"Entity '{typeof(TEntity).Name}' with id '{id}' was not found.");

        await ValidateUpdateAsync(existingEntity, updatedEntity, cancellationToken);

        UpdateEntityValues(existingEntity, updatedEntity);

        return await SaveUpdatedEntityAsync(existingEntity, cancellationToken);
    }

    public virtual async Task DeleteAsync(
        TKey id,
        CancellationToken cancellationToken = default)
    {
        var entity = await FindByIdAsync(id, cancellationToken)
            ?? throw new KeyNotFoundException(
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
        TKey id,
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
