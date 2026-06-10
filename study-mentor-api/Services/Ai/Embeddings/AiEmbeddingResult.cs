namespace StudyMentorApi.Services.Ai.Embeddings;

public sealed record AiEmbeddingResult(
    string Model,
    float[] Vector)
{
    public int Dimensions => Vector.Length;
}
