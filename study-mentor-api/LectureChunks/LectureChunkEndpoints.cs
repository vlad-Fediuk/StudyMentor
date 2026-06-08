using Microsoft.AspNetCore.Mvc;
using StudyMentorApi.Common;
using StudyMentorApi.Data;
using StudyMentorApi.Data.Models;

namespace StudyMentorApi.LectureChunks;

public static class LectureChunkEndpoints
{
    public static void MapLectureChunkEndpoints(this IEndpointRouteBuilder routes)
    {
        var group = routes
            .MapGroup("/lecture-chunks")
            .WithTags("LectureChunks");

        group.MapPost("/upload", Upload);
        group.MapGet("/lecture/{lectureId}", GetByLectureId);
        group.MapDelete("/lecture/{lectureId}", DeleteByLectureId);
    }

    private static async Task<IResult> Upload(
        [FromForm] Guid SubjectId,
        [FromForm] Guid LectureId,
        IFormFile? File,
        LectureChunkService service,
        CancellationToken ct)
    {
        try
        {
            var request = new LectureChunkRequest(SubjectId, LectureId, File);
            var chunks = await service.CreateFromFileAsync(request, ct);
            return Results.Ok(chunks);
        }
        catch (ValidationException ex)
        {
            return Results.BadRequest(new { error = ex.Message });
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

    private static async Task<IResult> GetByLectureId(
        Guid lectureId,
        AppDbContext dbContext,
        CancellationToken ct)
    {
        try
        {
            var chunks = await dbContext.LectureChunks
                .Where(c => c.LectureId == lectureId)
                .OrderBy(c => c.Order)
                .Select(c => new LectureChunkResponse(c.Id, c.Content, c.Order, c.LectureId))
                .ToListAsync(ct);

            return Results.Ok(chunks);
        }
        catch (Exception ex)
        {
            return Results.Problem(ex.Message, statusCode: 500);
        }
    }

    private static async Task<IResult> DeleteByLectureId(
        Guid lectureId,
        AppDbContext dbContext,
        CancellationToken ct)
    {
        try
        {
            var chunks = await dbContext.LectureChunks
                .Where(c => c.LectureId == lectureId)
                .ToListAsync(ct);

            if (chunks.Count == 0)
            {
                return Results.NotFound(new { error = "No chunks found for this lecture" });
            }

            dbContext.LectureChunks.RemoveRange(chunks);
            await dbContext.SaveChangesAsync(ct);

            return Results.NoContent();
        }
        catch (Exception ex)
        {
            return Results.Problem(ex.Message, statusCode: 500);
        }
    }
}
