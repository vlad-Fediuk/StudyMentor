using StudyMentorApi.Common;
using StudyMentorApi.Data.Models;

namespace StudyMentorApi.ChatMessages;

public static class ChatMessageEndpoints
{
    public static void MapChatMessageEndpoints(this IEndpointRouteBuilder routes)
    {
        var group = routes
            .MapGroup("/chat-messages")
            .WithTags("ChatMessages");

        group.MapGet("/", GetAll);
        group.MapGet("/{id}", GetById);
        group.MapGet("/session/{sessionId}", GetBySession);
        group.MapPost("/", Create);
        group.MapPut("/{id}", Update);
        group.MapDelete("/{id}", Delete);
    }

    private static async Task<IResult> GetAll(
        ChatMessageService service,
        CancellationToken ct)
    {
        try
        {
            var items = await service.GetAllAsync(ct);
            return Results.Ok(items.Select(ToResponse));
        }
        catch (Exception ex)
        {
            return Results.Problem(ex.Message, statusCode: 500);
        }
    }

    private static async Task<IResult> GetById(
        string id,
        ChatMessageService service,
        CancellationToken ct)
    {
        try
        {
            var item = await service.GetByIdAsync(id, ct);
            return Results.Ok(ToResponse(item));
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

    private static async Task<IResult> GetBySession(
        string sessionId,
        ChatMessageService service,
        CancellationToken ct)
    {
        try
        {
            var items = await service.GetBySessionAsync(sessionId, ct);
            return Results.Ok(items.Select(ToResponse));
        }
        catch (Exception ex)
        {
            return Results.Problem(ex.Message, statusCode: 500);
        }
    }

    private static async Task<IResult> Create(
        ChatMessageRequest request,
        ChatMessageService service,
        CancellationToken ct)
    {
        try
        {
            var entity = new ChatMessage
            {
                ChatSessionId = request.ChatSessionId,
                Content = request.Content,
                Timestamp = request.Timestamp ?? DateTime.UtcNow,
                Role = request.Role,
                SequenceNumber = request.SequenceNumber
            };
            var created = await service.CreateAsync(entity, ct);
            return Results.Created($"/chat-messages/{created.Id}", ToResponse(created));
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
        ChatMessageRequest request,
        ChatMessageService service,
        CancellationToken ct)
    {
        try
        {
            var entity = new ChatMessage
            {
                ChatSessionId = request.ChatSessionId,
                Content = request.Content,
                Timestamp = request.Timestamp ?? DateTime.UtcNow,
                Role = request.Role,
                SequenceNumber = request.SequenceNumber
            };
            var updated = await service.UpdateAsync(id, entity, ct);
            return Results.Ok(ToResponse(updated));
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
        ChatMessageService service,
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

    private static ChatMessageResponse ToResponse(ChatMessage m) =>
        new(m.Id, m.ChatSessionId, m.Content, m.Timestamp, m.Role, m.SequenceNumber);
}
