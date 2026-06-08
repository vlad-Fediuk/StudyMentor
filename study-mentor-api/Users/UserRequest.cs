namespace StudyMentorApi.Users;

public record UserRequest(
    string Name,
    string Password,
    Guid GroupId,
    string? Email = null,
    IEnumerable<string>? Roles = null);
    
