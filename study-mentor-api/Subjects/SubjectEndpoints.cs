using StudyMentorApi.Common;
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
        try
        {
            var items = await service.GetAllAsync(ct);
            var majors = (await majorService.GetAllAsync(ct))
                .ToDictionary(m => m.Id, m => new MajorResponse(m.Id, m.Name));

            return Results.Ok(items.Select(s => new SubjectResponse(
                s.Id,
                s.Name,
                majors[s.MajorId])));
        }
        catch (Exception ex)
        {
            return Results.Problem(ex.Message, statusCode: 500);
        }
    }

    private static async Task<IResult> GetById(
        string id,
        SubjectService service,
        MajorService majorService,
        CancellationToken ct)
    {
        try
        {
            var item = await service.GetByIdAsync(id, ct);
            var major = await majorService.GetByIdAsync(item.MajorId, ct);
            return Results.Ok(new SubjectResponse(item.Id, item.Name, new MajorResponse(major.Id, major.Name)));
        }
        catch (NotFoundException ex)
        {
            return Results.NotFound(new { error = ex.Message });
        }
        catch (Exception ex)
        {
            return Results.Problem(ex.Message, statusCode: 500);
        }
    }

    private static async Task<IResult> Create(
        SubjectRequest request,
        SubjectService service,
        MajorService majorService,
        CancellationToken ct)
    {
        try
        {
            var entity = new Subject { Name = request.Name, MajorId = request.MajorId };
            var created = await service.CreateAsync(entity, ct);
            var major = await majorService.GetByIdAsync(created.MajorId, ct);
            return Results.Created(
                $"/subjects/{created.Id}",
                new SubjectResponse(created.Id, created.Name, new MajorResponse(major.Id, major.Name)));
        }
        catch (ValidationException ex)
        {
            return Results.BadRequest(new { error = ex.Message });
        }
        catch (Exception ex)
        {
            return Results.Problem(ex.Message, statusCode: 500);
        }
    }

    private static async Task<IResult> Update(
        string id,
        SubjectRequest request,
        SubjectService service,
        MajorService majorService,
        CancellationToken ct)
    {
        try
        {
            var entity = new Subject { Name = request.Name, MajorId = request.MajorId };
            var updated = await service.UpdateAsync(id, entity, ct);
            var major = await majorService.GetByIdAsync(updated.MajorId, ct);
            return Results.Ok(new SubjectResponse(updated.Id, updated.Name, new MajorResponse(major.Id, major.Name)));
        }
        catch (NotFoundException ex)
        {
            return Results.NotFound(new { error = ex.Message });
        }
        catch (ValidationException ex)
        {
            return Results.BadRequest(new { error = ex.Message });
        }
        catch (Exception ex)
        {
            return Results.Problem(ex.Message, statusCode: 500);
        }
    }

    private static async Task<IResult> Delete(
        string id,
        SubjectService service,
        CancellationToken ct)
    {
        try
        {
            await service.DeleteAsync(id, ct);
            return Results.NoContent();
        }
        catch (NotFoundException ex)
        {
            return Results.NotFound(new { error = ex.Message });
        }
        catch (Exception ex)
        {
            return Results.Problem(ex.Message, statusCode: 500);
        }
    }
}
