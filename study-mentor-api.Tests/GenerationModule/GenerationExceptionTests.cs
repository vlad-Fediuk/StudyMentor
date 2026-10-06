using System.Security.Claims;
using Microsoft.EntityFrameworkCore;
using Moq;
using StudyMentorApi.Authentication;
using StudyMentorApi.Authentication.Jwt;
using StudyMentorApi.Common;
using StudyMentorApi.Data;
using StudyMentorApi.LearningContent;
using StudyMentorApi.Services.Ai;

namespace study_mentor_api.Tests.GenerationModule;

[TestFixture]
public class GenerationExceptionTests
{
    private LearningContentGenerationService _service = null!;
    private AppDbContext _dbContext = null!;

    [SetUp]
    public void SetUp()
    {
        var dbOptions = new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;
        _dbContext = new AppDbContext(dbOptions);

        var aiGenMock = new Mock<IAiGenerationService>();
        _service = new LearningContentGenerationService(
            aiGenMock.Object,
            null!,
            null!,
            null!,
            null!,
            null!,
            _dbContext);
    }

    [TearDown]
    public void TearDown()
    {
        _dbContext.Dispose();
    }

    [Test]
    public void LearningContentGenerationService_ShouldThrowValidationException_WhenTypeIsUnsupported()
    {
        var request = new GenerateLearningContentRequest("invalid", "msg", 5, null, null);

        var ex = Assert.ThrowsAsync<ValidationException>(() =>
            _service.GenerateAsync(request, CancellationToken.None));

        Assert.That(ex, Is.Not.Null);
        Assert.That(ex!.Message, Is.EqualTo("type must be either 'test' or 'flashcard'."));
    }

    [TestCase(0)]
    [TestCase(-2)]
    public void LearningContentGenerationService_ShouldThrowValidationException_WhenCountIsZeroOrNegative(int invalidCount)
    {
        var request = new GenerateLearningContentRequest("test", "msg", invalidCount, null, null);

        var ex = Assert.ThrowsAsync<ValidationException>(() =>
            _service.GenerateAsync(request, CancellationToken.None));

        Assert.That(ex, Is.Not.Null);
        Assert.That(ex!.Message, Is.EqualTo("count must be greater than zero."));
    }

    [Test]
    public void JwtAuthenticationService_ShouldThrowAuthenticationException_WhenEmailClaimMissing()
    {
        var validatorMock = new Mock<JwtAuthenticationValidator>();
        var principalWithoutEmail = new ClaimsPrincipal(new ClaimsIdentity([
            new Claim("sub", "12345"),
            new Claim("name", "Test User")
        ], "Bearer"));

        validatorMock
            .Setup(v => v.ValidateIdTokenAsync("dummy_token"))
            .ReturnsAsync(principalWithoutEmail);

        var tokenServiceMock = new Mock<IJwtTokenService>();
        var authService = new JwtAuthenticationService(null!, validatorMock.Object, tokenServiceMock.Object);

        var request = new JwtAuthenticationRequest("dummy_token");

        var ex = Assert.ThrowsAsync<AuthenticationException>(() =>
            authService.AuthenticateAsync(request));

        Assert.That(ex, Is.Not.Null);
        Assert.That(ex!.Message, Is.EqualTo("Email claim is missing in the Microsoft token."));
    }
}
