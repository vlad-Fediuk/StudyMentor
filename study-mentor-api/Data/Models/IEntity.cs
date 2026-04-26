namespace StudyMentorApi.Data.Models;

public interface IEntity<TKey> where TKey : notnull
{
    TKey Id { get; }
}
