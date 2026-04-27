using StudyMentorApi.Data.Models;
using StudyMentorApi.Majors;

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
        MajorService majorService,
        CancellationToken ct)
    {
        var items = await service.GetAllAsync(ct);
        var majors = (await majorService.GetAllAsync(ct))
            .ToDictionary(m => m.Id, m => new MajorResponse(m.Id, m.Name));

        return Results.Ok(items.Select(s => new SubjectResponse(
            s.Id,
            s.Name,
            majors[s.MajorId])));
    }

    private static async Task<IResult> GetById(
        string id,
        SubjectService service,
        MajorService majorService,
        CancellationToken ct)
    {
        var item = await service.GetByIdAsync(id, ct);
        var major = await majorService.GetByIdAsync(item.MajorId, ct);
        return Results.Ok(new SubjectResponse(item.Id, item.Name, new MajorResponse(major.Id, major.Name)));
    }

    private static async Task<IResult> Create(
        SubjectRequest request,
        SubjectService service,
        MajorService majorService,
        CancellationToken ct)
    {
        var entity = new Subject { Name = request.Name, MajorId = request.MajorId };
        var created = await service.CreateAsync(entity, ct);
        var major = await majorService.GetByIdAsync(created.MajorId, ct);
        return Results.Created(
            $"/subjects/{created.Id}",
            new SubjectResponse(created.Id, created.Name, new MajorResponse(major.Id, major.Name)));
    }

    private static async Task<IResult> Update(
        string id,
        SubjectRequest request,
        SubjectService service,
        MajorService majorService,
        CancellationToken ct)
    {
        var entity = new Subject { Name = request.Name, MajorId = request.MajorId };
        var updated = await service.UpdateAsync(id, entity, ct);
        var major = await majorService.GetByIdAsync(updated.MajorId, ct);
        return Results.Ok(new SubjectResponse(updated.Id, updated.Name, new MajorResponse(major.Id, major.Name)));
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
