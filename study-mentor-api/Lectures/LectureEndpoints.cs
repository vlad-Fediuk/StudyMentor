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
        group.MapPost("/", Create);
        group.MapPut("/{id}", Update);
        group.MapDelete("/{id}", Delete);
    }

    private static async Task<IResult> GetAll(
        LectureService service,
        SubjectService subjectService,
        MajorService majorService,
        CancellationToken ct)
    {
        var items = await service.GetAllAsync(ct);
        var subjects = await BuildSubjectResponsesAsync(subjectService, majorService, ct);

        return Results.Ok(items.Select(l => new LectureResponse(
            l.Id,
            l.Name,
            subjects[l.SubjectId])));
    }

    private static async Task<IResult> GetById(
        string id,
        LectureService service,
        SubjectService subjectService,
        MajorService majorService,
        CancellationToken ct)
    {
        var item = await service.GetByIdAsync(id, ct);
        var subject = await MapSubjectResponseAsync(item.SubjectId, subjectService, majorService, ct);
        return Results.Ok(new LectureResponse(item.Id, item.Name, subject));
    }

    private static async Task<IResult> Create(
        LectureRequest request,
        LectureService service,
        SubjectService subjectService,
        MajorService majorService,
        CancellationToken ct)
    {
        var entity = new Lecture { Name = request.Name, SubjectId = request.SubjectId };
        var created = await service.CreateAsync(entity, ct);
        var subject = await MapSubjectResponseAsync(created.SubjectId, subjectService, majorService, ct);

        return Results.Created(
            $"/lectures/{created.Id}",
            new LectureResponse(created.Id, created.Name, subject));
    }

    private static async Task<IResult> Update(
        string id,
        LectureRequest request,
        LectureService service,
        SubjectService subjectService,
        MajorService majorService,
        CancellationToken ct)
    {
        var entity = new Lecture { Name = request.Name, SubjectId = request.SubjectId };
        var updated = await service.UpdateAsync(id, entity, ct);
        var subject = await MapSubjectResponseAsync(updated.SubjectId, subjectService, majorService, ct);
        return Results.Ok(new LectureResponse(updated.Id, updated.Name, subject));
    }

    private static async Task<IResult> Delete(
        string id,
        LectureService service,
        CancellationToken ct)
    {
        await service.DeleteAsync(id, ct);
        return Results.NoContent();
    }

    private static async Task<Dictionary<string, SubjectResponse>> BuildSubjectResponsesAsync(
        SubjectService subjectService,
        MajorService majorService,
        CancellationToken ct)
    {
        var majors = (await majorService.GetAllAsync(ct))
            .ToDictionary(m => m.Id, m => new MajorResponse(m.Id, m.Name));

        return (await subjectService.GetAllAsync(ct))
            .ToDictionary(
                s => s.Id,
                s => new SubjectResponse(s.Id, s.Name, majors[s.MajorId]));
    }

    private static async Task<SubjectResponse> MapSubjectResponseAsync(
        string subjectId,
        SubjectService subjectService,
        MajorService majorService,
        CancellationToken ct)
    {
        var subject = await subjectService.GetByIdAsync(subjectId, ct);
        var major = await majorService.GetByIdAsync(subject.MajorId, ct);
        return new SubjectResponse(subject.Id, subject.Name, new MajorResponse(major.Id, major.Name));
    }
}
