using StudyMentorApi.Common;

namespace StudyMentorApi.AiChat;

public static class AiChatEndpoints
{
    public static void MapAiChatEndpoints(this IEndpointRouteBuilder routes)
    {
        var group = routes
            .MapGroup("/api/ai-chat")
            .WithTags("AiChat");

        group.MapGet("/messages", GetMessages);
        group.MapPost("/messages", SendMessage);
    }

    private static async Task<IResult> GetMessages(
        string? chatId,
        AiChatService service,
        CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(chatId))
        {
            return Results.BadRequest(new { error = "chatId is required." });
        }

        try
        {
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
        AiChatService service,
        CancellationToken cancellationToken)
    {
        try
        {
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
        catch (AiChatFailedException ex)
        {
            return Results.Json(
                new AiChatErrorResponse("failed", ex.Message),
                statusCode: StatusCodes.Status500InternalServerError);
        }
    }
}
