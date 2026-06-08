namespace StudyMentorApi.Services.Ai.StructuredOutput;

public interface IAiStructuredOutputParser
{
    TOutput ParseAndValidate<TOutput>(string content);
}
