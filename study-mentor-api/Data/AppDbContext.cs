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

    public DbSet<Group> Groups => Set<Group>();

    public DbSet<User> Users => Set<User>();

    public DbSet<AiProvider> AiProviders => Set<AiProvider>();

    public DbSet<AiModel> AiModels => Set<AiModel>();

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
        ConfigureBaseEntity<Group>(modelBuilder);
        ConfigureBaseEntity<User>(modelBuilder);
        ConfigureBaseEntity<AiProvider>(modelBuilder);
        ConfigureBaseEntity<AiModel>(modelBuilder);

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

        modelBuilder.Entity<AiProvider>(entity =>
        {
            entity.ToTable("ai_providers");
            entity.Property(e => e.Name).IsRequired();
            entity.Property(e => e.Type).IsRequired();
            entity.Property(e => e.BaseUrl).IsRequired();
            entity.Property(e => e.IsEnabled).HasDefaultValue(true);
            entity.Property(e => e.TimeoutSeconds).HasDefaultValue(300);
            entity.Property(e => e.SettingsJson).HasColumnType("jsonb");
            entity.HasIndex(e => e.Type).IsUnique();
            entity.HasMany(e => e.Models)
                .WithOne(e => e.Provider)
                .HasForeignKey(e => e.ProviderId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<AiModel>(entity =>
        {
            entity.ToTable("ai_models");
            entity.Property(e => e.ModelName).IsRequired();
            entity.Property(e => e.DisplayName).IsRequired();
            entity.Property(e => e.IsEnabled).HasDefaultValue(true);
            entity.Property(e => e.TopP).HasDefaultValue(1.0);
            entity.Property(e => e.MaxOutputTokens).HasDefaultValue(2048);
            entity.Property(e => e.CapabilitiesJson).HasColumnType("jsonb");
            entity.Property(e => e.SettingsJson).HasColumnType("jsonb");
            entity.HasIndex(e => new { e.ProviderId, e.ModelName }).IsUnique();
        });

        var lmStudioProviderId = Guid.Parse("58f2f8b9-0d72-4c49-9dd0-6da81f4d4a01");
        var nvidiaProviderId = Guid.Parse("cbe16bfc-6b2d-4d2f-a9e0-f0b3786c2102");

        modelBuilder.Entity<AiProvider>().HasData(
            new
            {
                Id = lmStudioProviderId,
                Name = "LmStudio",
                Type = "lmstudio",
                BaseUrl = "http://localhost:1234/api/v1/chat",
                ApiKeyEnvironmentVariable = (string?)null,
                IsEnabled = true,
                Priority = 1,
                TimeoutSeconds = 300,
                SettingsJson = (string?)null
            },
            new
            {
                Id = nvidiaProviderId,
                Name = "Nvidia",
                Type = "nvidia",
                BaseUrl = "https://integrate.api.nvidia.com/v1/chat/completions",
                ApiKeyEnvironmentVariable = "NVIDIA_API_KEY",
                IsEnabled = true,
                Priority = 2,
                TimeoutSeconds = 300,
                SettingsJson = (string?)null
            });

        modelBuilder.Entity<AiModel>().HasData(
            new
            {
                Id = Guid.Parse("70de4e65-5337-4ee4-9224-7eceadf3ed2d"),
                ProviderId = lmStudioProviderId,
                ModelName = "gemma-4-e2b-it",
                DisplayName = "gemma-4-e2b-it",
                IsEnabled = true,
                Priority = 1,
                Temperature = 0.15,
                TopP = 1.0,
                MaxOutputTokens = 2048,
                ReasoningBudget = (int?)null,
                EnableThinking = false,
                CapabilitiesJson = (string?)null,
                SettingsJson = (string?)null
            },
            new
            {
                Id = Guid.Parse("902ab4f1-c498-4f62-979a-99860b8548fe"),
                ProviderId = nvidiaProviderId,
                ModelName = "mistralai/mistral-large-3-675b-instruct-2512",
                DisplayName = "mistralai/mistral-large-3-675b-instruct-2512",
                IsEnabled = true,
                Priority = 1,
                Temperature = 0.15,
                TopP = 1.0,
                MaxOutputTokens = 2048,
                ReasoningBudget = 2048,
                EnableThinking = false,
                CapabilitiesJson = (string?)null,
                SettingsJson = (string?)null
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
