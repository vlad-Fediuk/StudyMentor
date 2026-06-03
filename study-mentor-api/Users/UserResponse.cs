namespace StudyMentorApi.Users;

public record UserResponse(
    Guid Id,
    string Name,
    Guid GroupId);
    
