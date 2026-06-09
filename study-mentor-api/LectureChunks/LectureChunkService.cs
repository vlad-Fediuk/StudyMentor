using Microsoft.EntityFrameworkCore;
using Pgvector;
using StudyMentorApi.Common;
using StudyMentorApi.Data;
using StudyMentorApi.Data.Models;
using StudyMentorApi.Services.Ai.Embeddings;
using System.Text.RegularExpressions;

namespace StudyMentorApi.LectureChunks;

public class LectureChunkService(AppDbContext dbContext, IAiEmbeddingService embeddingService)
{
    private const int TargetChunkLength = 1000;
    private const int MaxChunkLength = 1500;
    private static readonly HashSet<string> AllowedExtensions = new(StringComparer.OrdinalIgnoreCase)
    {
        ".txt",
        ".md"
    };

    public async Task<IReadOnlyCollection<LectureChunkResponse>> CreateFromFileAsync(
        LectureChunkRequest request,
        CancellationToken cancellationToken = default)
    {
        var lecture = await dbContext.Lectures
            .FirstOrDefaultAsync(l => l.Id == request.LectureId, cancellationToken);

        if (lecture is null)
        {
            throw new NotFoundException("Lecture not found");
        }

        if (lecture.SubjectId != request.SubjectId)
        {
            throw new ValidationException("Lecture does not belong to the specified subject");
        }

        ValidateFile(request.File);

        var content = await ReadFileAsync(request.File!, cancellationToken);
        var chunks = SplitIntoChunks(content);

        if (chunks.Count == 0)
        {
            throw new ValidationException("No chunks were produced after processing");
        }

        var entities = new List<LectureChunk>(chunks.Count);
        for (var index = 0; index < chunks.Count; index++)
        {
            var embedding = await embeddingService.CreateEmbeddingAsync(chunks[index], cancellationToken);

            entities.Add(new LectureChunk
            {
                Content = chunks[index],
                LectureId = lecture.Id,
                Order = index + 1,
                Embedding = new Vector(embedding.Vector),
                EmbeddingModel = embedding.Model,
                EmbeddingDimensions = embedding.Dimensions
            });
        }

        await using var transaction = await dbContext.Database.BeginTransactionAsync(cancellationToken);

        var existingChunks = await dbContext.LectureChunks
            .Where(chunk => chunk.LectureId == lecture.Id)
            .ToListAsync(cancellationToken);

        dbContext.LectureChunks.RemoveRange(existingChunks);
        dbContext.LectureChunks.AddRange(entities);
        await dbContext.SaveChangesAsync(cancellationToken);
        await transaction.CommitAsync(cancellationToken);

        return entities
            .OrderBy(e => e.Order)
            .Select(e => new LectureChunkResponse(e.Id, e.Content, e.Order, e.LectureId))
            .ToList();
    }

    private static void ValidateFile(IFormFile? file)
    {
        if (file is null)
        {
            throw new ValidationException("File not provided");
        }

        if (file.Length == 0)
        {
            throw new ValidationException("File is empty");
        }

        var extension = Path.GetExtension(file.FileName);
        if (!AllowedExtensions.Contains(extension))
        {
            throw new ValidationException("File extension is not .txt or .md");
        }
    }

    private static async Task<string> ReadFileAsync(IFormFile file, CancellationToken cancellationToken)
    {
        await using var stream = file.OpenReadStream();
        using var reader = new StreamReader(stream);
        return await reader.ReadToEndAsync(cancellationToken);
    }

    private static List<string> SplitIntoChunks(string content)
    {
        var normalized = content.Replace("\r\n", "\n").Replace("\r", "\n");
        var paragraphs = Regex
            .Split(normalized, @"\n\s*\n")
            .Select(p => p.Trim())
            .Where(p => !string.IsNullOrWhiteSpace(p))
            .ToList();

        var chunks = new List<string>();
        var current = string.Empty;

        foreach (var paragraph in paragraphs)
        {
            if (current.Length == 0)
            {
                current = paragraph;
                continue;
            }

            var candidateLength = current.Length + 2 + paragraph.Length;
            if (current.Length >= TargetChunkLength || candidateLength > MaxChunkLength)
            {
                chunks.Add(current);
                current = paragraph;
                continue;
            }

            current = $"{current}\n\n{paragraph}";
        }

        if (!string.IsNullOrWhiteSpace(current))
        {
            chunks.Add(current);
        }

        return chunks;
    }
}
