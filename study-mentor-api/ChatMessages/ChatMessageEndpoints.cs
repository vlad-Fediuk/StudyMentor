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
        var items = await service.GetAllAsync(ct);
        return Results.Ok(items.Select(ToResponse));
    }

    private static async Task<IResult> GetById(
        string id,
        ChatMessageService service,
        CancellationToken ct)
    {
        var item = await service.GetByIdAsync(id, ct);
        return Results.Ok(ToResponse(item));
    }

    private static async Task<IResult> GetBySession(
        string sessionId,
        ChatMessageService service,
        CancellationToken ct)
    {
        var items = await service.GetBySessionAsync(sessionId, ct);
        return Results.Ok(items.Select(ToResponse));
    }

    private static async Task<IResult> Create(
        ChatMessageRequest request,
        ChatMessageService service,
        CancellationToken ct)
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

    private static async Task<IResult> Update(
        string id,
        ChatMessageRequest request,
        ChatMessageService service,
        CancellationToken ct)
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

    private static async Task<IResult> Delete(
        string id,
        ChatMessageService service,
        CancellationToken ct)
    {
        await service.DeleteAsync(id, ct);
        return Results.NoContent();
    }

    private static ChatMessageResponse ToResponse(ChatMessage m) =>
        new(m.Id, m.ChatSessionId, m.Content, m.Timestamp, m.Role, m.SequenceNumber);
}
