using System.Security.Claims;
using Microsoft.EntityFrameworkCore;
using Moq;
using StudyMentorApi.Authentication.Jwt;
using StudyMentorApi.Data;
using StudyMentorApi.LearningContent;
using StudyMentorApi.LectureChunks;
using StudyMentorApi.Services.Ai;
using StudyMentorApi.Services.Ai.StructuredOutput;

namespace study_mentor_api.Tests.GenerationModule;

[TestFixture]
public class GenerationUnitTests
{
    [TestCase("cards")]
    [TestCase("flashcards")]
    [TestCase("CARD")]
    public async Task LearningContentGenerationService_ShouldNormalizeType_WhenFlashcardsAliasesPassed(string alias)
    {
        var aiGenerationMock = new Mock<IAiGenerationService>();
        aiGenerationMock
            .Setup(x => x.GenerateStructuredAsync<GeneratedFlashcardsDto>(
                It.IsAny<AiGenerationRequest>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(new GeneratedFlashcardsDto
            {
                Cards = [new GeneratedFlashcardDto { Front = "Term", Back = "Definition" }]
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

        var request = new GenerateLearningContentRequest(alias, "Generate study cards", 3, null, null);
        var response = await service.GenerateAsync(request, CancellationToken.None);

        Assert.That(response, Is.Not.Null);
        Assert.That(response.Type, Is.EqualTo("flashcard"));
        Assert.That(response.GeneratedContent, Is.InstanceOf<GeneratedFlashcardsDto>());
    }

    [Test]
    public void LectureChunkRetrievalService_ShouldFormatContextWithSimilarityHeader_WhenChunksFound()
    {
        var chunks = new List<LectureChunkRetrievalService.LectureChunkContextItem>
        {
            new(1, "First chunk text about algorithms", 0.15),
            new(2, "Second chunk text about data structures", 0.25)
        };

        var formatted = LectureChunkRetrievalService.FormatContext(chunks, 2000);

        Assert.That(formatted, Does.Contain("Relevant lecture context:"));
        Assert.That(formatted, Does.Contain("[Lecture chunk 1; similarity 0.85]"));
        Assert.That(formatted, Does.Contain("[Lecture chunk 2; similarity 0.75]"));
        Assert.That(formatted, Does.Contain("First chunk text about algorithms"));
        Assert.That(formatted, Does.Contain("Second chunk text about data structures"));
    }

    [Test]
    public void LectureChunkRetrievalService_ShouldTruncateContext_WhenExceedingMaxChars()
    {
        const int maxChars = 500;
        var hugeText = new string('X', 1200);
        var chunks = new List<LectureChunkRetrievalService.LectureChunkContextItem>
        {
            new(1, hugeText, 0.1)
        };

        var formatted = LectureChunkRetrievalService.FormatContext(chunks, maxChars);

        Assert.That(formatted.Length, Is.LessThanOrEqualTo(maxChars));
        Assert.That(formatted, Does.Contain("[Lecture chunk 1; similarity"));
        Assert.That(formatted, Does.Contain("XXXXX"));
    }

    [Test]
    public void JwtAuthenticationValidator_ShouldExtractEmailClaim_WhenPreferredUsernameOrEmailPresent()
    {
        var principalPreferred = new ClaimsPrincipal(new ClaimsIdentity([
            new Claim("preferred_username", "  STUDENT@POLYTECH.PP.UA  ")
        ], "TestAuth"));

        var emailPreferred = JwtAuthenticationValidator.ExtractEmail(principalPreferred);

        Assert.That(emailPreferred, Is.EqualTo("student@polytech.pp.ua"));

        var principalEmail = new ClaimsPrincipal(new ClaimsIdentity([
            new Claim("email", "  USER@EXAMPLE.COM  ")
        ], "TestAuth"));

        var emailDirect = JwtAuthenticationValidator.ExtractEmail(principalEmail);

        Assert.That(emailDirect, Is.EqualTo("user@example.com"));
    }
}
