using StudyMentorApi.Authentication.Jwt;
using Microsoft.AspNetCore.Mvc;

namespace StudyMentorApi.Authentication;

public static class AuthenticationEndpoints
{
    public static void MapAuthenticationEndpoints(this IEndpointRouteBuilder routes)
    {
        routes.MapPost("/auth", async (
            [FromBody] JwtAuthenticationRequest request,
            [FromServices] IJwtAuthenticationService authService) =>
        {
            return await AuthenticateAsync(() => authService.AuthenticateAsync(request));
        }).WithTags("Authentication").AllowAnonymous();
    }

    private static async Task<IResult> AuthenticateAsync(Func<Task<string>> authenticate)
    {
        try
        {
            var jwt = await authenticate();
            return Results.Ok(new { token = jwt });
        }
        catch (AuthenticationException ex)
        {
            return Results.Json(new { message = ex.Message }, statusCode: StatusCodes.Status401Unauthorized);
        }
        catch (Exception ex)
        {
            return Results.Json(new { message = $"Authentication failed: {ex.Message}" }, statusCode: StatusCodes.Status500InternalServerError);
        }
    }
}
