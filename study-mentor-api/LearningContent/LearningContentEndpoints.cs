using System.Security.Claims;
using StudyMentorApi.ChatSessions;
using StudyMentorApi.Common;
using StudyMentorApi.Extensions;
using StudyMentorApi.Services.Ai;
using StudyMentorApi.Services.Ai.StructuredOutput;

namespace StudyMentorApi.LearningContent;

public static class LearningContentEndpoints
{
    public static void MapLearningContentEndpoints(this IEndpointRouteBuilder routes)
    {
        var group = routes
            .MapGroup("/api/learning-content")
            .WithTags("LearningContent")
            .RequireAuthorization("user")
            .RequireRateLimiting("ai");

        group.MapPost("/generate", Generate);
        group.MapPost("/generate/chat", GenerateForChat);
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
        catch (AiGenerationFailedException)
        {
            return Results.Json(
                new { error = "AI content generation failed." },
                statusCode: StatusCodes.Status500InternalServerError);
        }
        catch (AiStructuredOutputParseException)
        {
            return Results.Json(
                new { error = "Failed to parse structured AI output." },
                statusCode: StatusCodes.Status500InternalServerError);
        }
    }

    private static async Task<IResult> GenerateForChat(
        GenerateLearningContentChatRequest request,
        ClaimsPrincipal user,
        LearningContentGenerationService service,
        ChatSessionService chatSessionService,
        CancellationToken cancellationToken)
    {
        var currentUserId = user.GetUserId();
        if (currentUserId == null)
        {
            return Results.Unauthorized();
        }

        if (!string.IsNullOrWhiteSpace(request.ChatId))
        {
            if (!Guid.TryParse(request.ChatId, out var parsedChatId))
            {
                return Results.BadRequest(new { error = "ChatId must be a valid GUID." });
            }

            try
            {
                var session = await chatSessionService.GetByIdAsync(parsedChatId, cancellationToken);
                if (session.UserId != currentUserId && !user.IsAdmin())
                {
                    return Results.Forbid();
                }
            }
            catch (NotFoundException ex)
            {
                return Results.NotFound(new { error = ex.Message });
            }
        }

        try
        {
            var response = await service.GenerateForChatAsync(request, cancellationToken);
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
    }
}
