using StudyMentorApi.Data.Models;

namespace StudyMentorApi.Groups;

public static class GroupEndpoints
{
    public static void MapGroupEndpoints(this IEndpointRouteBuilder routes)
    {
        var group = routes
            .MapGroup("/groups")
            .WithTags("Groups")
            .RequireAuthorization("user");

        group.MapGet("/", GetAll);
        group.MapGet("/{id}", GetById);
        group.MapPost("/", Create);
        group.MapPut("/{id}", Update);
        group.MapDelete("/{id}", Delete);
    }

    private static async Task<IResult> GetAll(
        GroupService service,
        CancellationToken ct)
    {
        var items = await service.GetAllAsync(ct);
        return Results.Ok(items.Select(ToResponse));
    }

    private static async Task<IResult> GetById(
        Guid id,
        GroupService service,
        CancellationToken ct)
    {
        var item = await service.GetByIdAsync(id, ct);
        return Results.Ok(ToResponse(item));
    }

    private static async Task<IResult> Create(
        GroupRequest request,
        GroupService service,
        CancellationToken ct)
    {
        var entity = new Group { Name = request.Name };
        var created = await service.CreateAsync(entity, ct);
        return Results.Created($"/groups/{created.Id}", ToResponse(created));
    }

    private static async Task<IResult> Update(
        Guid id,
        GroupRequest request,
        GroupService service,
        CancellationToken ct)
    {
        var entity = new Group { Name = request.Name };
        var updated = await service.UpdateAsync(id, entity, ct);
        return Results.Ok(ToResponse(updated));
    }

    private static async Task<IResult> Delete(
        Guid id,
        GroupService service,
        CancellationToken ct)
    {
        await service.DeleteAsync(id, ct);
        return Results.NoContent();
    }

    private static GroupResponse ToResponse(Group group) =>
        new(group.Id, group.Name);
}
