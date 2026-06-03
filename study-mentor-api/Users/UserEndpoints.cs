using StudyMentorApi.Data.Models;

namespace StudyMentorApi.Users;

public static class UserEndpoints
{
    public static void MapUserEndpoints(this IEndpointRouteBuilder routes)
    {
        var group = routes
            .MapGroup("/users")
            .WithTags("Users");

        group.MapGet("/", GetAll);
        group.MapGet("/{id}", GetById);
        group.MapPost("/", Create);
        group.MapPut("/{id}", Update);
        group.MapDelete("/{id}", Delete);
    }

    private static async Task<IResult> GetAll(
        UserService service,
        CancellationToken ct)
    {
        var items = await service.GetAllAsync(ct);
        return Results.Ok(items.Select(ToResponse));
    }

    private static async Task<IResult> GetById(
        Guid id,
        UserService service,
        CancellationToken ct)
    {
        var item = await service.GetByIdAsync(id, ct);
        return Results.Ok(ToResponse(item));
    }

    private static async Task<IResult> Create(
        UserRequest request,
        UserService service,
        CancellationToken ct)
    {
        var entity = new User
        {
            Name = request.Name,
            Email = NormalizeEmail(request.Email),
            Password = request.Password,
            GroupId = request.GroupId,
            Roles = NormalizeRoles(request.Roles)
        };
        var created = await service.CreateAsync(entity, ct);
        return Results.Created($"/users/{created.Id}", ToResponse(created));
    }

    private static async Task<IResult> Update(
        Guid id,
        UserRequest request,
        UserService service,
        CancellationToken ct)
    {
        var entity = new User
        {
            Name = request.Name,
            Email = NormalizeEmail(request.Email),
            Password = request.Password,
            GroupId = request.GroupId,
            Roles = NormalizeRoles(request.Roles)
        };
        var updated = await service.UpdateAsync(id, entity, ct);
        return Results.Ok(ToResponse(updated));
    }

    private static async Task<IResult> Delete(
        Guid id,
        UserService service,
        CancellationToken ct)
    {
        await service.DeleteAsync(id, ct);
        return Results.NoContent();
    }

    private static UserResponse ToResponse(User u) =>
        new(u.Id, u.Name, u.GroupId, u.Email, NormalizeRoles(u.Roles));

    private static string? NormalizeEmail(string? email)
    {
        return string.IsNullOrWhiteSpace(email)
            ? null
            : email.Trim();
    }

    private static List<string> NormalizeRoles(IEnumerable<string>? roles)
    {
        var normalizedRoles = roles?
            .Where(role => !string.IsNullOrWhiteSpace(role))
            .Select(NormalizeRoleName)
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .ToList();

        return normalizedRoles is { Count: > 0 }
            ? normalizedRoles
            : ["User"];
    }

    private static string NormalizeRoleName(string role)
    {
        var normalizedRole = role.Trim();

        if (normalizedRole.Equals("admin", StringComparison.OrdinalIgnoreCase))
        {
            return "Admin";
        }

        if (normalizedRole.Equals("user", StringComparison.OrdinalIgnoreCase))
        {
            return "User";
        }

        return normalizedRole;
    }
}
