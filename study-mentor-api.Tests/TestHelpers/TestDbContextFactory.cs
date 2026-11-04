using Microsoft.EntityFrameworkCore;
using StudyMentorApi.Data;
using StudyMentorApi.Data.Models;

namespace study_mentor_api.Tests.TestHelpers;

public class TestAppDbContext(DbContextOptions<AppDbContext> options) : AppDbContext(options)
{
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
        modelBuilder.Entity<LectureChunk>().Ignore(c => c.Embedding);
    }
}

public static class TestDbContextFactory
{
    public static AppDbContext Create(string? dbName = null)
    {
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase(dbName ?? Guid.NewGuid().ToString())
            .Options;

        return new TestAppDbContext(options);
    }
}
