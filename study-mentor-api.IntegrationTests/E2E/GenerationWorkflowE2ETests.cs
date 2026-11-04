using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Security.Claims;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.IdentityModel.Tokens;
using Moq;
using StudyMentorApi.Authentication.Jwt;
using StudyMentorApi.Data;
using StudyMentorApi.Data.Models;
using StudyMentorApi.LearningContent;
using StudyMentorApi.Services.Ai;
using StudyMentorApi.Services.Ai.Embeddings;
using StudyMentorApi.Services.Ai.StructuredOutput;
using StudyMentorApi.Tests;

namespace study_mentor_api.IntegrationTests.E2E;

public class E2EWebApplicationFactory : WebApplicationFactory<StudyMentorApi.Program>
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

            var aiGenDescriptors = services.Where(d =>
                d.ServiceType == typeof(IAiGenerationService)).ToList();
            foreach (var descriptor in aiGenDescriptors)
            {
                services.Remove(descriptor);
            }

            var aiGenMock = new Mock<IAiGenerationService>();
            aiGenMock
                .Setup(a => a.GenerateStructuredAsync<GeneratedTestDto>(
                    It.IsAny<AiGenerationRequest>(),
                    It.IsAny<CancellationToken>()))
                .ReturnsAsync(new GeneratedTestDto
                {
                    Title = "Тест по лекції з архітектури",
                    Questions = [
                        new GeneratedQuestionDto
                        {
                            Text = "Що таке мікросервіс?",
                            Type = "single_choice",
                            Options = ["Автономний сервіс", "Монолітний модуль", "Таблиця БД"],
                            CorrectAnswer = "Автономний сервіс",
                            Explanation = "Мікросервісна архітектура передбачає автономні компоненти."
                        }
                    ]
                });

            services.AddSingleton(aiGenMock.Object);

            var embeddingDescriptors = services.Where(d =>
                d.ServiceType == typeof(IAiEmbeddingService)).ToList();
            foreach (var descriptor in embeddingDescriptors)
            {
                services.Remove(descriptor);
            }

            var embeddingMock = new Mock<IAiEmbeddingService>();
            embeddingMock
                .Setup(e => e.CreateEmbeddingAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(new AiEmbeddingResult("test-model", new float[384]));

            services.AddSingleton(embeddingMock.Object);

            var validatorDescriptors = services.Where(d =>
                d.ServiceType == typeof(JwtAuthenticationValidator)).ToList();
            foreach (var descriptor in validatorDescriptors)
            {
                services.Remove(descriptor);
            }

            var validatorMock = new Mock<JwtAuthenticationValidator>();
            validatorMock
                .Setup(v => v.ValidateIdTokenAsync(It.Is<string>(t => t == "valid_e2e_microsoft_token")))
                .ReturnsAsync(new ClaimsPrincipal(new ClaimsIdentity([
                    new Claim("preferred_username", "e2e_student@polytech.pp.ua"),
                    new Claim("name", "E2E Student")
                ], "Bearer")));

            validatorMock
                .Setup(v => v.ValidateIdTokenAsync(It.Is<string>(t => t != "valid_e2e_microsoft_token")))
                .ThrowsAsync(new SecurityTokenException("Invalid token."));

            services.AddSingleton(validatorMock.Object);
        });
    }
}

public sealed record AuthTokenResponse(string Token);

[TestFixture]
public class GenerationWorkflowE2ETests
{
    private E2EWebApplicationFactory _factory = null!;
    private HttpClient _client = null!;
    private Guid _lectureId;
    private Guid _chatSessionId;

    [OneTimeSetUp]
    public async Task OneTimeSetUp()
    {
        _factory = new E2EWebApplicationFactory();
        _client = _factory.CreateClient();

        using var scope = _factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();

        var major = new Major { Name = "Software Engineering" };
        var subject = new Subject { Name = "System Architecture", Major = major };
        var lecture = new Lecture { Name = "Microservices Design", Subject = subject };
        var lectureChunk = new LectureChunk
        {
            Lecture = lecture,
            Order = 1,
            Content = "Microservices architecture patterns and design."
        };

        var user = new User
        {
            Name = "E2E Student",
            Email = "e2e_student@polytech.pp.ua",
            Password = "password_hash",
            Roles = ["User"]
        };

        var chatSession = new ChatSession
        {
            Lecture = lecture,
            User = user
        };

        db.Majors.Add(major);
        db.Subjects.Add(subject);
        db.Lectures.Add(lecture);
        db.LectureChunks.Add(lectureChunk);
        db.Users.Add(user);
        db.ChatSessions.Add(chatSession);
        await db.SaveChangesAsync();

        _lectureId = lecture.Id;
        _chatSessionId = chatSession.Id;
    }

    [OneTimeTearDown]
    public void OneTimeTearDown()
    {
        _client.Dispose();
        _factory.Dispose();
    }

    [Test]
    public async Task AutomatedQuizGenerationFromLectureScenario_ShouldGenerateAndPersistTestToDatabase()
    {
        var authRequest = new JwtAuthenticationRequest("valid_e2e_microsoft_token");
        var authResponse = await _client.PostAsJsonAsync("/auth", authRequest);
        var tokenResult = await authResponse.Content.ReadFromJsonAsync<AuthTokenResponse>();
        _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", tokenResult!.Token);

        var requestPayload = new GenerateLearningContentChatRequest(
            ExplicitGeneration: true,
            Type: "test",
            ChatId: _chatSessionId.ToString(),
            Message: "Створи перевірочний тест за матеріалами лекції",
            Count: 1,
            LectureId: _lectureId,
            Context: null);

        var generateResponse = await _client.PostAsJsonAsync("/api/learning-content/generate/chat", requestPayload);

        Assert.That(generateResponse.StatusCode, Is.EqualTo(HttpStatusCode.OK));

        var chatResponse = await generateResponse.Content.ReadFromJsonAsync<GenerateLearningContentChatResponse>();
        Assert.That(chatResponse, Is.Not.Null);
        Assert.That(chatResponse!.Status, Is.EqualTo("completed"));
        Assert.That(chatResponse.Type, Is.EqualTo("test"));
        Assert.That(chatResponse.SystemMessage.Status, Is.EqualTo("completed"));
        Assert.That(chatResponse.CreatedContent, Is.Not.Null);

        var testsResponse = await _client.GetAsync("/tests");
        Assert.That(testsResponse.StatusCode, Is.EqualTo(HttpStatusCode.OK));

        var testsList = await testsResponse.Content.ReadFromJsonAsync<List<TestResponse>>();
        Assert.That(testsList, Is.Not.Null);
        Assert.That(testsList!.Any(t => t.Name == "Тест по лекції з архітектури"), Is.True);

        _client.DefaultRequestHeaders.Authorization = null;
    }

    [Test]
    public async Task AuthenticationToContentGenerationSecurityScenario_ShouldVerifyProtectedFlow()
    {
        var unauthenticatedResponse = await _client.GetAsync("/majors");
        Assert.That(unauthenticatedResponse.StatusCode, Is.EqualTo(HttpStatusCode.Unauthorized));

        var authRequest = new JwtAuthenticationRequest("valid_e2e_microsoft_token");
        var authResponse = await _client.PostAsJsonAsync("/auth", authRequest);
        Assert.That(authResponse.StatusCode, Is.EqualTo(HttpStatusCode.OK));

        var tokenResult = await authResponse.Content.ReadFromJsonAsync<AuthTokenResponse>();
        Assert.That(tokenResult, Is.Not.Null);
        Assert.That(tokenResult!.Token, Is.Not.Null.And.Not.Empty);

        using var authorizedRequest = new HttpRequestMessage(HttpMethod.Get, "/majors");
        authorizedRequest.Headers.Authorization = new AuthenticationHeaderValue("Bearer", tokenResult.Token);

        var authorizedResponse = await _client.SendAsync(authorizedRequest);
        Assert.That(authorizedResponse.StatusCode, Is.EqualTo(HttpStatusCode.OK));
    }
}
