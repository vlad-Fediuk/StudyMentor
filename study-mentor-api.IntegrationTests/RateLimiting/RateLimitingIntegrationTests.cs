using System.IdentityModel.Tokens.Jwt;
using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Security.Claims;
using System.Text;
using System.Text.Json;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.IdentityModel.Tokens;
using StudyMentorApi.Data;
using StudyMentorApi.Data.Models;

namespace StudyMentorApi.IntegrationTests.RateLimiting;

public class RateLimitingIntegrationTests
{
    private string GenerateJwt(string key, string issuer, string audience, Guid userId, string[] roles)
    {
        var tokenHandler = new JwtSecurityTokenHandler();
        var keyBytes = Encoding.UTF8.GetBytes(key);

        var claims = new List<Claim>
        {
            new(ClaimTypes.NameIdentifier, userId.ToString()),
            new(ClaimTypes.Name, "TestUser"),
            new(ClaimTypes.Email, "test@test.com"),
            new(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString())
        };
        claims.AddRange(roles.Select(r => new Claim(ClaimTypes.Role, r)));

        var tokenDescriptor = new SecurityTokenDescriptor
        {
            Subject = new ClaimsIdentity(claims),
            Audience = audience,
            Issuer = issuer,
            Expires = DateTime.UtcNow.AddMinutes(60),
            SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(keyBytes), SecurityAlgorithms.HmacSha256Signature)
        };

        var token = tokenHandler.CreateToken(tokenDescriptor);
        return tokenHandler.WriteToken(token);
    }

    private WebApplicationFactory<Program> CreateTestFactory(bool enabled, int aiLimit, int apiLimit, int authLimit, int windowSeconds = 60)
    {
        var dbName = "TestRateLimitDb_" + Guid.NewGuid();

        return new WebApplicationFactory<Program>().WithWebHostBuilder(builder =>
        {
            builder.UseEnvironment("Testing");
            builder.UseSetting("Testing:DatabaseName", dbName);
            builder.UseSetting("RateLimiting:Enabled", enabled ? "true" : "false");
            builder.UseSetting("RateLimiting:Ai:PermitLimit", aiLimit.ToString());
            builder.UseSetting("RateLimiting:Ai:WindowSeconds", windowSeconds.ToString());
            builder.UseSetting("RateLimiting:Api:PermitLimit", apiLimit.ToString());
            builder.UseSetting("RateLimiting:Api:WindowSeconds", windowSeconds.ToString());
            builder.UseSetting("RateLimiting:Auth:PermitLimit", authLimit.ToString());
            builder.UseSetting("RateLimiting:Auth:WindowSeconds", windowSeconds.ToString());
        });
    }

    private (Guid userId, Guid sessionId) SeedChatSession(WebApplicationFactory<Program> factory)
    {
        using var scope = factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();

        var group = new Group { Name = "RateLimit Group" };
        db.Groups.Add(group);
        db.SaveChanges();

        var user = new User
        {
            Name = "RateLimit User",
            Email = $"user_{Guid.NewGuid():N}@test.com",
            Password = "Password123",
            GroupId = group.Id,
            Roles = ["User"]
        };
        db.Users.Add(user);

        var major = new Major { Name = "CS" };
        db.Majors.Add(major);
        db.SaveChanges();

        var subject = new Subject { Name = "Math", MajorId = major.Id };
        db.Subjects.Add(subject);
        db.SaveChanges();

        var lecture = new Lecture { Name = "L1", SubjectId = subject.Id };
        db.Lectures.Add(lecture);
        db.SaveChanges();

        var session = new ChatSession
        {
            UserId = user.Id,
            LectureId = lecture.Id
        };
        db.ChatSessions.Add(session);
        db.SaveChanges();

        return (user.Id, session.Id);
    }

    [Test]
    public async Task AiPolicy_WhenLimitExceeded_Returns429TooManyRequestsAndRetryAfterHeader()
    {
        using var factory = CreateTestFactory(enabled: true, aiLimit: 3, apiLimit: 100, authLimit: 100);
        using var client = factory.CreateClient();

        var (userId, sessionId) = SeedChatSession(factory);

        var config = factory.Services.GetRequiredService<IConfiguration>();
        var key = config["Jwt:Campus:Key"] ?? "TestingDefaultSecretKeyMinimum32CharactersLong!";
        var issuer = config["Jwt:Campus:Issuer"] ?? "StudyMentorApi";
        var audience = config["Jwt:Campus:Audience"] ?? "StudyMentorClient";

        var token = GenerateJwt(key, issuer, audience, userId, ["User"]);
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

        for (var i = 0; i < 3; i++)
        {
            var okResponse = await client.GetAsync($"/api/ai-chat/messages?chatId={sessionId}");
            Assert.That(okResponse.StatusCode, Is.EqualTo(HttpStatusCode.OK));
        }

        var throttledResponse = await client.GetAsync($"/api/ai-chat/messages?chatId={sessionId}");
        Assert.That(throttledResponse.StatusCode, Is.EqualTo(HttpStatusCode.TooManyRequests));

        Assert.That(throttledResponse.Headers.Contains("Retry-After"), Is.True);
        var retryAfter = throttledResponse.Headers.GetValues("Retry-After").FirstOrDefault();
        Assert.That(retryAfter, Is.Not.Null);
        Assert.That(int.TryParse(retryAfter, out var seconds), Is.True);
        Assert.That(seconds, Is.GreaterThan(0));

        var bodyJson = await throttledResponse.Content.ReadFromJsonAsync<JsonElement>();
        Assert.That(bodyJson.TryGetProperty("error", out var errorProp), Is.True);
        Assert.That(errorProp.GetString(), Is.EqualTo("Too many requests"));
        Assert.That(bodyJson.TryGetProperty("retryAfterSeconds", out var retryAfterProp), Is.True);
        Assert.That(retryAfterProp.GetInt32(), Is.GreaterThan(0));
    }

    [Test]
    public async Task WindowRecovery_AfterWindowExpires_AllowsRequestsAgain()
    {
        using var factory = CreateTestFactory(enabled: true, aiLimit: 1, apiLimit: 100, authLimit: 100, windowSeconds: 2);
        using var client = factory.CreateClient();

        var (userId, sessionId) = SeedChatSession(factory);

        var config = factory.Services.GetRequiredService<IConfiguration>();
        var key = config["Jwt:Campus:Key"] ?? "TestingDefaultSecretKeyMinimum32CharactersLong!";
        var issuer = config["Jwt:Campus:Issuer"] ?? "StudyMentorApi";
        var audience = config["Jwt:Campus:Audience"] ?? "StudyMentorClient";

        var token = GenerateJwt(key, issuer, audience, userId, ["User"]);
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

        var firstResponse = await client.GetAsync($"/api/ai-chat/messages?chatId={sessionId}");
        Assert.That(firstResponse.StatusCode, Is.EqualTo(HttpStatusCode.OK));

        var throttledResponse = await client.GetAsync($"/api/ai-chat/messages?chatId={sessionId}");
        Assert.That(throttledResponse.StatusCode, Is.EqualTo(HttpStatusCode.TooManyRequests));
        Assert.That(throttledResponse.Headers.Contains("Retry-After"), Is.True);
        var retryAfterValue = int.Parse(throttledResponse.Headers.GetValues("Retry-After").First());
        Assert.That(retryAfterValue, Is.InRange(1, 2));

        await Task.Delay(TimeSpan.FromSeconds(2.5));

        var recoveredResponse = await client.GetAsync($"/api/ai-chat/messages?chatId={sessionId}");
        Assert.That(recoveredResponse.StatusCode, Is.EqualTo(HttpStatusCode.OK));
    }

    [Test]
    public async Task PartitionIsolation_DifferentUsers_HaveIndependentLimits()
    {
        using var factory = CreateTestFactory(enabled: true, aiLimit: 2, apiLimit: 100, authLimit: 100);
        using var clientA = factory.CreateClient();
        using var clientB = factory.CreateClient();

        var (userAId, sessionAId) = SeedChatSession(factory);
        var (userBId, sessionBId) = SeedChatSession(factory);

        var config = factory.Services.GetRequiredService<IConfiguration>();
        var key = config["Jwt:Campus:Key"] ?? "TestingDefaultSecretKeyMinimum32CharactersLong!";
        var issuer = config["Jwt:Campus:Issuer"] ?? "StudyMentorApi";
        var audience = config["Jwt:Campus:Audience"] ?? "StudyMentorClient";

        var tokenA = GenerateJwt(key, issuer, audience, userAId, ["User"]);
        var tokenB = GenerateJwt(key, issuer, audience, userBId, ["User"]);

        clientA.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", tokenA);
        clientB.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", tokenB);

        for (var i = 0; i < 2; i++)
        {
            var res = await clientA.GetAsync($"/api/ai-chat/messages?chatId={sessionAId}");
            Assert.That(res.StatusCode, Is.EqualTo(HttpStatusCode.OK));
        }

        var throttledA = await clientA.GetAsync($"/api/ai-chat/messages?chatId={sessionAId}");
        Assert.That(throttledA.StatusCode, Is.EqualTo(HttpStatusCode.TooManyRequests));

        var resB = await clientB.GetAsync($"/api/ai-chat/messages?chatId={sessionBId}");
        Assert.That(resB.StatusCode, Is.EqualTo(HttpStatusCode.OK));
    }

    [Test]
    public async Task RateLimitingDisabled_WhenDisabledViaConfig_DoesNotThrottle()
    {
        using var factory = CreateTestFactory(enabled: false, aiLimit: 2, apiLimit: 2, authLimit: 2);
        using var client = factory.CreateClient();

        var (userId, sessionId) = SeedChatSession(factory);

        var config = factory.Services.GetRequiredService<IConfiguration>();
        var key = config["Jwt:Campus:Key"] ?? "TestingDefaultSecretKeyMinimum32CharactersLong!";
        var issuer = config["Jwt:Campus:Issuer"] ?? "StudyMentorApi";
        var audience = config["Jwt:Campus:Audience"] ?? "StudyMentorClient";

        var token = GenerateJwt(key, issuer, audience, userId, ["User"]);
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

        for (var i = 0; i < 6; i++)
        {
            var response = await client.GetAsync($"/api/ai-chat/messages?chatId={sessionId}");
            Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.OK));
        }
    }

    [Test]
    public async Task AuthPolicy_WhenLimitExceeded_Returns429TooManyRequests()
    {
        using var factory = CreateTestFactory(enabled: true, aiLimit: 100, apiLimit: 100, authLimit: 2);
        using var client = factory.CreateClient();

        for (var i = 0; i < 2; i++)
        {
            var response = await client.PostAsJsonAsync("/auth", new
            {
                token = "invalid_microsoft_id_token"
            });
            Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.Unauthorized));
        }

        var throttledResponse = await client.PostAsJsonAsync("/auth", new
        {
            token = "invalid_microsoft_id_token"
        });

        Assert.That(throttledResponse.StatusCode, Is.EqualTo(HttpStatusCode.TooManyRequests));
        Assert.That(throttledResponse.Headers.Contains("Retry-After"), Is.True);
    }

    [Test]
    public async Task PublicEndpoint_Lectures_IsNotThrottledByRateLimiter()
    {
        using var factory = CreateTestFactory(enabled: true, aiLimit: 2, apiLimit: 2, authLimit: 2);
        using var client = factory.CreateClient();

        for (var i = 0; i < 6; i++)
        {
            var response = await client.GetAsync("/lectures");
            Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.OK));
        }
    }
}
