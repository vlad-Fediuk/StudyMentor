namespace StudyMentorApi.Services.Ai.Embeddings;

public sealed class AiEmbeddingOptions
{
    public const string SectionName = "AiEmbeddings";

    public string Provider { get; set; } = "lmstudio";

    public LmStudioEmbeddingOptions LmStudio { get; set; } = new();

    public RagRetrievalOptions Retrieval { get; set; } = new();
}

public sealed class LmStudioEmbeddingOptions
{
    public string BaseUrl { get; set; } = "http://localhost:1234/v1/embeddings";

    public string Model { get; set; } = string.Empty;

    public int TimeoutSeconds { get; set; } = 120;
}

public sealed class RagRetrievalOptions
{
    public int TopK { get; set; } = 4;

    public int MaxContextChars { get; set; } = 6000;

    public double MinSimilarity { get; set; } = 0.35;

    public bool Strict { get; set; }
}
