using MongoDB.Bson;
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

        var response = items.Select(s =>
        {
            if (!majors.TryGetValue(s.MajorId, out var major))
                return null;
            return new SubjectResponse(s.Id, s.Name, major);
        }).Where(s => s is not null);

        return Results.Ok(response);
    }

    private static async Task<IResult> GetById(
        string id,
        SubjectService service,
        MajorService majorService,
        CancellationToken ct)
    {
        if (!ObjectId.TryParse(id, out _))
            return Results.BadRequest($"'{id}' is not a valid id format.");

        try
        {
            var item = await service.GetByIdAsync(id, ct);
            var major = await majorService.GetByIdAsync(item.MajorId, ct);
            return Results.Ok(new SubjectResponse(item.Id, item.Name, new MajorResponse(major.Id, major.Name)));
        }
        catch (KeyNotFoundException)
        {
            return Results.NotFound($"Subject with id '{id}' not found.");
        }
    }

    private static async Task<IResult> Create(
        SubjectRequest request,
        SubjectService service,
        MajorService majorService,
        CancellationToken ct)
    {
        if (string.IsNullOrWhiteSpace(request.Name))
            return Results.BadRequest("Name is required.");

        if (!ObjectId.TryParse(request.MajorId, out _))
            return Results.BadRequest($"'{request.MajorId}' is not a valid id format.");

        try
        {
            await majorService.GetByIdAsync(request.MajorId, ct);
        }
        catch (KeyNotFoundException)
        {
            return Results.BadRequest($"Major with id '{request.MajorId}' does not exist.");
        }

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
        if (!ObjectId.TryParse(id, out _))
            return Results.BadRequest($"'{id}' is not a valid id format.");

        if (string.IsNullOrWhiteSpace(request.Name))
            return Results.BadRequest("Name is required.");

        if (!ObjectId.TryParse(request.MajorId, out _))
            return Results.BadRequest($"'{request.MajorId}' is not a valid id format.");

        try
        {
            await majorService.GetByIdAsync(request.MajorId, ct);
        }
        catch (KeyNotFoundException)
        {
            return Results.BadRequest($"Major with id '{request.MajorId}' does not exist.");
        }

        try
        {
            var entity = new Subject { Name = request.Name, MajorId = request.MajorId };
            var updated = await service.UpdateAsync(id, entity, ct);
            var major = await majorService.GetByIdAsync(updated.MajorId, ct);
            return Results.Ok(new SubjectResponse(updated.Id, updated.Name, new MajorResponse(major.Id, major.Name)));
        }
        catch (KeyNotFoundException)
        {
            return Results.NotFound($"Subject with id '{id}' not found.");
        }
    }

    private static async Task<IResult> Delete(
        string id,
        SubjectService service,
        CancellationToken ct)
    {
        if (!ObjectId.TryParse(id, out _))
            return Results.BadRequest($"'{id}' is not a valid id format.");

        try
        {
            await service.DeleteAsync(id, ct);
            return Results.NoContent();
        }
        catch (KeyNotFoundException)
        {
            return Results.NotFound($"Subject with id '{id}' not found.");
        }
    }
}