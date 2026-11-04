using StudyMentorApi.Data.Models;

namespace study_mentor_api.Tests.TestHelpers;

public static class EntityTestExtensions
{
    public static T WithId<T>(this T entity, Guid id) where T : BaseEntity
    {
        typeof(BaseEntity).GetProperty(nameof(BaseEntity.Id))!.SetValue(entity, id);
        return entity;
    }
}
