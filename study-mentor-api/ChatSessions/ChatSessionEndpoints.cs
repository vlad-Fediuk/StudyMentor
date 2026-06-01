using StudyMentorApi.Data.Models;

namespace StudyMentorApi.ChatSessions;

public static class ChatSessionEndpoints
{
    public static void MapChatSessionEndpoints(this IEndpointRouteBuilder routes)
    {
        var group = routes
            .MapGroup("/chat-sessions")
            .WithTags("ChatSessions");

        group.MapGet("/", GetAll);
        group.MapGet("/{id}", GetById);
        group.MapGet("/user/{userId}", GetByUser);
        group.MapPost("/", Create);
        group.MapPut("/{id}", Update);
        group.MapDelete("/{id}", Delete);
    }

    private static async Task<IResult> GetAll(
        ChatSessionService service,
        CancellationToken ct)
    {
        var items = await service.GetAllAsync(ct);
        return Results.Ok(items.Select(ToResponse));
    }

    private static async Task<IResult> GetById(
        string id,
        ChatSessionService service,
        CancellationToken ct)
    {
        var item = await service.GetByIdAsync(id, ct);
        return Results.Ok(ToResponse(item));
    }

    private static async Task<IResult> GetByUser(
        string userId,
        ChatSessionService service,
        CancellationToken ct)
    {
        var items = await service.GetByUserAsync(userId, ct);
        return Results.Ok(items.Select(ToResponse));
    }

    private static async Task<IResult> Create(
        ChatSessionRequest request,
        ChatSessionService service,
        CancellationToken ct)
    {
        var entity = new ChatSession
        {
            UserId = request.UserId,
            LectureId = request.LectureId
        };
        var created = await service.CreateAsync(entity, ct);
        return Results.Created($"/chat-sessions/{created.Id}", ToResponse(created));
    }

    private static async Task<IResult> Update(
        string id,
        ChatSessionRequest request,
        ChatSessionService service,
        CancellationToken ct)
    {
        var entity = new ChatSession
        {
            UserId = request.UserId,
            LectureId = request.LectureId
        };
        var updated = await service.UpdateAsync(id, entity, ct);
        return Results.Ok(ToResponse(updated));
    }

    private static async Task<IResult> Delete(
        string id,
        ChatSessionService service,
        CancellationToken ct)
    {
        await service.DeleteAsync(id, ct);
        return Results.NoContent();
    }

    private static ChatSessionResponse ToResponse(ChatSession s) =>
        new(s.Id, s.UserId, s.LectureId);
}
