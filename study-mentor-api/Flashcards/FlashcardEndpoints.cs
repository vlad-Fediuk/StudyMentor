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

        MapV1(group);

        var api = routes.NewVersionedApi("Flashcards");
        var versioned = api.MapGroup("/api/v{version:apiVersion}/flashcards").WithTags("Flashcards");
        MapV1(versioned.MapGroup("").HasApiVersion(1.0));
        var v2 = versioned.MapGroup("").HasApiVersion(2.0);
        v2.MapGet("/", GetPage);
        v2.MapGet("/{id:guid}", GetByIdV2);
    }

    private static void MapV1(RouteGroupBuilder group)
    {
        group.MapGet("/", GetAll);
        group.MapGet("/{id}", GetById);
        group.MapPost("/", Create);
        group.MapPut("/{id}", Update);
        group.MapDelete("/{id}", Delete);
    }

    private static async Task<IResult> GetPage(
        FlashcardService service, CancellationToken ct, int page = 1, int pageSize = 20)
    {
        if (page < 1 || pageSize is < 1 or > 100 || (long)(page - 1) * pageSize > int.MaxValue)
        {
            return Results.BadRequest(new { error = "page must be positive and pageSize must be between 1 and 100." });
        }

        return Results.Ok(await service.GetPageAsync(page, pageSize, ct));
    }

    private static async Task<IResult> GetByIdV2(Guid id, FlashcardService service, CancellationToken ct)
    {
        try
        {
            return Results.Ok(ToV2Response(await service.GetByIdAsync(id, ct)));
        }
        catch (NotFoundException ex)
        {
            return Results.NotFound(new { error = ex.Message });
        }
    }

    internal static FlashcardV2Response ToV2Response(Flashcard flashcard)
    {
        var response = ToResponse(flashcard);
        return new(response.Id, response.Name, response.ChatMessageId, response.Cards, response.Cards.Count);
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
