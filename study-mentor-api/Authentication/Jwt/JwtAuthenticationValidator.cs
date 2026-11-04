using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using Microsoft.IdentityModel.Protocols;
using Microsoft.IdentityModel.Protocols.OpenIdConnect;
using Microsoft.IdentityModel.Tokens;

namespace StudyMentorApi.Authentication.Jwt;

public class JwtAuthenticationValidator
{
    private readonly ConfigurationManager<OpenIdConnectConfiguration>? _configurationManager;
    private readonly TokenValidationParameters? _validationParameters;

    public JwtAuthenticationValidator()
    {
    }

    public JwtAuthenticationValidator(IConfiguration configuration)
    {
        var authority = configuration["Jwt:Microsoft:Authority"]
            ?? throw new InvalidOperationException("Jwt:Microsoft:Authority is not configured.");
        var audience = configuration["Jwt:Microsoft:Audience"]
            ?? throw new InvalidOperationException("Jwt:Microsoft:Audience is not configured.");

        _configurationManager = new ConfigurationManager<OpenIdConnectConfiguration>(
            $"{authority}/.well-known/openid-configuration",
            new OpenIdConnectConfigurationRetriever());

        _validationParameters = new TokenValidationParameters
        {
            ValidIssuer = authority,
            ValidAudience = audience,
            ValidateIssuerSigningKey = true,
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true,
            ClockSkew = TimeSpan.FromMinutes(1)
        };
    }

    public virtual async Task<ClaimsPrincipal> ValidateIdTokenAsync(string idToken)
    {
        if (_configurationManager == null || _validationParameters == null)
        {
            throw new InvalidOperationException("Validator is not configured.");
        }

        var openIdConfig = await _configurationManager.GetConfigurationAsync();
        _validationParameters.IssuerSigningKeys = openIdConfig.SigningKeys;

        return new JwtSecurityTokenHandler().ValidateToken(idToken, _validationParameters, out _);
    }

    public static string ExtractEmail(ClaimsPrincipal principal)
    {
        var email = principal.FindFirstValue("preferred_username")?.Trim()
            ?? principal.FindFirstValue("email")?.Trim()
            ?? principal.FindFirstValue("upn")?.Trim();

        if (string.IsNullOrWhiteSpace(email))
        {
            throw new AuthenticationException("Email claim is missing in the Microsoft token.");
        }

        return email.ToLowerInvariant();
    }
}
