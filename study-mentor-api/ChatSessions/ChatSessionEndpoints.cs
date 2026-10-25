using System.Security.Claims;
using StudyMentorApi.Common;
using StudyMentorApi.Data.Models;
using StudyMentorApi.Extensions;

namespace StudyMentorApi.ChatSessions;

public static class ChatSessionEndpoints
{
    public static void MapChatSessionEndpoints(this IEndpointRouteBuilder routes)
    {
        var group = routes
            .MapGroup("/chat-sessions")
            .WithTags("ChatSessions")
            .RequireAuthorization("user")
            .RequireRateLimiting("api");

        group.MapGet("/", GetAll).RequireAuthorization("admin");
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
        Guid id,
        ClaimsPrincipal user,
        ChatSessionService service,
        CancellationToken ct)
    {
        var currentUserId = user.GetUserId();
        if (currentUserId == null)
        {
            return Results.Unauthorized();
        }

        try
        {
            var item = await service.GetByIdAsync(id, ct);
            if (item.UserId != currentUserId && !user.IsAdmin())
            {
                return Results.Forbid();
            }

            return Results.Ok(ToResponse(item));
        }
        catch (NotFoundException ex)
        {
            return Results.NotFound(new { error = ex.Message });
        }
    }

    private static async Task<IResult> GetByUser(
        Guid userId,
        ClaimsPrincipal user,
        ChatSessionService service,
        CancellationToken ct)
    {
        var currentUserId = user.GetUserId();
        if (currentUserId == null)
        {
            return Results.Unauthorized();
        }

        if (userId != currentUserId && !user.IsAdmin())
        {
            return Results.Forbid();
        }

        var items = await service.GetByUserAsync(userId, ct);
        return Results.Ok(items.Select(ToResponse));
    }

    private static async Task<IResult> Create(
        ClaimsPrincipal user,
        ChatSessionRequest request,
        ChatSessionService service,
        CancellationToken ct)
    {
        var currentUserId = user.GetUserId();
        if (currentUserId == null)
        {
            return Results.Unauthorized();
        }

        Guid targetUserId;
        if (user.IsAdmin())
        {
            if (request.UserId == Guid.Empty)
            {
                return Results.BadRequest(new { error = "UserId is required and cannot be empty." });
            }

            targetUserId = request.UserId;
        }
        else
        {
            targetUserId = currentUserId.Value;
        }

        var entity = new ChatSession
        {
            UserId = targetUserId,
            LectureId = request.LectureId
        };
        var created = await service.CreateAsync(entity, ct);
        return Results.Created($"/chat-sessions/{created.Id}", ToResponse(created));
    }

    private static async Task<IResult> Update(
        Guid id,
        ClaimsPrincipal user,
        ChatSessionRequest request,
        ChatSessionService service,
        CancellationToken ct)
    {
        var currentUserId = user.GetUserId();
        if (currentUserId == null)
        {
            return Results.Unauthorized();
        }

        ChatSession existing;
        try
        {
            existing = await service.GetByIdAsync(id, ct);
        }
        catch (NotFoundException ex)
        {
            return Results.NotFound(new { error = ex.Message });
        }

        var isAdmin = user.IsAdmin();
        if (existing.UserId != currentUserId && !isAdmin)
        {
            return Results.Forbid();
        }

        var targetUserId = isAdmin && request.UserId != Guid.Empty
            ? request.UserId
            : existing.UserId;

        var entity = new ChatSession
        {
            UserId = targetUserId,
            LectureId = request.LectureId
        };
        var updated = await service.UpdateAsync(id, entity, ct);
        return Results.Ok(ToResponse(updated));
    }

    private static async Task<IResult> Delete(
        Guid id,
        ClaimsPrincipal user,
        ChatSessionService service,
        CancellationToken ct)
    {
        var currentUserId = user.GetUserId();
        if (currentUserId == null)
        {
            return Results.Unauthorized();
        }

        ChatSession existing;
        try
        {
            existing = await service.GetByIdAsync(id, ct);
        }
        catch (NotFoundException ex)
        {
            return Results.NotFound(new { error = ex.Message });
        }

        if (existing.UserId != currentUserId && !user.IsAdmin())
        {
            return Results.Forbid();
        }

        await service.DeleteAsync(id, ct);
        return Results.NoContent();
    }

    private static ChatSessionResponse ToResponse(ChatSession s) =>
        new(s.Id, s.UserId, s.LectureId);
}
