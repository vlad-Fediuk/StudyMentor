using System.Security.Claims;
using StudyMentorApi.ChatSessions;
using StudyMentorApi.Common;
using StudyMentorApi.Extensions;

namespace StudyMentorApi.AiChat;

public static class AiChatEndpoints
{
    public static void MapAiChatEndpoints(this IEndpointRouteBuilder routes)
    {
        var group = routes
            .MapGroup("/api/ai-chat")
            .WithTags("AiChat")
            .RequireAuthorization("user");

        group.MapGet("/messages", GetMessages);
        group.MapPost("/messages", SendMessage);
    }

    private static async Task<IResult> GetMessages(
        string? chatId,
        ClaimsPrincipal user,
        AiChatService service,
        ChatSessionService chatSessionService,
        CancellationToken cancellationToken)
    {
        var currentUserId = user.GetUserId();
        if (currentUserId == null)
        {
            return Results.Unauthorized();
        }

        if (string.IsNullOrWhiteSpace(chatId))
        {
            return Results.BadRequest(new { error = "chatId is required." });
        }

        if (!Guid.TryParse(chatId, out var parsedChatId))
        {
            return Results.BadRequest(new { error = "chatId must be a valid GUID." });
        }

        try
        {
            var session = await chatSessionService.GetByIdAsync(parsedChatId, cancellationToken);
            if (session.UserId != currentUserId && !user.IsAdmin())
            {
                return Results.Forbid();
            }

            var messages = await service.GetMessagesAsync(chatId, cancellationToken);
            return Results.Ok(messages);
        }
        catch (NotFoundException ex)
        {
            return Results.NotFound(new { error = ex.Message });
        }
    }

    private static async Task<IResult> SendMessage(
        AiChatSendMessageRequest request,
        ClaimsPrincipal user,
        AiChatService service,
        ChatSessionService chatSessionService,
        CancellationToken cancellationToken)
    {
        var currentUserId = user.GetUserId();
        if (currentUserId == null)
        {
            return Results.Unauthorized();
        }

        if (string.IsNullOrWhiteSpace(request.ChatId))
        {
            return Results.BadRequest(new { error = "chatId is required." });
        }

        if (!Guid.TryParse(request.ChatId, out var parsedChatId))
        {
            return Results.BadRequest(new { error = "chatId must be a valid GUID." });
        }

        try
        {
            var session = await chatSessionService.GetByIdAsync(parsedChatId, cancellationToken);
            if (session.UserId != currentUserId && !user.IsAdmin())
            {
                return Results.Forbid();
            }

            var response = await service.SendMessageAsync(request, cancellationToken);
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
        catch (ArgumentException ex)
        {
            return Results.BadRequest(new { error = ex.Message });
        }
        catch (AiChatFailedException)
        {
            return Results.Json(
                new AiChatErrorResponse("failed", "AI chat generation failed."),
                statusCode: StatusCodes.Status500InternalServerError);
        }
    }
}
