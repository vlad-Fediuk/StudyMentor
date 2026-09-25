using Pgvector;

namespace StudyMentorApi.Data.Models;

public class LectureChunk : BaseEntity
{
    public required string Content { get; set; }

    public int Order { get; set; }

    public Vector? Embedding { get; set; }

    public string? EmbeddingModel { get; set; }

    public int? EmbeddingDimensions { get; set; }

    public Guid LectureId { get; set; }

    public Lecture Lecture { get; set; } = null!;
}
