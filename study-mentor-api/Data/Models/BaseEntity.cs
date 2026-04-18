namespace study_mentor_api.Data.Models;

public abstract class BaseEntity<TKey> : IEntity<TKey>  where TKey : notnull
{
    public TKey Id { get; protected set; } = default!;
}
