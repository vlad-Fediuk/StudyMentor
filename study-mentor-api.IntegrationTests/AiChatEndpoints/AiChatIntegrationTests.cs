using System.Net;
using System.Net.Http.Json;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using StudyMentorApi.AiChat;
using StudyMentorApi.ChatSessions;
using StudyMentorApi.Data;
using StudyMentorApi.Data.Models;

namespace StudyMentorApi.IntegrationTests.AiChatEndpoints;

public class CustomWebApplicationFactory : WebApplicationFactory<Program>
{
    private readonly string _databaseName = Guid.NewGuid().ToString();

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.UseEnvironment("Development");

        builder.ConfigureServices(services =>
        {
            var descriptors = services.Where(d =>
                d.ServiceType == typeof(DbContextOptions<AppDbContext>) ||
                d.ServiceType == typeof(DbContextOptions)).ToList();

            foreach (var descriptor in descriptors)
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
        });
    }
}

[TestFixture]
public class AiChatIntegrationTests
{
    private CustomWebApplicationFactory _factory = null!;
    private HttpClient _client = null!;
    private Guid _testUserId;
    private Guid _testLectureId;

    [OneTimeSetUp]
    public async Task OneTimeSetUp()
    {
        _factory = new CustomWebApplicationFactory();
        _client = _factory.CreateClient();

        using var scope = _factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();

        var major = new Major { Name = "CS" };
        var subject = new Subject { Name = "Testing", Major = major };
        var lecture = new Lecture { Name = "Integration Testing", Subject = subject };
        var user = new User
        {
            Name = "Integration Student",
            Email = "student@test.com",
            Password = "hash",
            Roles = ["Student"]
        };

        db.Majors.Add(major);
        db.Subjects.Add(subject);
        db.Lectures.Add(lecture);
        db.Users.Add(user);
        await db.SaveChangesAsync();

        _testUserId = user.Id;
        _testLectureId = lecture.Id;
    }

    [OneTimeTearDown]
    public void OneTimeTearDown()
    {
        _client.Dispose();
        _factory.Dispose();
    }

    [Test]
    public async Task PostChatSession_ShouldReturn201Created_WhenLectureIdIsValid()
    {
        var request = new ChatSessionRequest(_testUserId, _testLectureId);

        var response = await _client.PostAsJsonAsync("/chat-sessions", request);

        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.Created));

        var createdSession = await response.Content.ReadFromJsonAsync<ChatSessionResponse>();
        Assert.That(createdSession, Is.Not.Null);
        Assert.That(createdSession!.UserId, Is.EqualTo(_testUserId));
        Assert.That(createdSession.LectureId, Is.EqualTo(_testLectureId));
    }

    [Test]
    public async Task GetChatSessionMessages_ShouldReturn200OkWithOrderedMessages()
    {
        var sessionId = Guid.NewGuid();

        using (var scope = _factory.Services.CreateScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
            var session = new ChatSession
            {
                Id = sessionId,
                UserId = _testUserId,
                LectureId = _testLectureId
            };
            var message1 = new ChatMessage
            {
                ChatSessionId = sessionId,
                Content = "First message",
                Role = MessageRole.User,
                Timestamp = DateTime.UtcNow.AddMinutes(-5),
                SequenceNumber = 1,
                Status = "completed"
            };
            var message2 = new ChatMessage
            {
                ChatSessionId = sessionId,
                Content = "Second message",
                Role = MessageRole.Assistant,
                Timestamp = DateTime.UtcNow.AddMinutes(-2),
                SequenceNumber = 2,
                Status = "completed"
            };

            db.ChatSessions.Add(session);
            db.ChatMessages.AddRange(message1, message2);
            await db.SaveChangesAsync();
        }

        var response = await _client.GetAsync($"/api/ai-chat/messages?chatId={sessionId}");

        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.OK));

        var messages = await response.Content.ReadFromJsonAsync<List<AiChatMessageDto>>();
        Assert.That(messages, Is.Not.Null);
        Assert.That(messages!, Has.Count.EqualTo(2));
        Assert.That(messages[0].Content, Is.EqualTo("First message"));
        Assert.That(messages[1].Content, Is.EqualTo("Second message"));
        Assert.That(messages[0].CreatedAt, Is.LessThan(messages[1].CreatedAt));
    }

    [Test]
    public async Task GetMessages_ShouldReturn400BadRequest_WhenChatIdQueryParamIsMissing()
    {
        var response = await _client.GetAsync("/api/ai-chat/messages");

        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.BadRequest));

        var body = await response.Content.ReadAsStringAsync();
        Assert.That(body, Does.Contain("chatId is required."));
    }
}
