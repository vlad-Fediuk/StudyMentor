using System.Globalization;
using System.Security.Claims;
using System.Threading.RateLimiting;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.RateLimiting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace StudyMentorApi.Extensions;

public static class RateLimitingExtensions
{
    public static IServiceCollection AddRateLimiting(this IServiceCollection services, IConfiguration configuration)
    {
        var enabled = configuration.GetValue<bool?>("RateLimiting:Enabled") ?? true;

        var apiPermitLimit = configuration.GetValue<int?>("RateLimiting:Api:PermitLimit")
            ?? configuration.GetValue<int?>("RateLimiting:Policies:Api:PermitLimit")
            ?? 100;
        var apiWindowSeconds = configuration.GetValue<int?>("RateLimiting:Api:WindowSeconds")
            ?? configuration.GetValue<int?>("RateLimiting:Policies:Api:WindowSeconds")
            ?? 60;

        var aiPermitLimit = configuration.GetValue<int?>("RateLimiting:Ai:PermitLimit")
            ?? configuration.GetValue<int?>("RateLimiting:Policies:Ai:PermitLimit")
            ?? 10;
        var aiWindowSeconds = configuration.GetValue<int?>("RateLimiting:Ai:WindowSeconds")
            ?? configuration.GetValue<int?>("RateLimiting:Policies:Ai:WindowSeconds")
            ?? 60;

        var authPermitLimit = configuration.GetValue<int?>("RateLimiting:Auth:PermitLimit")
            ?? configuration.GetValue<int?>("RateLimiting:Policies:Auth:PermitLimit")
            ?? 10;
        var authWindowSeconds = configuration.GetValue<int?>("RateLimiting:Auth:WindowSeconds")
            ?? configuration.GetValue<int?>("RateLimiting:Policies:Auth:WindowSeconds")
            ?? 60;

        if (apiPermitLimit <= 0)
        {
            throw new InvalidOperationException("RateLimiting:Api:PermitLimit must be greater than zero.");
        }
        if (apiWindowSeconds <= 0)
        {
            throw new InvalidOperationException("RateLimiting:Api:WindowSeconds must be greater than zero.");
        }
        if (aiPermitLimit <= 0)
        {
            throw new InvalidOperationException("RateLimiting:Ai:PermitLimit must be greater than zero.");
        }
        if (aiWindowSeconds <= 0)
        {
            throw new InvalidOperationException("RateLimiting:Ai:WindowSeconds must be greater than zero.");
        }
        if (authPermitLimit <= 0)
        {
            throw new InvalidOperationException("RateLimiting:Auth:PermitLimit must be greater than zero.");
        }
        if (authWindowSeconds <= 0)
        {
            throw new InvalidOperationException("RateLimiting:Auth:WindowSeconds must be greater than zero.");
        }

        services.AddRateLimiter(options =>
        {
            options.RejectionStatusCode = StatusCodes.Status429TooManyRequests;

            options.OnRejected = async (context, token) =>
            {
                context.HttpContext.Response.StatusCode = StatusCodes.Status429TooManyRequests;
                context.HttpContext.Response.ContentType = "application/json";

                var retryAfterSeconds = 60;
                if (context.Lease.TryGetMetadata(MetadataName.RetryAfter, out var retryAfter))
                {
                    retryAfterSeconds = Math.Max(1, (int)Math.Ceiling(retryAfter.TotalSeconds));
                }

                context.HttpContext.Response.Headers.RetryAfter = retryAfterSeconds.ToString(CultureInfo.InvariantCulture);

                await context.HttpContext.Response.WriteAsJsonAsync(new
                {
                    error = "Too many requests",
                    retryAfterSeconds
                }, token);
            };

            options.AddPolicy("api", httpContext =>
            {
                if (!enabled)
                {
                    return RateLimitPartition.GetNoLimiter("api_disabled");
                }

                var key = httpContext.User.FindFirst(ClaimTypes.NameIdentifier)?.Value
                    ?? httpContext.Connection.RemoteIpAddress?.ToString()
                    ?? "anonymous_api";

                return RateLimitPartition.GetFixedWindowLimiter(key, _ => new FixedWindowRateLimiterOptions
                {
                    PermitLimit = apiPermitLimit,
                    Window = TimeSpan.FromSeconds(apiWindowSeconds),
                    QueueProcessingOrder = QueueProcessingOrder.OldestFirst,
                    QueueLimit = 0
                });
            });

            options.AddPolicy("ai", httpContext =>
            {
                if (!enabled)
                {
                    return RateLimitPartition.GetNoLimiter("ai_disabled");
                }

                var key = httpContext.User.FindFirst(ClaimTypes.NameIdentifier)?.Value
                    ?? httpContext.Connection.RemoteIpAddress?.ToString()
                    ?? "anonymous_ai";

                return RateLimitPartition.GetFixedWindowLimiter(key, _ => new FixedWindowRateLimiterOptions
                {
                    PermitLimit = aiPermitLimit,
                    Window = TimeSpan.FromSeconds(aiWindowSeconds),
                    QueueProcessingOrder = QueueProcessingOrder.OldestFirst,
                    QueueLimit = 0
                });
            });

            options.AddPolicy("auth", httpContext =>
            {
                if (!enabled)
                {
                    return RateLimitPartition.GetNoLimiter("auth_disabled");
                }

                var key = httpContext.Connection.RemoteIpAddress?.ToString()
                    ?? "anonymous_auth";

                return RateLimitPartition.GetFixedWindowLimiter(key, _ => new FixedWindowRateLimiterOptions
                {
                    PermitLimit = authPermitLimit,
                    Window = TimeSpan.FromSeconds(authWindowSeconds),
                    QueueProcessingOrder = QueueProcessingOrder.OldestFirst,
                    QueueLimit = 0
                });
            });
        });

        return services;
    }

    public static IApplicationBuilder UseRateLimiting(this IApplicationBuilder app)
    {
        return app.UseRateLimiter();
    }
}
