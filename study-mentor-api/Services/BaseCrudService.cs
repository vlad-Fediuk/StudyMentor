using System.Linq.Expressions;
using System.Reflection;
using Microsoft.EntityFrameworkCore;
using study_mentor_api.Data.Models;
using study_mentor_api.DTOs;

namespace study_mentor_api.Services;

public abstract class BaseCrudService<TEntity, TKey>
    where TEntity : class, IEntity<TKey>
    where TKey : notnull
{
    public virtual async Task<IReadOnlyCollection<TEntity>> GetAllAsync(
        CrudQueryOptions? options = null,
        CancellationToken cancellationToken = default)
    {
        var query = BuildQuery(options);
        var items = await query.ToListAsync(cancellationToken);
        return items;
    }

    public virtual async Task<PagedResult<TEntity>> GetPageAsync(
        CrudQueryOptions options,
        CancellationToken cancellationToken = default)
    {
        ValidatePagination(options);

        var query = BuildQuery(options);
        var totalCount = await query.CountAsync(cancellationToken);

        var items = await ApplyPagination(query, options)
            .ToListAsync(cancellationToken);

        return new PagedResult<TEntity>(
            items,
            totalCount,
            options.Page,
            options.PageSize);
    }

    public virtual async Task<TEntity> GetByIdAsync(
        TKey id,
        CancellationToken cancellationToken = default)
    {
        var entity = await FindByIdAsync(id, cancellationToken);
        return entity ?? throw new EntityNotFoundException<TEntity, TKey>(id);
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
            ?? throw new EntityNotFoundException<TEntity, TKey>(id);

        await ValidateUpdateAsync(existingEntity, updatedEntity, cancellationToken);

        UpdateEntityValues(existingEntity, updatedEntity);

        return await SaveUpdatedEntityAsync(existingEntity, cancellationToken);
    }

    public virtual async Task DeleteAsync(
        TKey id,
        CancellationToken cancellationToken = default)
    {
        var entity = await FindByIdAsync(id, cancellationToken)
            ?? throw new EntityNotFoundException<TEntity, TKey>(id);

        await DeleteEntityAsync(entity, cancellationToken);
    }

    protected virtual IQueryable<TEntity> BuildQuery(CrudQueryOptions? options)
    {
        var query = Query();

        if (options is null)
        {
            return query;
        }

        query = ApplySearch(query, options.SearchTerm);
        query = ApplySorting(query, options.SortBy, options.SortDescending);

        return query;
    }

    protected virtual IQueryable<TEntity> ApplySearch(
        IQueryable<TEntity> query,
        string? searchTerm)
    {
        if (string.IsNullOrWhiteSpace(searchTerm))
        {
            return query;
        }

        var parameter = Expression.Parameter(typeof(TEntity), "entity");
        Expression? body = null;

        var stringProperties = typeof(TEntity)
            .GetProperties(BindingFlags.Public | BindingFlags.Instance)
            .Where(property => property.PropertyType == typeof(string));

        foreach (var property in stringProperties)
        {
            var propertyAccess = Expression.Property(parameter, property);

            var notNullExpression = Expression.NotEqual(
                propertyAccess,
                Expression.Constant(null, typeof(string)));

            var toLowerMethod = typeof(string).GetMethod(nameof(string.ToLower), Type.EmptyTypes)!;
            var containsMethod = typeof(string).GetMethod(nameof(string.Contains), new[] { typeof(string) })!;

            var loweredProperty = Expression.Call(propertyAccess, toLowerMethod);
            var loweredSearchTerm = Expression.Constant(searchTerm.Trim().ToLower());

            var containsExpression = Expression.Call(loweredProperty, containsMethod, loweredSearchTerm);
            var safeContainsExpression = Expression.AndAlso(notNullExpression, containsExpression);

            body = body is null
                ? safeContainsExpression
                : Expression.OrElse(body, safeContainsExpression);
        }

        if (body is null)
        {
            return query;
        }

        var predicate = Expression.Lambda<Func<TEntity, bool>>(body, parameter);
        return query.Where(predicate);
    }

    protected virtual IQueryable<TEntity> ApplySorting(
        IQueryable<TEntity> query,
        string? sortBy,
        bool sortDescending)
    {
        if (string.IsNullOrWhiteSpace(sortBy))
        {
            return query;
        }

        var property = typeof(TEntity).GetProperty(
            sortBy,
            BindingFlags.Public | BindingFlags.Instance | BindingFlags.IgnoreCase);

        if (property is null)
        {
            return query;
        }

        var parameter = Expression.Parameter(typeof(TEntity), "entity");
        var propertyAccess = Expression.Property(parameter, property);
        var keySelector = Expression.Lambda(propertyAccess, parameter);

        var methodName = sortDescending
            ? nameof(Queryable.OrderByDescending)
            : nameof(Queryable.OrderBy);

        var method = typeof(Queryable)
            .GetMethods(BindingFlags.Public | BindingFlags.Static)
            .First(m =>
                m.Name == methodName &&
                m.GetParameters().Length == 2);

        var genericMethod = method.MakeGenericMethod(typeof(TEntity), property.PropertyType);

        return (IQueryable<TEntity>)genericMethod.Invoke(null, new object[] { query, keySelector })!;
    }

    protected virtual IQueryable<TEntity> ApplyPagination(
        IQueryable<TEntity> query,
        CrudQueryOptions options)
    {
        var skip = (options.Page - 1) * options.PageSize;
        return query.Skip(skip).Take(options.PageSize);
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

    private static void ValidatePagination(CrudQueryOptions options)
    {
        if (options.Page < 1)
        {
            throw new ArgumentOutOfRangeException(nameof(options.Page), "Page must be greater than 0.");
        }

        if (options.PageSize < 1)
        {
            throw new ArgumentOutOfRangeException(nameof(options.PageSize), "PageSize must be greater than 0.");
        }
    }
}