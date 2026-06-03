namespace StudyMentorApi.Users;

public record UserResponse(
    Guid Id,
    string Name,
    Guid GroupId,
    string? Email = null,
    IEnumerable<string>? Roles = null);
    
