using System.Security.Claims;
using StudyMentorApi.ChatSessions;
using StudyMentorApi.Common;
using StudyMentorApi.Data.Models;
using StudyMentorApi.Extensions;

namespace StudyMentorApi.ChatMessages;

public static class ChatMessageEndpoints
{
    public static void MapChatMessageEndpoints(this IEndpointRouteBuilder routes)
    {
        var group = routes
            .MapGroup("/chat-messages")
            .WithTags("ChatMessages")
            .RequireAuthorization("user");

        group.MapGet("/", GetAll).RequireAuthorization("admin");
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
        Guid id,
        ClaimsPrincipal user,
        ChatMessageService messageService,
        ChatSessionService sessionService,
        CancellationToken ct)
    {
        var currentUserId = user.GetUserId();
        if (currentUserId == null)
        {
            return Results.Unauthorized();
        }

        ChatMessage item;
        try
        {
            item = await messageService.GetByIdAsync(id, ct);
        }
        catch (NotFoundException ex)
        {
            return Results.NotFound(new { error = ex.Message });
        }

        ChatSession session;
        try
        {
            session = await sessionService.GetByIdAsync(item.ChatSessionId, ct);
        }
        catch (NotFoundException)
        {
            return Results.NotFound(new { error = "Associated chat session was not found." });
        }

        if (session.UserId != currentUserId && !user.IsAdmin())
        {
            return Results.Forbid();
        }

        return Results.Ok(ToResponse(item));
    }

    private static async Task<IResult> GetBySession(
        Guid sessionId,
        ClaimsPrincipal user,
        ChatMessageService messageService,
        ChatSessionService sessionService,
        CancellationToken ct)
    {
        var currentUserId = user.GetUserId();
        if (currentUserId == null)
        {
            return Results.Unauthorized();
        }

        ChatSession session;
        try
        {
            session = await sessionService.GetByIdAsync(sessionId, ct);
        }
        catch (NotFoundException ex)
        {
            return Results.NotFound(new { error = ex.Message });
        }

        if (session.UserId != currentUserId && !user.IsAdmin())
        {
            return Results.Forbid();
        }

        var items = await messageService.GetBySessionAsync(sessionId, ct);
        return Results.Ok(items.Select(ToResponse));
    }

    private static async Task<IResult> Create(
        ChatMessageRequest request,
        ClaimsPrincipal user,
        ChatMessageService messageService,
        ChatSessionService sessionService,
        CancellationToken ct)
    {
        var currentUserId = user.GetUserId();
        if (currentUserId == null)
        {
            return Results.Unauthorized();
        }

        ChatSession session;
        try
        {
            session = await sessionService.GetByIdAsync(request.ChatSessionId, ct);
        }
        catch (NotFoundException ex)
        {
            return Results.NotFound(new { error = ex.Message });
        }

        if (session.UserId != currentUserId && !user.IsAdmin())
        {
            return Results.Forbid();
        }

        try
        {
            var entity = new ChatMessage
            {
                ChatSessionId = request.ChatSessionId,
                Content = request.Content,
                Timestamp = NormalizeTimestamp(request.Timestamp),
                Role = request.Role,
                SequenceNumber = request.SequenceNumber,
                Status = "completed"
            };
            var created = await messageService.CreateAsync(entity, ct);
            return Results.Created($"/chat-messages/{created.Id}", ToResponse(created));
        }
        catch (ValidationException ex)
        {
            return Results.BadRequest(new { error = ex.Message });
        }
    }

    private static async Task<IResult> Update(
        Guid id,
        ChatMessageRequest request,
        ClaimsPrincipal user,
        ChatMessageService messageService,
        ChatSessionService sessionService,
        CancellationToken ct)
    {
        var currentUserId = user.GetUserId();
        if (currentUserId == null)
        {
            return Results.Unauthorized();
        }

        ChatMessage existingMessage;
        try
        {
            existingMessage = await messageService.GetByIdAsync(id, ct);
        }
        catch (NotFoundException ex)
        {
            return Results.NotFound(new { error = ex.Message });
        }

        ChatSession originalSession;
        try
        {
            originalSession = await sessionService.GetByIdAsync(existingMessage.ChatSessionId, ct);
        }
        catch (NotFoundException)
        {
            return Results.NotFound(new { error = "Associated chat session was not found." });
        }

        var isAdmin = user.IsAdmin();
        if (originalSession.UserId != currentUserId && !isAdmin)
        {
            return Results.Forbid();
        }

        var targetSessionId = existingMessage.ChatSessionId;
        if (request.ChatSessionId != existingMessage.ChatSessionId)
        {
            if (!isAdmin)
            {
                targetSessionId = existingMessage.ChatSessionId;
            }
            else
            {
                try
                {
                    await sessionService.GetByIdAsync(request.ChatSessionId, ct);
                    targetSessionId = request.ChatSessionId;
                }
                catch (NotFoundException ex)
                {
                    return Results.NotFound(new { error = ex.Message });
                }
            }
        }

        try
        {
            var entity = new ChatMessage
            {
                ChatSessionId = targetSessionId,
                Content = request.Content,
                Timestamp = NormalizeTimestamp(request.Timestamp),
                Role = request.Role,
                SequenceNumber = request.SequenceNumber,
                Status = "completed"
            };
            var updated = await messageService.UpdateAsync(id, entity, ct);
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
    }

    private static async Task<IResult> Delete(
        Guid id,
        ClaimsPrincipal user,
        ChatMessageService messageService,
        ChatSessionService sessionService,
        CancellationToken ct)
    {
        var currentUserId = user.GetUserId();
        if (currentUserId == null)
        {
            return Results.Unauthorized();
        }

        ChatMessage existingMessage;
        try
        {
            existingMessage = await messageService.GetByIdAsync(id, ct);
        }
        catch (NotFoundException ex)
        {
            return Results.NotFound(new { error = ex.Message });
        }

        ChatSession session;
        try
        {
            session = await sessionService.GetByIdAsync(existingMessage.ChatSessionId, ct);
        }
        catch (NotFoundException)
        {
            return Results.NotFound(new { error = "Associated chat session was not found." });
        }

        if (session.UserId != currentUserId && !user.IsAdmin())
        {
            return Results.Forbid();
        }

        await messageService.DeleteAsync(id, ct);
        return Results.NoContent();
    }

    private static ChatMessageResponse ToResponse(ChatMessage m) =>
        new(m.Id, m.ChatSessionId, m.Content, m.Timestamp, m.Role, m.SequenceNumber, m.Status);

    private static DateTime NormalizeTimestamp(DateTime? timestamp) =>
        timestamp?.ToUniversalTime() ?? DateTime.UtcNow;
}
