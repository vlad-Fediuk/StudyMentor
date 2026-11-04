using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using StudyMentorApi.Authentication.Jwt;
using StudyMentorApi.Data;
using StudyMentorApi.Data.Models;
using StudyMentorApi.Users;
using System.Security.Cryptography;
using System.Text;

namespace StudyMentorApi.Authentication;

public record RegisterRequest(string Email, string Name, string Password);
public record LoginRequest(string Email, string Password);

public static class AuthenticationEndpoints
{
    public static void MapAuthenticationEndpoints(this IEndpointRouteBuilder routes)
    {
        var group = routes
            .MapGroup("/auth")
            .WithTags("Authentication")
            .AllowAnonymous()
            .RequireRateLimiting("auth");

        group.MapPost("/register", async (
            [FromBody] RegisterRequest request,
            [FromServices] AppDbContext db,
            [FromServices] UserService userService,
            [FromServices] IJwtTokenService jwtTokenService,
            CancellationToken ct) =>
        {
            if (string.IsNullOrWhiteSpace(request.Email) || string.IsNullOrWhiteSpace(request.Password))
            {
                return Results.BadRequest(new { message = "Email and password are required." });
            }

            var normalizedEmail = request.Email.Trim().ToLowerInvariant();
            var existingUser = await db.Users.FirstOrDefaultAsync(u => u.Email == normalizedEmail, ct);
            if (existingUser != null)
            {
                return Results.Conflict(new { message = "User with this email already exists." });
            }

            var defaultGroup = await db.Groups.FirstOrDefaultAsync(ct);
            var groupId = defaultGroup?.Id ?? Guid.NewGuid();
            if (defaultGroup == null)
            {
                var group = new Group
                {
                    Id = groupId,
                    Name = "General"
                };
                db.Groups.Add(group);
                await db.SaveChangesAsync(ct);
            }

            var name = string.IsNullOrWhiteSpace(request.Name) ? normalizedEmail.Split('@')[0] : request.Name.Trim();
            var hashedPassword = HashPassword(request.Password);

            var user = new User
            {
                Name = name,
                Email = normalizedEmail,
                Password = hashedPassword,
                GroupId = groupId,
                Roles = ["User"]
            };

            await userService.CreateAsync(user, ct);
            var token = await jwtTokenService.CreateTokenAsync(user);

            return Results.Ok(new { token, name = user.Name, email = user.Email });
        });

        group.MapPost("/login", async (
            [FromBody] LoginRequest request,
            [FromServices] AppDbContext db,
            [FromServices] IJwtTokenService jwtTokenService,
            CancellationToken ct) =>
        {
            if (string.IsNullOrWhiteSpace(request.Email) || string.IsNullOrWhiteSpace(request.Password))
            {
                return Results.BadRequest(new { message = "Email and password are required." });
            }

            var normalizedEmail = request.Email.Trim().ToLowerInvariant();
            var user = await db.Users.FirstOrDefaultAsync(u => u.Email == normalizedEmail, ct);
            if (user == null)
            {
                return Results.Json(new { message = "Invalid email or password." }, statusCode: StatusCodes.Status401Unauthorized);
            }

            var hashedInput = HashPassword(request.Password);
            if (user.Password != hashedInput)
            {
                return Results.Json(new { message = "Invalid email or password." }, statusCode: StatusCodes.Status401Unauthorized);
            }

            var token = await jwtTokenService.CreateTokenAsync(user);
            return Results.Ok(new { token, name = user.Name, email = user.Email });
        });

        routes.MapPost("/auth", async (
            [FromBody] JwtAuthenticationRequest request,
            [FromServices] IJwtAuthenticationService authService) =>
        {
            return await AuthenticateAsync(() => authService.AuthenticateAsync(request));
        })
        .WithTags("Authentication")
        .AllowAnonymous()
        .RequireRateLimiting("auth");
    }

    private static string HashPassword(string password)
    {
        var bytes = Encoding.UTF8.GetBytes(password);
        var hash = SHA256.HashData(bytes);
        return Convert.ToHexString(hash);
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
        catch (Exception)
        {
            return Results.Json(new { message = "Authentication failed due to an unexpected server error." }, statusCode: StatusCodes.Status500InternalServerError);
        }
    }
}
