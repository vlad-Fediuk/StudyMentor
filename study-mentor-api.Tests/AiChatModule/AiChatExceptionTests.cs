using StudyMentorApi.Services.Ai.StructuredOutput;

namespace StudyMentorApi.Tests.AiChatModule;

[TestFixture]
public class AiChatExceptionTests
{
    private readonly AiStructuredOutputParser _parser = new();

    [Test]
    public void AiStructuredOutputParser_ShouldThrowParseException_WhenContentIsEmpty()
    {
        var ex = Assert.Throws<AiStructuredOutputParseException>(() =>
            _parser.ParseAndValidate<GeneratedTestDto>("   "));

        Assert.That(ex.Message, Is.EqualTo("Structured AI response is empty."));
    }

    [Test]
    public void AiStructuredOutputParser_ShouldThrowParseException_WhenJsonIsMalformed()
    {
        var brokenJson = "{ title: missing quotes, questions: [";

        var ex = Assert.Throws<AiStructuredOutputParseException>(() =>
            _parser.ParseAndValidate<GeneratedTestDto>(brokenJson));

        Assert.That(ex.Message, Is.EqualTo("Structured AI response is not valid JSON."));
    }

    [Test]
    public void AiStructuredOutputParser_ShouldThrowParseException_WhenQuestionOptionsAreMissingForChoiceType()
    {
        var jsonWithoutOptions = """
        {
          "title": "Invalid Test",
          "questions": [
            {
              "text": "Question without choices?",
              "type": "single_choice",
              "options": [],
              "correctAnswer": "Answer"
            }
          ]
        }
        """;

        var ex = Assert.Throws<AiStructuredOutputParseException>(() =>
            _parser.ParseAndValidate<GeneratedTestDto>(jsonWithoutOptions));

        Assert.That(ex.Message, Does.Contain("options are required"));
    }
}
