using StudyMentorApi.Data.Models;

namespace StudyMentorApi.Users;

public static class UserMappingExtensions
{
    public static UserResponse ToResponse(this User u) =>
        new(
            u.Id,
            u.Name,
            u.GroupId,
            u.Email,
            u.Roles);

    public static string? NormalizeEmail(this string? email) =>
        string.IsNullOrWhiteSpace(email) ? null : email.Trim().ToLowerInvariant();

    public static User ToNewUserEntity(this UserRequest request) =>
        new()
        {
            Name = request.Name,
            Email = request.Email.NormalizeEmail(),
            Password = request.Password,
            GroupId = request.GroupId,
            Roles = [UserRoles.User]
        };

    public static User BuildUpdatedEntity(this User existing, UserRequest request, bool isAdmin)
    {
        var rolesToAssign = isAdmin
            ? (request.Roles?.ToArray() ?? existing.Roles)
            : existing.Roles;

        var passwordToAssign = string.IsNullOrWhiteSpace(request.Password)
            ? existing.Password
            : request.Password;

        return new User
        {
            Name = request.Name,
            Email = request.Email.NormalizeEmail(),
            Password = passwordToAssign,
            GroupId = request.GroupId,
            Roles = rolesToAssign
        };
    }
}
