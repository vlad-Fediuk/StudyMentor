using StudyMentorApi.Common;
using StudyMentorApi.Services.Ai;
using StudyMentorApi.Services.Ai.StructuredOutput;

namespace StudyMentorApi.LearningContent;

public static class LearningContentEndpoints
{
    public static void MapLearningContentEndpoints(this IEndpointRouteBuilder routes)
    {
        routes
            .MapPost("/api/learning-content/generate", Generate)
            .WithTags("LearningContent");
    }

    private static async Task<IResult> Generate(
        GenerateLearningContentRequest request,
        LearningContentGenerationService service,
        CancellationToken cancellationToken)
    {
        try
        {
            var response = await service.GenerateAsync(request, cancellationToken);
            return Results.Ok(response);
        }
        catch (ValidationException ex)
        {
            return Results.BadRequest(new { error = ex.Message });
        }
        catch (NotFoundException ex)
        {
            return Results.NotFound(new { error = ex.Message });
        }
        catch (AiGenerationFailedException ex)
        {
            return Results.Json(
                new { error = ex.Message },
                statusCode: StatusCodes.Status500InternalServerError);
        }
        catch (AiStructuredOutputParseException ex)
        {
            return Results.Json(
                new { error = ex.Message },
                statusCode: StatusCodes.Status500InternalServerError);
        }
    }
}
