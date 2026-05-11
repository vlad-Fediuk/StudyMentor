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
        string id,
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
            Password = request.Password,
            GroupId = request.GroupId
        };
        var created = await service.CreateAsync(entity, ct);
        return Results.Created($"/users/{created.Id}", ToResponse(created));
    }

    private static async Task<IResult> Update(
        string id,
        UserRequest request,
        UserService service,
        CancellationToken ct)
    {
        var entity = new User
        {
            Name = request.Name,
            Password = request.Password,
            GroupId = request.GroupId
        };
        var updated = await service.UpdateAsync(id, entity, ct);
        return Results.Ok(ToResponse(updated));
    }

    private static async Task<IResult> Delete(
        string id,
        UserService service,
        CancellationToken ct)
    {
        await service.DeleteAsync(id, ct);
        return Results.NoContent();
    }

    private static UserResponse ToResponse(User u) =>
        new(u.Id, u.Name, u.GroupId);
}