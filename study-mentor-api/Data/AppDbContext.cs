using Microsoft.EntityFrameworkCore;
using StudyMentorApi.Data.Models;

namespace StudyMentorApi.Data;

public class AppDbContext(DbContextOptions<AppDbContext> options) : DbContext(options)
{
    public DbSet<Major> Majors => Set<Major>();

    public DbSet<Subject> Subjects => Set<Subject>();

    public DbSet<Lecture> Lectures => Set<Lecture>();

    public DbSet<ChatMessage> ChatMessages => Set<ChatMessage>();

    public DbSet<ChatSession> ChatSessions => Set<ChatSession>();

    public DbSet<Exercise> Exercises => Set<Exercise>();

    public DbSet<Flashcard> Flashcards => Set<Flashcard>();

    public DbSet<Card> Cards => Set<Card>();

    public DbSet<Test> Tests => Set<Test>();

    public DbSet<TestQuestion> TestQuestions => Set<TestQuestion>();

    public DbSet<TestAnswerVariant> TestAnswerVariants => Set<TestAnswerVariant>();

    public DbSet<Group> Groups => Set<Group>();

    public DbSet<User> Users => Set<User>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.HasPostgresExtension("pgcrypto");

        ConfigureBaseEntity<Major>(modelBuilder);
        ConfigureBaseEntity<Subject>(modelBuilder);
        ConfigureBaseEntity<Lecture>(modelBuilder);
        ConfigureBaseEntity<ChatMessage>(modelBuilder);
        ConfigureBaseEntity<ChatSession>(modelBuilder);
        ConfigureBaseEntity<Exercise>(modelBuilder);
        ConfigureBaseEntity<Card>(modelBuilder);
        ConfigureBaseEntity<TestQuestion>(modelBuilder);
        ConfigureBaseEntity<TestAnswerVariant>(modelBuilder);
        ConfigureBaseEntity<Group>(modelBuilder);
        ConfigureBaseEntity<User>(modelBuilder);

        modelBuilder.Entity<Major>(entity =>
        {
            entity.ToTable("majors");
            entity.Property(e => e.Name).IsRequired();
            entity.HasMany(e => e.Subjects)
                .WithOne(e => e.Major)
                .HasForeignKey(e => e.MajorId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<Subject>(entity =>
        {
            entity.ToTable("subjects");
            entity.Property(e => e.Name).IsRequired();
            entity.HasMany(e => e.Lectures)
                .WithOne(e => e.Subject)
                .HasForeignKey(e => e.SubjectId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<Lecture>(entity =>
        {
            entity.ToTable("lectures");
            entity.Property(e => e.Name).IsRequired();
            entity.HasMany(e => e.ChatSessions)
                .WithOne(e => e.Lecture)
                .HasForeignKey(e => e.LectureId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<ChatSession>(entity =>
        {
            entity.ToTable("chat_sessions");
            entity.HasMany(e => e.Messages)
                .WithOne(e => e.ChatSession)
                .HasForeignKey(e => e.ChatSessionId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<ChatMessage>(entity =>
        {
            entity.ToTable("chat_messages");
            entity.Property(e => e.Content).IsRequired();
            entity.Property(e => e.Status)
                .IsRequired()
                .HasDefaultValue("completed");
            entity.HasMany(e => e.Exercises)
                .WithOne(e => e.ChatMessage)
                .HasForeignKey(e => e.ChatMessageId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<Exercise>(entity =>
        {
            entity.ToTable("exercises");
            entity.Property(e => e.Name).IsRequired();
            entity.HasDiscriminator<string>("ExerciseType")
                .HasValue<Exercise>("Exercise")
                .HasValue<Flashcard>("Flashcard")
                .HasValue<Test>("Test");
        });

        modelBuilder.Entity<Flashcard>(entity =>
        {
            entity.HasMany(e => e.Cards)
                .WithOne(e => e.Flashcard)
                .HasForeignKey(e => e.FlashcardId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<Test>(entity =>
        {
            entity.HasOne(e => e.SourceFlashcard)
                .WithMany()
                .HasForeignKey(e => e.SourceFlashcardId)
                .OnDelete(DeleteBehavior.SetNull);
            entity.HasMany(e => e.Questions)
                .WithOne(e => e.Test)
                .HasForeignKey(e => e.TestId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<TestQuestion>(entity =>
        {
            entity.ToTable("test_questions");
            entity.Property(e => e.Prompt).IsRequired();
            entity.HasMany(e => e.AnswerVariants)
                .WithOne(e => e.TestQuestion)
                .HasForeignKey(e => e.TestQuestionId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<TestAnswerVariant>(entity =>
        {
            entity.ToTable("test_answer_variants");
            entity.Property(e => e.Text).IsRequired();
        });

        modelBuilder.Entity<Card>(entity =>
        {
            entity.ToTable("cards");
            entity.Property(e => e.Term).IsRequired();
            entity.Property(e => e.Definition).IsRequired();
        });

        modelBuilder.Entity<Group>(entity =>
        {
            entity.ToTable("groups");
            entity.Property(e => e.Name).IsRequired();
        });

        modelBuilder.Entity<User>(entity =>
        {
            entity.ToTable("users");
            entity.Property(e => e.Name).IsRequired();
            entity.Property(e => e.Password).IsRequired();
            entity.HasMany(e => e.ChatSessions)
                .WithOne(e => e.User)
                .HasForeignKey(e => e.UserId)
                .OnDelete(DeleteBehavior.Cascade);
        });
    }

    private static void ConfigureBaseEntity<TEntity>(ModelBuilder modelBuilder)
        where TEntity : BaseEntity
    {
        modelBuilder.Entity<TEntity>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Id)
                .HasDefaultValueSql("gen_random_uuid()")
                .ValueGeneratedOnAdd();
        });
    }
}
