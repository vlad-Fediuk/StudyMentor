using Microsoft.AspNetCore.Hosting;
using Moq;
using StudyMentorApi.Services.Ai;
using StudyMentorApi.Services.Ai.Prompts;
using StudyMentorApi.Services.Ai.StructuredOutput;

namespace StudyMentorApi.Tests.AiChatModule;

[TestFixture]
public class AiChatUnitTests
{
    private string _tempDirectory = null!;
    private Mock<IWebHostEnvironment> _environmentMock = null!;

    [OneTimeSetUp]
    public void OneTimeSetUp()
    {
        _tempDirectory = Path.Combine(Path.GetTempPath(), "StudyMentorTests_" + Guid.NewGuid().ToString("N"));
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
    public async Task PromptComposer_ShouldSubstituteResponseSchemaVariable_WhenTemplateContainsPlaceholder()
    {
        var templateProviderMock = new Mock<IPromptTemplateProvider>();
        templateProviderMock
            .Setup(p => p.GetActiveTemplateAsync(AiTaskType.TestGeneration, "test-generation", It.IsAny<string?>(), It.IsAny<string?>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((string?)null);

        var request = new PromptCompositionRequest
        {
            TaskType = AiTaskType.TestGeneration,
            UserMessage = "Generate a sample test",
            ResponseSchema = "{ \"test\": true }"
        };

        var composer = new PromptComposer(_environmentMock.Object, templateProviderMock.Object);

        var result = await composer.ComposeAsync(request);

        Assert.That(result.Content, Does.Contain("{ \"test\": true }"));
        Assert.That(result.Content, Does.Not.Contain("{{response_schema}}"));
    }

    [Test]
    public async Task PromptComposer_ShouldFallbackToDefaultTemplate_WhenProviderReturnsNull()
    {
        var templateProviderMock = new Mock<IPromptTemplateProvider>();
        templateProviderMock
            .Setup(p => p.GetActiveTemplateAsync(AiTaskType.TestGeneration, "test-generation", It.IsAny<string?>(), It.IsAny<string?>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((string?)null);

        var request = new PromptCompositionRequest
        {
            TaskType = AiTaskType.TestGeneration,
            UserMessage = "Generate quiz questions"
        };

        var composer = new PromptComposer(_environmentMock.Object, templateProviderMock.Object);

        var result = await composer.ComposeAsync(request);

        Assert.That(result.Content, Does.Contain("Generate a study test from the user's request and available lecture context."));
    }

    [Test]
    public void AiStructuredOutputParser_ShouldStripMarkdownFences_WhenResponseEnclosedInCodeBlocks()
    {
        var parser = new AiStructuredOutputParser();
        var content = "```json\n{\n  \"cards\": [{\"front\": \"A\", \"back\": \"B\"}]\n}\n```";

        var result = parser.ParseAndValidate<GeneratedFlashcardsDto>(content);

        Assert.That(result, Is.Not.Null);
        Assert.That(result.Cards, Is.Not.Null);
        Assert.That(result.Cards!, Has.Count.EqualTo(1));
        Assert.That(result.Cards!.First().Front, Is.EqualTo("A"));
        Assert.That(result.Cards!.First().Back, Is.EqualTo("B"));
    }

    [Test]
    public void AiStructuredOutputParser_ShouldParseValidTestDto_WhenJsonIsWellFormed()
    {
        var parser = new AiStructuredOutputParser();
        var json = """
        {
          "title": "C# Basics",
          "questions": [
            {
              "text": "What is the entry point in C#?",
              "type": "single_choice",
              "options": ["Main", "Start", "Run"],
              "correctAnswer": "Main",
              "explanation": "Main is the standard entry point."
            }
          ]
        }
        """;

        var result = parser.ParseAndValidate<GeneratedTestDto>(json);

        Assert.That(result, Is.Not.Null);
        Assert.That(result.Title, Is.EqualTo("C# Basics"));
        Assert.That(result.Questions, Is.Not.Null);
        Assert.That(result.Questions!, Has.Count.EqualTo(1));

        var question = result.Questions!.First();
        Assert.That(question.Text, Is.EqualTo("What is the entry point in C#?"));
        Assert.That(question.Type, Is.EqualTo("single_choice"));
        Assert.That(question.CorrectAnswer, Is.EqualTo("Main"));
    }
}
