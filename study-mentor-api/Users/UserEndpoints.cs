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
            .WithTags("Users")
            .RequireRateLimiting("api");

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
        return Results.Ok(items.Select(u => u.ToResponse()));
    }

    private static async Task<IResult> GetById(
        Guid id,
        ClaimsPrincipal user,
        UserService service,
        CancellationToken ct)
    {
        if (user.AuthorizeOwnerOrAdmin(id) is { } authResult)
        {
            return authResult;
        }

        var item = await service.GetByIdAsync(id, ct);
        return Results.Ok(item.ToResponse());
    }

    private static async Task<IResult> Create(
        UserRequest request,
        UserService service,
        CancellationToken ct)
    {
        if (string.IsNullOrWhiteSpace(request.Name) || string.IsNullOrWhiteSpace(request.Email))
        {
            return Results.BadRequest(new { error = "Name and Email are required." });
        }

        var entity = request.ToNewUserEntity();
        var created = await service.CreateAsync(entity, ct);
        return Results.Created($"/users/{created.Id}", created.ToResponse());
    }

    private static async Task<IResult> Update(
        Guid id,
        ClaimsPrincipal user,
        UserRequest request,
        UserService service,
        CancellationToken ct)
    {
        if (user.AuthorizeOwnerOrAdmin(id) is { } authResult)
        {
            return authResult;
        }

        var existing = await service.GetByIdAsync(id, ct);
        var entity = existing.BuildUpdatedEntity(request, user.IsAdmin());
        var updated = await service.UpdateAsync(id, entity, ct);
        return Results.Ok(updated.ToResponse());
    }

    private static async Task<IResult> Delete(
        Guid id,
        ClaimsPrincipal user,
        UserService service,
        CancellationToken ct)
    {
        if (user.AuthorizeOwnerOrAdmin(id) is { } authResult)
        {
            return authResult;
        }

        await service.DeleteAsync(id, ct);
        return Results.NoContent();
    }
}
