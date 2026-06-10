namespace StudyMentorApi.Services.Ai.Embeddings;

public interface IAiEmbeddingService
{
    Task<AiEmbeddingResult> CreateEmbeddingAsync(
        string input,
        CancellationToken cancellationToken = default);
}
