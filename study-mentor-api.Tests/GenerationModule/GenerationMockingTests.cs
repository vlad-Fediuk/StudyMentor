using System.Security.Claims;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Options;
using Moq;
using StudyMentorApi.Authentication.Jwt;
using StudyMentorApi.Data;
using StudyMentorApi.Data.Models;
using StudyMentorApi.LearningContent;
using StudyMentorApi.LectureChunks;
using StudyMentorApi.Services.Ai;
using StudyMentorApi.Services.Ai.Embeddings;
using StudyMentorApi.Services.Ai.StructuredOutput;
using StudyMentorApi.Users;

namespace study_mentor_api.Tests.GenerationModule;

[TestFixture]
public class GenerationMockingTests
{
    [Test]
    public async Task LearningContentGenerationService_ShouldCallAiGenerationService_WhenGeneratingTests()
    {
        var aiGenerationMock = new Mock<IAiGenerationService>();
        aiGenerationMock
            .Setup(x => x.GenerateStructuredAsync<GeneratedTestDto>(
                It.Is<AiGenerationRequest>(r => r.TaskType == AiTaskType.TestGeneration),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(new GeneratedTestDto
            {
                Title = "Generated Test",
                Questions = []
            });

        var dbOptions = new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;
        await using var dbContext = new AppDbContext(dbOptions);

        var service = new LearningContentGenerationService(
            aiGenerationMock.Object,
            null!,
            null!,
            null!,
            null!,
            null!,
            dbContext);

        var request = new GenerateLearningContentRequest("test", "Create test questions", 5, null, null);
        var response = await service.GenerateAsync(request, CancellationToken.None);

        Assert.That(response, Is.Not.Null);
        Assert.That(response.Type, Is.EqualTo("test"));
        aiGenerationMock.Verify(
            s => s.GenerateStructuredAsync<GeneratedTestDto>(
                It.Is<AiGenerationRequest>(r => r.TaskType == AiTaskType.TestGeneration),
                It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Test]
    public async Task LectureChunkRetrievalService_ShouldCallEmbeddingService_WhenUserMessageProvided()
    {
        var embeddingMock = new Mock<IAiEmbeddingService>();
        var dummyVector = new float[384];
        embeddingMock
            .Setup(e => e.CreateEmbeddingAsync("питання студента", It.IsAny<CancellationToken>()))
            .ReturnsAsync(new AiEmbeddingResult("test-model", dummyVector));

        var dbOptions = new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;
        await using var dbContext = new AppDbContext(dbOptions);

        var options = Options.Create(new AiEmbeddingOptions
        {
            Retrieval = new RagRetrievalOptions
            {
                TopK = 3,
                MaxContextChars = 1000,
                MinSimilarity = 0.5,
                Strict = false
            }
        });

        var retrievalService = new LectureChunkRetrievalService(
            dbContext,
            embeddingMock.Object,
            options,
            NullLogger<LectureChunkRetrievalService>.Instance);

        var lectureId = Guid.NewGuid();
        var context = await retrievalService.GetRelevantContextAsync(lectureId, "питання студента");

        Assert.That(context, Is.Not.Null);
        embeddingMock.Verify(
            e => e.CreateEmbeddingAsync("питання студента", It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Test]
    public async Task JwtAuthenticationService_ShouldCallTokenService_WhenUserIsValid()
    {
        var dbOptions = new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;
        await using var dbContext = new AppDbContext(dbOptions);

        var user = new User
        {
            Id = Guid.NewGuid(),
            Email = "student@polytech.pp.ua",
            Name = "Student Name",
            Password = "test_password",
            Roles = ["User"]
        };
        dbContext.Users.Add(user);
        await dbContext.SaveChangesAsync();

        var userService = new UserService(dbContext);

        var validatorMock = new Mock<JwtAuthenticationValidator>();
        var principal = new ClaimsPrincipal(new ClaimsIdentity([
            new Claim("preferred_username", "student@polytech.pp.ua"),
            new Claim("name", "Student Name")
        ], "Bearer"));

        validatorMock
            .Setup(v => v.ValidateIdTokenAsync("valid_jwt_id_token"))
            .ReturnsAsync(principal);

        var tokenServiceMock = new Mock<IJwtTokenService>();
        tokenServiceMock
            .Setup(t => t.CreateTokenAsync(It.IsAny<User>()))
            .ReturnsAsync("jwt_test_token");

        var authService = new JwtAuthenticationService(
            userService,
            validatorMock.Object,
            tokenServiceMock.Object);

        var resultToken = await authService.AuthenticateAsync(new JwtAuthenticationRequest("valid_jwt_id_token"));

        Assert.That(resultToken, Is.EqualTo("jwt_test_token"));
        tokenServiceMock.Verify(t => t.CreateTokenAsync(It.Is<User>(u => u.Email == "student@polytech.pp.ua")), Times.Once);
    }
}
