using System.Net;
using System.Net.Http.Json;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.IdentityModel.Tokens;
using Moq;
using StudyMentorApi.Authentication.Jwt;
using StudyMentorApi.Data;

namespace study_mentor_api.IntegrationTests.GenerationEndpoints;

public class CustomWebApplicationFactory : WebApplicationFactory<StudyMentorApi.Program>
{
    private readonly string _databaseName = Guid.NewGuid().ToString();

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.UseEnvironment("Development");

        builder.ConfigureServices(services =>
        {
            var dbDescriptors = services.Where(d =>
                d.ServiceType == typeof(DbContextOptions<AppDbContext>) ||
                d.ServiceType == typeof(DbContextOptions)).ToList();

            foreach (var descriptor in dbDescriptors)
            {
                services.Remove(descriptor);
            }

            var inMemoryProvider = new ServiceCollection()
                .AddEntityFrameworkInMemoryDatabase()
                .BuildServiceProvider();

            services.AddDbContext<AppDbContext>(options =>
            {
                options.UseInMemoryDatabase(_databaseName);
                options.UseInternalServiceProvider(inMemoryProvider);
            });

            var validatorDescriptors = services.Where(d =>
                d.ServiceType == typeof(JwtAuthenticationValidator)).ToList();

            foreach (var descriptor in validatorDescriptors)
            {
                services.Remove(descriptor);
            }

            var validatorMock = new Mock<JwtAuthenticationValidator>();
            validatorMock
                .Setup(v => v.ValidateIdTokenAsync(It.IsAny<string>()))
                .ThrowsAsync(new SecurityTokenException("Malformed token format."));

            services.AddSingleton(validatorMock.Object);
        });
    }
}

[TestFixture]
public class GenerationIntegrationTests
{
    private CustomWebApplicationFactory _factory = null!;
    private HttpClient _client = null!;

    [OneTimeSetUp]
    public void OneTimeSetUp()
    {
        _factory = new CustomWebApplicationFactory();
        _client = _factory.CreateClient();
    }

    [OneTimeTearDown]
    public void OneTimeTearDown()
    {
        _client.Dispose();
        _factory.Dispose();
    }

    [Test]
    public async Task PostGenerateLearningContent_ShouldReturn400BadRequest_WhenPayloadInvalid()
    {
        var payload = new
        {
            type = "wrong",
            count = 0,
            message = "Some prompt"
        };

        var response = await _client.PostAsJsonAsync("/api/learning-content/generate", payload);

        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.BadRequest));
        var content = await response.Content.ReadAsStringAsync();
        Assert.That(content, Does.Contain("type must be either 'test' or 'flashcard'."));
    }

    [Test]
    public async Task PostAuth_ShouldReturn401Unauthorized_WhenTokenIsMalformed()
    {
        var request = new JwtAuthenticationRequest("invalid_malformed_token");

        var response = await _client.PostAsJsonAsync("/auth", request);

        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.Unauthorized));
        var content = await response.Content.ReadAsStringAsync();
        Assert.That(content, Does.Contain("Invalid Microsoft ID token."));
    }

    [Test]
    public async Task PostGenerateChatContent_ShouldReturn400BadRequest_WhenExplicitGenerationIsFalse()
    {
        var payload = new
        {
            explicitGeneration = false,
            type = "test",
            chatId = Guid.NewGuid().ToString(),
            message = "Generate test"
        };

        var response = await _client.PostAsJsonAsync("/api/learning-content/generate/chat", payload);

        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.BadRequest));
        var content = await response.Content.ReadAsStringAsync();
        Assert.That(content, Does.Contain("explicitGeneration must be true for chat learning-content generation."));
    }
}
