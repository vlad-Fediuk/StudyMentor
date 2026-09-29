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
using StudyMentorApi.LectureChunks;
using StudyMentorApi.Services.Ai;

namespace StudyMentorApi.IntegrationTests.E2E;

public class StubAiGenerationService : IAiGenerationService
{
    public Task<AiGenerationResponse> GenerateAsync(AiGenerationRequest request, CancellationToken cancellationToken = default)
    {
        return Task.FromResult(new AiGenerationResponse(
            $"Консультація тьютора: відповідь на запит '{request.UserMessage}'.",
            "MockAiProvider",
            "mock-llm-v1",
            false,
            request.TaskType));
    }

    public Task<TOutput> GenerateStructuredAsync<TOutput>(AiGenerationRequest request, CancellationToken cancellationToken = default)
    {
        throw new NotImplementedException();
    }
}

public class StubLectureChunkRetrievalService : LectureChunkRetrievalService
{
    public StubLectureChunkRetrievalService(
        AppDbContext dbContext,
        Microsoft.Extensions.Options.IOptions<StudyMentorApi.Services.Ai.Embeddings.AiEmbeddingOptions> options,
        Microsoft.Extensions.Logging.ILogger<LectureChunkRetrievalService> logger)
        : base(dbContext, null!, options, logger)
    {
    }

    public override Task<string> GetRelevantContextAsync(
        Guid lectureId,
        string userMessage,
        CancellationToken cancellationToken = default)
    {
        return Task.FromResult("Матеріали лекції: інкапсуляція приховує деталі реалізації, поліморфізм надає єдиний інтерфейс.");
    }
}

public class E2EWebApplicationFactory : WebApplicationFactory<Program>
{
    private readonly string _databaseName = Guid.NewGuid().ToString();

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.UseEnvironment("Development");

        builder.ConfigureServices(services =>
        {
            var descriptors = services.Where(d =>
                d.ServiceType == typeof(DbContextOptions<AppDbContext>) ||
                d.ServiceType == typeof(DbContextOptions) ||
                d.ServiceType == typeof(LectureChunkRetrievalService) ||
                d.ServiceType == typeof(IAiGenerationService)).ToList();

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

            services.AddScoped<LectureChunkRetrievalService, StubLectureChunkRetrievalService>();
            services.AddScoped<IAiGenerationService, StubAiGenerationService>();
        });
    }
}

[TestFixture]
public class AiChatWorkflowE2ETests
{
    private E2EWebApplicationFactory _factory = null!;
    private HttpClient _client = null!;
    private Guid _studentUserId;
    private Guid _lectureId1;
    private Guid _lectureId2;

    [OneTimeSetUp]
    public async Task OneTimeSetUp()
    {
        _factory = new E2EWebApplicationFactory();
        _client = _factory.CreateClient();

        using var scope = _factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();

        var major = new Major { Name = "Computer Science" };
        var subject = new Subject { Name = "Software Engineering", Major = major };
        var lecture1 = new Lecture { Name = "OOP Principles", Subject = subject };
        var lecture2 = new Lecture { Name = "Database Design", Subject = subject };
        var student = new User
        {
            Name = "Student E2E Tester",
            Email = "e2e_student@studymentor.com",
            Password = "hashed_password",
            Roles = ["Student"]
        };

        db.Majors.Add(major);
        db.Subjects.Add(subject);
        db.Lectures.AddRange(lecture1, lecture2);
        db.Users.Add(student);
        await db.SaveChangesAsync();

        _studentUserId = student.Id;
        _lectureId1 = lecture1.Id;
        _lectureId2 = lecture2.Id;
    }

    [TearDown]
    public async Task TearDown()
    {
        using var scope = _factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        db.ChatMessages.RemoveRange(db.ChatMessages);
        db.ChatSessions.RemoveRange(db.ChatSessions);
        await db.SaveChangesAsync();
    }

    [OneTimeTearDown]
    public void OneTimeTearDown()
    {
        _client.Dispose();
        _factory.Dispose();
    }

    [Test]
    public async Task StudentConsultationFullWorkflow_Scenario()
    {
        var sessionResponse = await _client.PostAsJsonAsync("/chat-sessions", new ChatSessionRequest(_studentUserId, _lectureId1));
        Assert.That(sessionResponse.StatusCode, Is.EqualTo(HttpStatusCode.Created));

        var createdSession = await sessionResponse.Content.ReadFromJsonAsync<ChatSessionResponse>();
        Assert.That(createdSession, Is.Not.Null);
        var sessionId = createdSession!.Id;

        var messageRequest1 = new AiChatSendMessageRequest(sessionId.ToString(), "Поясни, будь ласка, принцип інкапсуляції.");
        var sendResponse1 = await _client.PostAsJsonAsync("/api/ai-chat/messages", messageRequest1);
        Assert.That(sendResponse1.StatusCode, Is.EqualTo(HttpStatusCode.OK));

        var aiResponse1 = await sendResponse1.Content.ReadFromJsonAsync<AiChatSendMessageResponse>();
        Assert.That(aiResponse1, Is.Not.Null);
        Assert.That(aiResponse1!.Status, Is.EqualTo("completed"));
        Assert.That(aiResponse1.AssistantMessage.Content, Does.Contain("принцип інкапсуляції"));

        var messageRequest2 = new AiChatSendMessageRequest(sessionId.ToString(), "Наведи приклад коду на C#.");
        var sendResponse2 = await _client.PostAsJsonAsync("/api/ai-chat/messages", messageRequest2);
        Assert.That(sendResponse2.StatusCode, Is.EqualTo(HttpStatusCode.OK));

        var historyResponse = await _client.GetAsync($"/api/ai-chat/messages?chatId={sessionId}");
        Assert.That(historyResponse.StatusCode, Is.EqualTo(HttpStatusCode.OK));

        var history = await historyResponse.Content.ReadFromJsonAsync<List<AiChatMessageDto>>();
        Assert.That(history, Is.Not.Null);
        Assert.That(history!, Has.Count.EqualTo(4));
        Assert.That(history[0].Role, Is.EqualTo("user"));
        Assert.That(history[1].Role, Is.EqualTo("assistant"));
        Assert.That(history[2].Role, Is.EqualTo("user"));
        Assert.That(history[3].Role, Is.EqualTo("assistant"));
        Assert.That(history[0].CreatedAt, Is.LessThanOrEqualTo(history[1].CreatedAt));
    }

    [Test]
    public async Task StudentSessionResumeAndContinuation_Scenario()
    {
        var createResponse = await _client.PostAsJsonAsync("/chat-sessions", new ChatSessionRequest(_studentUserId, _lectureId1));
        var session = await createResponse.Content.ReadFromJsonAsync<ChatSessionResponse>();
        var sessionId = session!.Id;

        await _client.PostAsJsonAsync("/api/ai-chat/messages", new AiChatSendMessageRequest(sessionId.ToString(), "Початкове питання діалогу."));

        var userSessionsResponse = await _client.GetAsync($"/chat-sessions/user/{_studentUserId}");
        Assert.That(userSessionsResponse.StatusCode, Is.EqualTo(HttpStatusCode.OK));

        var sessionsList = await userSessionsResponse.Content.ReadFromJsonAsync<List<ChatSessionResponse>>();
        Assert.That(sessionsList, Is.Not.Null);
        Assert.That(sessionsList!.Any(s => s.Id == sessionId), Is.True);

        var resumeHistoryResponse = await _client.GetAsync($"/api/ai-chat/messages?chatId={sessionId}");
        var initialHistory = await resumeHistoryResponse.Content.ReadFromJsonAsync<List<AiChatMessageDto>>();
        Assert.That(initialHistory, Has.Count.EqualTo(2));

        var continueResponse = await _client.PostAsJsonAsync("/api/ai-chat/messages", new AiChatSendMessageRequest(sessionId.ToString(), "Продовження діалогу після паузи."));
        Assert.That(continueResponse.StatusCode, Is.EqualTo(HttpStatusCode.OK));

        var updatedHistoryResponse = await _client.GetAsync($"/api/ai-chat/messages?chatId={sessionId}");
        var updatedHistory = await updatedHistoryResponse.Content.ReadFromJsonAsync<List<AiChatMessageDto>>();
        Assert.That(updatedHistory, Has.Count.EqualTo(4));
        Assert.That(updatedHistory![2].Content, Is.EqualTo("Продовження діалогу після паузи."));
    }

    [Test]
    public async Task MultiLectureParallelStudySessions_Scenario()
    {
        var session1Res = await _client.PostAsJsonAsync("/chat-sessions", new ChatSessionRequest(_studentUserId, _lectureId1));
        var session2Res = await _client.PostAsJsonAsync("/chat-sessions", new ChatSessionRequest(_studentUserId, _lectureId2));

        var session1 = (await session1Res.Content.ReadFromJsonAsync<ChatSessionResponse>())!;
        var session2 = (await session2Res.Content.ReadFromJsonAsync<ChatSessionResponse>())!;

        await _client.PostAsJsonAsync("/api/ai-chat/messages", new AiChatSendMessageRequest(session1.Id.ToString(), "Питання по ООП принципах."));
        await _client.PostAsJsonAsync("/api/ai-chat/messages", new AiChatSendMessageRequest(session2.Id.ToString(), "Питання по проектуванню реляційних баз даних."));

        var history1Res = await _client.GetAsync($"/api/ai-chat/messages?chatId={session1.Id}");
        var history2Res = await _client.GetAsync($"/api/ai-chat/messages?chatId={session2.Id}");

        var history1 = await history1Res.Content.ReadFromJsonAsync<List<AiChatMessageDto>>();
        var history2 = await history2Res.Content.ReadFromJsonAsync<List<AiChatMessageDto>>();

        Assert.That(history1, Has.Count.EqualTo(2));
        Assert.That(history2, Has.Count.EqualTo(2));
        Assert.That(history1![0].Content, Does.Contain("ООП"));
        Assert.That(history2![0].Content, Does.Contain("реляційних баз"));
    }

    [Test]
    public async Task ClientInputValidationAndRecovery_Scenario()
    {
        var sessionResponse = await _client.PostAsJsonAsync("/chat-sessions", new ChatSessionRequest(_studentUserId, _lectureId1));
        var session = (await sessionResponse.Content.ReadFromJsonAsync<ChatSessionResponse>())!;

        var invalidResponse = await _client.PostAsJsonAsync("/api/ai-chat/messages", new AiChatSendMessageRequest(session.Id.ToString(), "   "));
        Assert.That(invalidResponse.StatusCode, Is.EqualTo(HttpStatusCode.BadRequest));

        var errorBody = await invalidResponse.Content.ReadAsStringAsync();
        Assert.That(errorBody, Does.Contain("content is required."));

        var validResponse = await _client.PostAsJsonAsync("/api/ai-chat/messages", new AiChatSendMessageRequest(session.Id.ToString(), "Виправлене питання: що таке поліморфізм?"));
        Assert.That(validResponse.StatusCode, Is.EqualTo(HttpStatusCode.OK));

        var historyResponse = await _client.GetAsync($"/api/ai-chat/messages?chatId={session.Id}");
        var history = await historyResponse.Content.ReadFromJsonAsync<List<AiChatMessageDto>>();

        Assert.That(history, Has.Count.EqualTo(2));
        Assert.That(history![0].Content, Is.EqualTo("Виправлене питання: що таке поліморфізм?"));
    }

    [Test]
    public async Task SessionLifecycleWithNonexistentQuery_Scenario()
    {
        var sessionResponse = await _client.PostAsJsonAsync("/chat-sessions", new ChatSessionRequest(_studentUserId, _lectureId1));
        var session = (await sessionResponse.Content.ReadFromJsonAsync<ChatSessionResponse>())!;

        await _client.PostAsJsonAsync("/api/ai-chat/messages", new AiChatSendMessageRequest(session.Id.ToString(), "Питання перед закриттям сесії."));

        var initialHistoryRes = await _client.GetAsync($"/api/ai-chat/messages?chatId={session.Id}");
        Assert.That(initialHistoryRes.StatusCode, Is.EqualTo(HttpStatusCode.OK));

        var randomMissingId = Guid.NewGuid();
        var missingSessionRes = await _client.GetAsync($"/chat-sessions/{randomMissingId}");
        Assert.That(missingSessionRes.StatusCode, Is.EqualTo(HttpStatusCode.NotFound));

        var missingMessagesRes = await _client.GetAsync($"/api/ai-chat/messages?chatId={randomMissingId}");
        Assert.That(missingMessagesRes.StatusCode, Is.EqualTo(HttpStatusCode.NotFound));
    }
}
