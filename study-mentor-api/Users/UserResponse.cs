namespace StudyMentorApi.Users;

public record UserResponse(
    string Id,
    string Name,
    string GroupId,
    string LearningLevel,
    string PreferredLanguage,
    string CurrentProgress,
    DateTime CreatedAt);
    
