using StudyMentorApi.Data.Models;

namespace StudyMentorApi.Subjects;

public static class SubjectEndpoints
{
    public static void MapSubjectEndpoints(this IEndpointRouteBuilder routes)
    {
        var group = routes
            .MapGroup("/subjects")
            .WithTags("Subjects");

        group.MapGet("/", GetAll);
        group.MapGet("/{id}", GetById);
        group.MapPost("/", Create);
        group.MapPut("/{id}", Update);
        group.MapDelete("/{id}", Delete);
    }

    private static async Task<IResult> GetAll(
        SubjectService service,
        CancellationToken ct)
    {
        var items = await service.GetAllAsync(ct);
        return Results.Ok(items.Select(s => new SubjectResponse(s.Id, s.Name, s.Major)));
    }

    private static async Task<IResult> GetById(
        string id,
        SubjectService service,
        CancellationToken ct)
    {
        var item = await service.GetByIdAsync(id, ct);
        return Results.Ok(new SubjectResponse(item.Id, item.Name, item.Major));
    }

    private static async Task<IResult> Create(
        SubjectRequest request,
        SubjectService service,
        CancellationToken ct)
    {
        var entity = new Subject { Name = request.Name, Major = request.Major };
        var created = await service.CreateAsync(entity, ct);
        return Results.Created($"/subjects/{created.Id}", new SubjectResponse(created.Id, created.Name, created.Major));
    }

    private static async Task<IResult> Update(
        string id,
        SubjectRequest request,
        SubjectService service,
        CancellationToken ct)
    {
        var entity = new Subject { Name = request.Name, Major = request.Major };
        var updated = await service.UpdateAsync(id, entity, ct);
        return Results.Ok(new SubjectResponse(updated.Id, updated.Name, updated.Major));
    }

    private static async Task<IResult> Delete(
        string id,
        SubjectService service,
        CancellationToken ct)
    {
        await service.DeleteAsync(id, ct);
        return Results.NoContent();
    }
}
