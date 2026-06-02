namespace StudyMentorApi.Users;

public record UserRequest(
    string Name,
    string Password,
    string GroupId,
    string? LearningLevel,
    string? PreferredLanguage,
    string? CurrentProgress);
    
