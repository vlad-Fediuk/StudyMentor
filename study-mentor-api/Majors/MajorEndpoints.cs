using StudyMentorApi.Data.Models;

namespace StudyMentorApi.Majors;

public static class MajorEndpoints
{
    public static void MapMajorEndpoints(this IEndpointRouteBuilder routes)
    {
        var group = routes
            .MapGroup("/majors")
            .WithTags("Majors");

        group.MapGet("/", GetAll);
        group.MapGet("/{id}", GetById);
        group.MapPost("/", Create);
        group.MapPut("/{id}", Update);
        group.MapDelete("/{id}", Delete);
    }

    private static async Task<IResult> GetAll(
        MajorService service,
        CancellationToken ct)
    {
        var items = await service.GetAllAsync(ct);
        return Results.Ok(items.Select(m => new MajorResponse(m.Id, m.Name)));
    }

    private static async Task<IResult> GetById(
        string id,
        MajorService service,
        CancellationToken ct)
    {
        var item = await service.GetByIdAsync(id, ct);
        return Results.Ok(new MajorResponse(item.Id, item.Name));
    }

    private static async Task<IResult> Create(
        MajorRequest request,
        MajorService service,
        CancellationToken ct)
    {
        var entity = new Major { Name = request.Name };
        var created = await service.CreateAsync(entity, ct);
        return Results.Created($"/majors/{created.Id}", new MajorResponse(created.Id, created.Name));
    }

    private static async Task<IResult> Update(
        string id,
        MajorRequest request,
        MajorService service,
        CancellationToken ct)
    {
        var entity = new Major { Name = request.Name };
        var updated = await service.UpdateAsync(id, entity, ct);
        return Results.Ok(new MajorResponse(updated.Id, updated.Name));
    }

    private static async Task<IResult> Delete(
        string id,
        MajorService service,
        CancellationToken ct)
    {
        await service.DeleteAsync(id, ct);
        return Results.NoContent();
    }
}
