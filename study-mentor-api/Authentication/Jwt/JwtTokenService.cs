using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.IdentityModel.Tokens;
using StudyMentorApi.Data.Models;

namespace StudyMentorApi.Authentication.Jwt;

public class JwtTokenService(
    IConfiguration configuration) : IJwtTokenService
{
    public async Task<string> CreateTokenAsync(User user)
    {
        await Task.CompletedTask;

        var tokenHandler = new JwtSecurityTokenHandler();
        var jwtKey = configuration["Jwt:Campus:Key"]
            ?? throw new InvalidOperationException("Configuration error: 'Jwt:Campus:Key' is not set.");
        var key = Encoding.UTF8.GetBytes(jwtKey);

        var claims = new List<Claim>
        {
            new(ClaimTypes.NameIdentifier, user.Id),
            new(ClaimTypes.Name, user.Name),
            new(ClaimTypes.Email, user.Email ?? string.Empty),
            new(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString())
        };
        claims.AddRange(GetRoles(user).Select(role => new Claim(ClaimTypes.Role, role)));

        var audience = configuration["Jwt:Campus:Audience"]
            ?? throw new InvalidOperationException("Configuration error: 'Jwt:Campus:Audience' is not set.");

        var tokenDescriptor = new SecurityTokenDescriptor
        {
            Subject = new ClaimsIdentity(claims),
            Audience = audience,
            Expires = DateTime.UtcNow.AddDays(30),
            SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(key), SecurityAlgorithms.HmacSha256Signature),
            Issuer = configuration["Jwt:Campus:Issuer"]
                ?? throw new InvalidOperationException("Configuration error: 'Jwt:Campus:Issuer' is not set.")
        };

        var token = tokenHandler.CreateToken(tokenDescriptor);
        return tokenHandler.WriteToken(token);
    }

    private static IEnumerable<string> GetRoles(User user)
    {
        return (user.Roles ?? [])
            .Where(role => !string.IsNullOrWhiteSpace(role))
            .Select(NormalizeRoleName)
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .DefaultIfEmpty("User");
    }

    private static string NormalizeRoleName(string role)
    {
        var normalizedRole = role.Trim();

        if (normalizedRole.Equals("admin", StringComparison.OrdinalIgnoreCase))
        {
            return "Admin";
        }

        if (normalizedRole.Equals("user", StringComparison.OrdinalIgnoreCase))
        {
            return "User";
        }

        return normalizedRole;
    }
}
