namespace StudyMentorApi.Data.Models;

public abstract class BaseEntity : IEntity
{
    public Guid Id { get; protected set; }
}
