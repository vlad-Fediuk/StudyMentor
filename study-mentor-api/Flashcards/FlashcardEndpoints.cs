using StudyMentorApi.Common;
using StudyMentorApi.Data.Models;

namespace StudyMentorApi.Flashcards;

public static class FlashcardEndpoints
{
    public static void MapFlashcardEndpoints(this IEndpointRouteBuilder routes)
    {
        var group = routes
            .MapGroup("/flashcards")
            .WithTags("Flashcards");

        group.MapGet("/", GetAll);
        group.MapGet("/{id}", GetById);
        group.MapPost("/", Create);
        group.MapPut("/{id}", Update);
        group.MapDelete("/{id}", Delete);
    }

    private static async Task<IResult> GetAll(
        FlashcardService service,
        CancellationToken ct)
    {
        var items = await service.GetAllAsync(ct);
        return Results.Ok(items.Select(ToResponse));
    }

    private static async Task<IResult> GetById(
        Guid id,
        FlashcardService service,
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
    }

    private static async Task<IResult> Create(
        FlashcardRequest request,
        FlashcardService service,
        CancellationToken ct)
    {
        try
        {
            var entity = ToEntity(request);
            var created = await service.CreateAsync(entity, ct);
            return Results.Created($"/flashcards/{created.Id}", ToResponse(created));
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

    private static async Task<IResult> Update(
        Guid id,
        FlashcardRequest request,
        FlashcardService service,
        CancellationToken ct)
    {
        try
        {
            var entity = ToEntity(request);
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
    }

    private static async Task<IResult> Delete(
        Guid id,
        FlashcardService service,
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
    }

    private static Flashcard ToEntity(FlashcardRequest request)
    {
        return new Flashcard
        {
            Name = request.Name?.Trim() ?? string.Empty,
            ChatMessageId = request.ChatMessageId,
            Cards = (request.Cards ?? [])
                .Where(c => c is not null)
                .Select(c => new Card
                {
                    Term = c.Term?.Trim() ?? string.Empty,
                    Definition = c.Definition?.Trim() ?? string.Empty
                })
                .ToList()
        };
    }

    private static FlashcardResponse ToResponse(Flashcard flashcard)
    {
        return new FlashcardResponse(
            flashcard.Id,
            flashcard.Name,
            flashcard.ChatMessageId,
            flashcard.Cards
                .Select(c => new CardResponse(c.Id, c.Term, c.Definition))
                .ToList());
    }
}
