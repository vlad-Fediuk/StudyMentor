using StudyMentorApi.Common;
using StudyMentorApi.Data.Models;

namespace StudyMentorApi.Tests;

public static class TestEndpoints
{
    public static void MapTestEndpoints(this IEndpointRouteBuilder routes)
    {
        var group = routes
            .MapGroup("/tests")
            .WithTags("Tests");

        group.MapGet("/", GetAll);
        group.MapGet("/{id}", GetById);
        group.MapPost("/", Create);
        group.MapPut("/{id}", Update);
        group.MapDelete("/{id}", Delete);
        group.MapPost("/{testId}/questions/{questionId}/answer", CheckAnswer);
    }

    private static async Task<IResult> GetAll(
        Guid? sourceFlashcardId,
        TestService service,
        CancellationToken ct)
    {
        var items = await service.GetAllAsync(ct);

        if (sourceFlashcardId.HasValue)
        {
            items = items
                .Where(test => test.SourceFlashcardId == sourceFlashcardId.Value)
                .ToList();
        }

        return Results.Ok(items.Select(ToResponse));
    }

    private static async Task<IResult> GetById(
        Guid id,
        TestService service,
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
        TestRequest request,
        TestService service,
        CancellationToken ct)
    {
        try
        {
            var entity = ToEntity(request);
            var created = await service.CreateAsync(entity, ct);
            return Results.Created($"/tests/{created.Id}", ToResponse(created));
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
        TestRequest request,
        TestService service,
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
        TestService service,
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

    private static async Task<IResult> CheckAnswer(
        Guid testId,
        Guid questionId,
        TestAnswerRequest request,
        TestService service,
        CancellationToken ct)
    {
        try
        {
            var result = await service.CheckAnswerAsync(
                testId,
                questionId,
                request.AnswerVariantId,
                ct);

            return Results.Ok(result);
        }
        catch (NotFoundException ex)
        {
            return Results.NotFound(new { error = ex.Message });
        }
    }

    private static Test ToEntity(TestRequest request)
    {
        var questions = (request.Questions ?? [])
            .Where(q => q is not null)
            .Select((q, questionIndex) => new TestQuestion
            {
                Prompt = q.Prompt?.Trim() ?? string.Empty,
                Order = questionIndex,
                AnswerVariants = (q.AnswerVariants ?? [])
                    .Where(a => a is not null)
                    .Select((a, answerIndex) => new TestAnswerVariant
                    {
                        Text = a.Text?.Trim() ?? string.Empty,
                        IsCorrect = a.IsCorrect,
                        Order = answerIndex
                    })
                    .ToList()
            })
            .ToList();

        return new Test
        {
            Name = request.Name?.Trim() ?? string.Empty,
            ChatMessageId = request.ChatMessageId,
            SourceFlashcardId = request.SourceFlashcardId,
            Questions = questions
        };
    }

    private static TestResponse ToResponse(Test test)
    {
        return new TestResponse(
            test.Id,
            test.Name,
            test.ChatMessageId,
            test.SourceFlashcardId,
            test.Questions
                .OrderBy(q => q.Order)
                .Select(q => new TestQuestionResponse(
                    q.Id,
                    q.Prompt,
                    q.AnswerVariants
                        .OrderBy(a => a.Order)
                        .Select(a => new TestAnswerVariantResponse(a.Id, a.Text))
                        .ToList()))
                .ToList());
    }
}
