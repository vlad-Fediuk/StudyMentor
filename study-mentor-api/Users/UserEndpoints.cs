using System.Security.Claims;
using StudyMentorApi.Data.Models;
using StudyMentorApi.Extensions;

namespace StudyMentorApi.Users;

public static class UserEndpoints
{
    public static void MapUserEndpoints(this IEndpointRouteBuilder routes)
    {
        var group = routes
            .MapGroup("/users")
            .WithTags("Users");

        group.MapGet("/", GetAll).RequireAuthorization("admin");
        group.MapGet("/{id}", GetById).RequireAuthorization("user");
        group.MapPost("/", Create);
        group.MapPut("/{id}", Update).RequireAuthorization("user");
        group.MapDelete("/{id}", Delete).RequireAuthorization("user");
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
        ClaimsPrincipal user,
        UserService service,
        CancellationToken ct)
    {
        var currentUserId = user.GetUserId();
        if (currentUserId == null)
        {
            return Results.Unauthorized();
        }

        if (currentUserId != id && !user.IsAdmin())
        {
            return Results.Forbid();
        }

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
            Roles = ["User"]
        };
        var created = await service.CreateAsync(entity, ct);
        return Results.Created($"/users/{created.Id}", ToResponse(created));
    }

    private static async Task<IResult> Update(
        Guid id,
        ClaimsPrincipal user,
        UserRequest request,
        UserService service,
        CancellationToken ct)
    {
        var currentUserId = user.GetUserId();
        if (currentUserId == null)
        {
            return Results.Unauthorized();
        }

        var isAdmin = user.IsAdmin();
        if (currentUserId != id && !isAdmin)
        {
            return Results.Forbid();
        }

        var existing = await service.GetByIdAsync(id, ct);

        var rolesToAssign = isAdmin
            ? (request.Roles?.ToArray() ?? existing.Roles)
            : existing.Roles;

        var passwordToAssign = string.IsNullOrWhiteSpace(request.Password)
            ? existing.Password
            : request.Password;

        var entity = new User
        {
            Name = request.Name,
            Email = NormalizeEmail(request.Email),
            Password = passwordToAssign,
            GroupId = request.GroupId,
            Roles = rolesToAssign
        };
        var updated = await service.UpdateAsync(id, entity, ct);
        return Results.Ok(ToResponse(updated));
    }

    private static async Task<IResult> Delete(
        Guid id,
        ClaimsPrincipal user,
        UserService service,
        CancellationToken ct)
    {
        var currentUserId = user.GetUserId();
        if (currentUserId == null)
        {
            return Results.Unauthorized();
        }

        if (currentUserId != id && !user.IsAdmin())
        {
            return Results.Forbid();
        }

        await service.DeleteAsync(id, ct);
        return Results.NoContent();
    }

    private static UserResponse ToResponse(User u) =>
        new(
            u.Id,
            u.Name,
            u.GroupId,
            u.Email,
            u.Roles);

    private static string? NormalizeEmail(string? email) =>
        string.IsNullOrWhiteSpace(email) ? null : email.Trim().ToLowerInvariant();
}
