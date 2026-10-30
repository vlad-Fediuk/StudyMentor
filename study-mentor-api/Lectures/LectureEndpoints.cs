using StudyMentorApi.Common;
using StudyMentorApi.Data.Models;
using StudyMentorApi.Majors;
using StudyMentorApi.Subjects;

namespace StudyMentorApi.Lectures;

public static class LectureEndpoints
{
    public static void MapLectureEndpoints(this IEndpointRouteBuilder routes)
    {
        var group = routes
            .MapGroup("/lectures")
            .WithTags("Lectures");

        group.MapGet("/", GetAll);
        group.MapGet("/{id}", GetById);
        group.MapPost("/", Create).RequireAuthorization("user");
        group.MapPut("/{id}", Update).RequireAuthorization("user");
        group.MapDelete("/{id}", Delete).RequireAuthorization("user");
    }

    private static async Task<IResult> GetAll(
        LectureService service,
        CancellationToken ct)
    {
        try
        {
            var items = await service.GetAllResponsesAsync(ct);
            return Results.Ok(items);
        }
        catch (Exception ex)
        {
            return Results.Problem(ex.Message, statusCode: 500);
        }
    }

    private static async Task<IResult> GetById(
        Guid id,
        LectureService service,
        CancellationToken ct)
    {
        try
        {
            var item = await service.GetResponseByIdAsync(id, ct);
            return Results.Ok(item);
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
        LectureRequest request,
        LectureService service,
        SubjectService subjectService,
        MajorService majorService,
        CancellationToken ct)
    {
        try
        {
            var entity = new Lecture { Name = request.Name, SubjectId = request.SubjectId };
            var created = await service.CreateAsync(entity, ct);
            var subject = await MapSubjectResponseAsync(created.SubjectId, subjectService, majorService, ct);
            return Results.Created(
                $"/lectures/{created.Id}",
                new LectureResponse(created.Id, created.Name, subject));
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
        Guid id,
        LectureRequest request,
        LectureService service,
        SubjectService subjectService,
        MajorService majorService,
        CancellationToken ct)
    {
        try
        {
            var entity = new Lecture { Name = request.Name, SubjectId = request.SubjectId };
            var updated = await service.UpdateAsync(id, entity, ct);
            var subject = await MapSubjectResponseAsync(updated.SubjectId, subjectService, majorService, ct);
            return Results.Ok(new LectureResponse(updated.Id, updated.Name, subject));
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
        Guid id,
        LectureService service,
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

    private static async Task<SubjectResponse> MapSubjectResponseAsync(
        Guid subjectId,
        SubjectService subjectService,
        MajorService majorService,
        CancellationToken ct)
    {
        var subject = await subjectService.GetByIdAsync(subjectId, ct);
        var major = await majorService.GetByIdAsync(subject.MajorId, ct);
        return new SubjectResponse(subject.Id, subject.Name, new MajorResponse(major.Id, major.Name));
    }
}
