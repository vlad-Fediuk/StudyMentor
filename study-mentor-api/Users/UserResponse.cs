namespace StudyMentorApi.Users;

public record UserResponse(
    string Id,
    string Name,
    string? Email,
    string GroupId,
    IReadOnlyCollection<string> Roles);
