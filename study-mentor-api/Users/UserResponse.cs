namespace StudyMentorApi.Users;

public record UserResponse(
    string Id,
    string Name,
    string Email,
    string GroupId);

public record LoginResponse(
    string Token,
    DateTime ExpiresAt,
    string Id,
    string Name,
    string Email,
    string GroupId);
    