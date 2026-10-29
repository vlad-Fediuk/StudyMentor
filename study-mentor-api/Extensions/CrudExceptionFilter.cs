using StudyMentorApi.Common;

namespace StudyMentorApi.Extensions;

// Keep the existing CRUD error body; the global handler also includes statusCode.
public sealed class CrudExceptionFilter(bool handleValidation = false) : IEndpointFilter
{
    public async ValueTask<object?> InvokeAsync(EndpointFilterInvocationContext context, EndpointFilterDelegate next)
    {
        try
        {
            return await next(context);
        }
        catch (NotFoundException ex)
        {
            return Results.NotFound(new { error = ex.Message });
        }
        catch (ValidationException ex) when (handleValidation)
        {
            return Results.BadRequest(new { error = ex.Message });
        }
    }
}
