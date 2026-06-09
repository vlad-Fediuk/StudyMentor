using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Pgvector;
using Pgvector.EntityFrameworkCore;
using StudyMentorApi.Data;
using StudyMentorApi.Services.Ai.Embeddings;

namespace StudyMentorApi.LectureChunks;

public sealed class LectureChunkRetrievalService(
    AppDbContext dbContext,
    IAiEmbeddingService embeddingService,
    IOptions<AiEmbeddingOptions> options,
    ILogger<LectureChunkRetrievalService> logger)
{
    public async Task<string> GetRelevantContextAsync(
        Guid lectureId,
        string userMessage,
        CancellationToken cancellationToken = default)
    {
        var retrievalOptions = options.Value.Retrieval;
        var topK = Math.Max(1, retrievalOptions.TopK);
        var maxContextChars = Math.Max(500, retrievalOptions.MaxContextChars);

        try
        {
            var embedding = await embeddingService.CreateEmbeddingAsync(userMessage, cancellationToken);
            var queryVector = new Vector(embedding.Vector);

            var chunks = await dbContext.LectureChunks
                .AsNoTracking()
                .Where(chunk =>
                    chunk.LectureId == lectureId
                    && chunk.Embedding != null
                    && chunk.EmbeddingModel == embedding.Model
                    && chunk.EmbeddingDimensions == embedding.Dimensions)
                .OrderBy(chunk => chunk.Embedding!.CosineDistance(queryVector))
                .Take(topK)
                .Select(chunk => new LectureChunkContextItem(chunk.Order, chunk.Content))
                .ToListAsync(cancellationToken);

            return FormatContext(chunks, maxContextChars);
        }
        catch (AiEmbeddingUnavailableException ex)
        {
            if (retrievalOptions.Strict)
            {
                throw;
            }

            logger.LogWarning(ex, "RAG context was skipped because embeddings are unavailable.");
            return string.Empty;
        }
    }

    private static string FormatContext(
        IReadOnlyCollection<LectureChunkContextItem> chunks,
        int maxContextChars)
    {
        if (chunks.Count == 0)
        {
            return string.Empty;
        }

        var parts = new List<string>();
        var usedChars = 0;

        foreach (var chunk in chunks)
        {
            var prefix = $"[Lecture chunk {chunk.Order}]\n";
            var remaining = maxContextChars - usedChars - prefix.Length;
            if (remaining <= 0)
            {
                break;
            }

            var content = chunk.Content.Length > remaining
                ? chunk.Content[..remaining]
                : chunk.Content;

            parts.Add(prefix + content);
            usedChars += prefix.Length + content.Length;
        }

        return parts.Count == 0
            ? string.Empty
            : $"Relevant lecture context:\n\n{string.Join("\n\n", parts)}";
    }

    private sealed record LectureChunkContextItem(int Order, string Content);
}
