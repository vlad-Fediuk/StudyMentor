using StudyMentorApi.Extensions;
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
        v2.MapGet("/{id:guid}", GetByIdV2).AddEndpointFilter(new CrudExceptionFilter());
    }

    private static void MapV1(RouteGroupBuilder group)
    {
        group.MapGet("/", GetAll);
        group.MapGet("/{id}", GetById).AddEndpointFilter(new CrudExceptionFilter());
        group.MapPost("/", Create).AddEndpointFilter(new CrudExceptionFilter(handleValidation: true));
        group.MapPut("/{id}", Update).AddEndpointFilter(new CrudExceptionFilter(handleValidation: true));
        group.MapDelete("/{id}", Delete).AddEndpointFilter(new CrudExceptionFilter());
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
        return Results.Ok((await service.GetByIdAsync(id, ct)).ToV2Response());
    }


    private static async Task<IResult> GetAll(
        FlashcardService service,
        CancellationToken ct)
    {
        var items = await service.GetAllAsync(ct);
        return Results.Ok(items.Select(item => item.ToResponse()));
    }

    private static async Task<IResult> GetById(
        Guid id,
        FlashcardService service,
        CancellationToken ct)
    {
        var item = await service.GetByIdAsync(id, ct);
        return Results.Ok(item.ToResponse());
    }

    private static async Task<IResult> Create(
        FlashcardRequest request,
        FlashcardService service,
        CancellationToken ct)
    {
        var entity = ToEntity(request);
        var created = await service.CreateAsync(entity, ct);
        return Results.Created($"/flashcards/{created.Id}", created.ToResponse());
    }

    private static async Task<IResult> Update(
        Guid id,
        FlashcardRequest request,
        FlashcardService service,
        CancellationToken ct)
    {
        var entity = ToEntity(request);
        var updated = await service.UpdateAsync(id, entity, ct);
        return Results.Ok(updated.ToResponse());
    }

    private static async Task<IResult> Delete(
        Guid id,
        FlashcardService service,
        CancellationToken ct)
    {
        await service.DeleteAsync(id, ct);
        return Results.NoContent();
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

}
