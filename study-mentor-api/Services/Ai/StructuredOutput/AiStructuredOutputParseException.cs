namespace StudyMentorApi.Services.Ai.StructuredOutput;

public sealed class AiStructuredOutputParseException(string message, Exception? innerException = null)
    : Exception(message, innerException);
