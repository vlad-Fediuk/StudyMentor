using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using Microsoft.IdentityModel.Protocols;
using Microsoft.IdentityModel.Protocols.OpenIdConnect;
using Microsoft.IdentityModel.Tokens;

namespace StudyMentorApi.Authentication.Jwt;

public class JwtAuthenticationValidator
{
    private readonly ConfigurationManager<OpenIdConnectConfiguration> _configurationManager;
    private readonly TokenValidationParameters _validationParameters;

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

    public async Task<ClaimsPrincipal> ValidateIdTokenAsync(string idToken)
    {
        var openIdConfig = await _configurationManager.GetConfigurationAsync();
        _validationParameters.IssuerSigningKeys = openIdConfig.SigningKeys;

        return new JwtSecurityTokenHandler().ValidateToken(idToken, _validationParameters, out _);
    }
}
