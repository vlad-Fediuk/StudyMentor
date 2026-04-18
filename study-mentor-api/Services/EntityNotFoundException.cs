
namespace study_mentor_api.Services;
public sealed class EntityNotFoundException<TEntity, TKey> : Exception
    where TEntity : class
    where TKey : notnull
{
    public EntityNotFoundException(TKey id)
        : base($"Entity '{typeof(TEntity).Name}' with id '{id}' was not found.")
    {
        EntityType = typeof(TEntity);
        EntityId = id;
    }

    public Type EntityType { get; }
    public TKey EntityId { get; }

}
