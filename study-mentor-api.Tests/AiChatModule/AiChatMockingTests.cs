using Microsoft.AspNetCore.Hosting;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Moq;
using StudyMentorApi.AiChat;
using StudyMentorApi.ChatMessages;
using StudyMentorApi.ChatSessions;
using StudyMentorApi.Data;
using StudyMentorApi.Data.Models;
using StudyMentorApi.LectureChunks;
using StudyMentorApi.Services.Ai;
using StudyMentorApi.Services.Ai.Embeddings;
using StudyMentorApi.Services.Ai.Prompts;
using StudyMentorApi.Users;

namespace StudyMentorApi.Tests.AiChatModule;

[TestFixture]
public class AiChatMockingTests
{
    private string _tempDirectory = null!;
    private Mock<IWebHostEnvironment> _environmentMock = null!;

    [OneTimeSetUp]
    public void OneTimeSetUp()
    {
        _tempDirectory = Path.Combine(Path.GetTempPath(), "StudyMentorMockTests_" + Guid.NewGuid().ToString("N"));
        var promptsDir = Path.Combine(_tempDirectory, "Services", "Ai", "Prompts");
        Directory.CreateDirectory(promptsDir);
        File.WriteAllText(Path.Combine(promptsDir, "SystemPrompt.md"), "Default system prompt for testing.");

        _environmentMock = new Mock<IWebHostEnvironment>();
        _environmentMock.Setup(e => e.ContentRootPath).Returns(_tempDirectory);
    }

    [OneTimeTearDown]
    public void OneTimeTearDown()
    {
        if (Directory.Exists(_tempDirectory))
        {
            try
            {
                Directory.Delete(_tempDirectory, true);
            }
            catch
            {
            }
        }
    }

    [Test]
    public async Task PromptComposer_ShouldCallPromptTemplateProviderWithCorrectTaskType_WhenComposing()
    {
        var providerMock = new Mock<IPromptTemplateProvider>();
        providerMock
            .Setup(p => p.GetActiveTemplateAsync(AiTaskType.ChatAnswer, "chat-answer", It.IsAny<string?>(), It.IsAny<string?>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync("Custom prompt template");

        var composer = new PromptComposer(_environmentMock.Object, providerMock.Object);

        var request = new PromptCompositionRequest
        {
            TaskType = AiTaskType.ChatAnswer,
            UserMessage = "Explain polymorphism in C#"
        };

        var result = await composer.ComposeAsync(request);

        Assert.That(result, Is.Not.Null);
        providerMock.Verify(
            p => p.GetActiveTemplateAsync(AiTaskType.ChatAnswer, "chat-answer", It.IsAny<string?>(), It.IsAny<string?>(), It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Test]
    public async Task AiModelRouter_ShouldRouteToPrimaryProviderClient_WhenProviderIsEnabled()
    {
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;
        using var dbContext = new AppDbContext(options);

        var provider = new AiProvider
        {
            Id = Guid.NewGuid(),
            Name = "LmStudio",
            Type = "lmstudio",
            BaseUrl = "http://localhost:1234/v1",
            IsEnabled = true,
            Priority = 1
        };
        var model = new AiModel
        {
            Id = Guid.NewGuid(),
            ProviderId = provider.Id,
            ModelName = "gemma-2b",
            DisplayName = "Gemma 2B",
            IsEnabled = true,
            Priority = 1
        };
        provider.Models.Add(model);
        dbContext.AiProviders.Add(provider);
        await dbContext.SaveChangesAsync();

        var lmStudioMock = new Mock<IAiProviderClient>();
        lmStudioMock.SetupGet(c => c.ProviderType).Returns("lmstudio");
        lmStudioMock
            .Setup(c => c.CompleteAsync(It.IsAny<AiProviderRequest>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new AiProviderResponse("LmStudio", "gemma-2b", "AI Answer content"));

        var loggerMock = new Mock<ILogger<AiModelRouter>>();
        var router = new AiModelRouter(dbContext, [lmStudioMock.Object], loggerMock.Object);

        var request = new AiChatRequest
        {
            Messages = [new AiChatMessage("user", "Hello tutor")]
        };

        var response = await router.CompleteAsync(request);

        Assert.That(response.Content, Is.EqualTo("AI Answer content"));
        lmStudioMock.Verify(
            c => c.CompleteAsync(It.IsAny<AiProviderRequest>(), It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Test]
    public async Task AiChatService_ShouldSaveAssistantMessage_WhenAiResponseIsSuccessful()
    {
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;
        using var dbContext = new AppDbContext(options);

        var chatId = Guid.NewGuid();
        var userId = Guid.NewGuid();
        var lectureId = Guid.NewGuid();

        var chatSession = new ChatSession
        {
            Id = chatId,
            UserId = userId,
            LectureId = lectureId,
            Lecture = new Lecture
            {
                Id = lectureId,
                Name = "Intro to OOP",
                Subject = new Subject { Id = Guid.NewGuid(), Name = "OOP", MajorId = Guid.NewGuid() }
            }
        };

        var user = new User
        {
            Id = userId,
            Name = "Student",
            Password = "hashedpassword",
            Roles = ["Student"],
            GroupId = Guid.NewGuid()
        };

        var chatSessionServiceMock = new Mock<ChatSessionService>(dbContext);
        chatSessionServiceMock
            .Setup(s => s.GetByIdAsync(chatId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(chatSession);

        var chatMessageServiceMock = new Mock<ChatMessageService>(dbContext);
        chatMessageServiceMock
            .Setup(s => s.GetNextSequenceNumberAsync(chatId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(1);
        chatMessageServiceMock
            .Setup(s => s.GetMessagesAsync(chatId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<ChatMessage>());
        chatMessageServiceMock
            .Setup(s => s.CreateAsync(It.IsAny<ChatMessage>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((ChatMessage m, CancellationToken _) => m);

        var userServiceMock = new Mock<UserService>(dbContext);
        userServiceMock
            .Setup(u => u.GetByIdAsync(userId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(user);

        var lectureChunkRetrievalServiceMock = new Mock<LectureChunkRetrievalService>(
            dbContext,
            Mock.Of<IAiEmbeddingService>(),
            Options.Create(new AiEmbeddingOptions()),
            Mock.Of<ILogger<LectureChunkRetrievalService>>());
        lectureChunkRetrievalServiceMock
            .Setup(r => r.GetRelevantContextAsync(lectureId, It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync("Relevant OOP context");

        var aiGenerationServiceMock = new Mock<IAiGenerationService>();
        aiGenerationServiceMock
            .Setup(g => g.GenerateAsync(It.IsAny<AiGenerationRequest>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new AiGenerationResponse("Assistant helpful explanation", "lmstudio", "gemma-2b", false, AiTaskType.ChatAnswer));

        var aiChatService = new AiChatService(
            chatSessionServiceMock.Object,
            chatMessageServiceMock.Object,
            userServiceMock.Object,
            lectureChunkRetrievalServiceMock.Object,
            aiGenerationServiceMock.Object);

        var request = new AiChatSendMessageRequest(chatId.ToString(), "Can you explain polymorphism?");

        var result = await aiChatService.SendMessageAsync(request, CancellationToken.None);

        Assert.That(result.Status, Is.EqualTo("completed"));
        chatMessageServiceMock.Verify(
            s => s.CreateAsync(
                It.Is<ChatMessage>(m => m.Role == MessageRole.Assistant && m.Content == "Assistant helpful explanation"),
                It.IsAny<CancellationToken>()),
            Times.Once);
    }
}
