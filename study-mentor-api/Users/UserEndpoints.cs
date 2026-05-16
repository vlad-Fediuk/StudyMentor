using System.Security.Claims;
using StudyMentorApi.Data.Models;

namespace StudyMentorApi.Users;

public static class UserEndpoints
{
    public static void MapUserEndpoints(this IEndpointRouteBuilder routes)
    {
        var auth = routes
            .MapGroup("/auth")
            .WithTags("Auth");

        auth.MapPost("/login",    Login);
        auth.MapPost("/register", Register);

        var group = routes
            .MapGroup("/users")
            .WithTags("Users")
            .RequireAuthorization();

        group.MapGet("/",        GetAll);
        group.MapGet("/me",      GetMe);
        group.MapGet("/{id}",    GetById);
        group.MapPut("/{id}",    Update);
        group.MapDelete("/{id}", Delete);
    }

    private static async Task<IResult> Login(
        LoginRequest request, UserService service, CancellationToken ct)
    {
        try
        {
            var response = await service.LoginAsync(request, ct);
            return Results.Ok(response);
        }
        catch (UnauthorizedAccessException)
        {
            return Results.Unauthorized();
        }
    }

    private static async Task<IResult> Register(
        UserRequest request, UserService service, CancellationToken ct)
    {
        var entity  = service.MapFromRequest(request);
        var created = await service.CreateAsync(entity, ct);
        return Results.Created($"/users/{created.Id}", ToResponse(created));
    }

    private static async Task<IResult> GetAll(
        UserService service, CancellationToken ct)
    {
        var items = await service.GetAllAsync(ct);
        return Results.Ok(items.Select(ToResponse));
    }

    private static async Task<IResult> GetById(
        string id, UserService service, CancellationToken ct)
    {
        var item = await service.GetByIdAsync(id, ct);
        return Results.Ok(ToResponse(item));
    }

    private static async Task<IResult> Update(
        string id, UserRequest request, UserService service, CancellationToken ct)
    {
        var entity  = service.MapFromRequest(request);
        var updated = await service.UpdateAsync(id, entity, ct);
        return Results.Ok(ToResponse(updated));
    }

    private static async Task<IResult> Delete(
        string id, UserService service, CancellationToken ct)
    {
        await service.DeleteAsync(id, ct);
        return Results.NoContent();
    }

    private static IResult GetMe(ClaimsPrincipal currentUser) =>
        Results.Ok(new
        {
            Id      = currentUser.FindFirst("sub")?.Value,
            Email   = currentUser.FindFirst("email")?.Value,
            Name    = currentUser.FindFirst("name")?.Value,
            GroupId = currentUser.FindFirst("group_id")?.Value
        });

    private static UserResponse ToResponse(User u) =>
        new(u.Id, u.Name, u.Email, u.GroupId);
}
