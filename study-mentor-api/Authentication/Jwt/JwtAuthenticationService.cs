using Microsoft.IdentityModel.Tokens;
using System.Security.Claims;
using StudyMentorApi.Data.Models;
using StudyMentorApi.Users;

namespace StudyMentorApi.Authentication.Jwt;

public class JwtAuthenticationService(
    UserService userService,
    JwtAuthenticationValidator jwtAuthenticationValidator,
    IJwtTokenService jwtTokenService) : IJwtAuthenticationService
{
    public async Task<string> AuthenticateAsync(JwtAuthenticationRequest request)
    {
        var principal = await ValidateTokenAndGetPrincipalAsync(request.Token);
        var email = ExtractAndValidateEmail(principal);
        var user = await GetOrCreateUserByEmailAsync(email, principal);

        return await jwtTokenService.CreateTokenAsync(user);
    }

    private async Task<ClaimsPrincipal> ValidateTokenAndGetPrincipalAsync(string token)
    {
        try
        {
            var principal = await jwtAuthenticationValidator.ValidateIdTokenAsync(token);
            if (principal.Identity == null || !principal.Identity.IsAuthenticated)
            {
                throw new AuthenticationException("Invalid Microsoft ID token or user not authenticated.");
            }

            return principal;
        }
        catch (Exception ex) when (ex is SecurityTokenException or ArgumentException)
        {
            throw new AuthenticationException("Invalid Microsoft ID token.");
        }
    }

    private static string ExtractAndValidateEmail(ClaimsPrincipal principal)
    {
        var email = principal.FindFirstValue("preferred_username")?.Trim()
            ?? principal.FindFirstValue("email")?.Trim()
            ?? principal.FindFirstValue("upn")?.Trim();

        if (string.IsNullOrWhiteSpace(email))
        {
            throw new AuthenticationException("Email claim is missing in the Microsoft token.");
        }

        return email;
    }

    private async Task<User> GetOrCreateUserByEmailAsync(string email, ClaimsPrincipal principal)
    {
        var user = await userService.GetByEmailAsync(email, CancellationToken.None);
        if (user is not null)
        {
            return user;
        }

        var name = principal.FindFirstValue("name")?.Trim()
            ?? principal.FindFirstValue(ClaimTypes.Name)?.Trim()
            ?? email;

        return await userService.CreateAsync(
            new User
            {
                Name = name,
                Email = email,
                Password = string.Empty,
                GroupId = string.Empty,
                Roles = ["User"]
            },
            CancellationToken.None);
    }
}
