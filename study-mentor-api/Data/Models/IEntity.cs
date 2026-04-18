namespace study_mentor_api.Data.Models;

public interface IEntity<TKey> where TKey : notnull
{
    TKey Id { get; }
}
