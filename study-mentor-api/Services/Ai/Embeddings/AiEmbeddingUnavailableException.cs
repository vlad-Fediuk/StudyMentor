namespace StudyMentorApi.Services.Ai.Embeddings;

public sealed class AiEmbeddingUnavailableException(string message, Exception? innerException = null)
    : Exception(message, innerException);
