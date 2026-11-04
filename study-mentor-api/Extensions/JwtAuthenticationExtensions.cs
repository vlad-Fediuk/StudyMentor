using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.IdentityModel.Tokens;
using System.Security.Claims;
using System.Text;

namespace StudyMentorApi.Extensions;

public static class JwtAuthenticationExtensions
{
    public static AuthenticationBuilder AddJwtAuthentication(this IServiceCollection services, IConfiguration configuration, Microsoft.Extensions.Hosting.IHostEnvironment? environment = null)
    {
        var jwtKey = configuration["Jwt:Campus:Key"];
        if (string.IsNullOrWhiteSpace(jwtKey))
        {
            if (environment?.IsEnvironment("Testing") == true)
            {
                jwtKey = "TestingDefaultSecretKeyMinimum32CharactersLong!";
            }
            else
            {
                throw new InvalidOperationException(
                "Jwt:Campus:Key is not configured. Please configure Jwt__Campus__Key via .env or environment variables.");
            }
        }

        var authenticationBuilder = services.AddAuthentication(options =>
        {
            options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
            options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
        })
        .AddJwtBearer(options =>
        {
            options.TokenValidationParameters = new TokenValidationParameters
            {
                ValidateIssuer = true,
                ValidateAudience = true,
                ValidateLifetime = true,
                ValidateIssuerSigningKey = true,
                ClockSkew = TimeSpan.FromSeconds(30),
                NameClaimType = ClaimTypes.NameIdentifier,
                RoleClaimType = ClaimTypes.Role,
                ValidIssuer = configuration["Jwt:Campus:Issuer"],
                ValidAudience = configuration["Jwt:Campus:Audience"],
                IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtKey))
            };
        });

        services.AddAuthorization(options =>
        {
            options.AddPolicy("user", policy => policy
                .AddAuthenticationSchemes(JwtBearerDefaults.AuthenticationScheme)
                .RequireAuthenticatedUser()
                .RequireRole("User", "Admin"));

            options.AddPolicy("admin", policy => policy
                .AddAuthenticationSchemes(JwtBearerDefaults.AuthenticationScheme)
                .RequireAuthenticatedUser()
                .RequireRole("Admin"));
        });

        return authenticationBuilder;
    }
}
