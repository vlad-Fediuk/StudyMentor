using Microsoft.EntityFrameworkCore;
using StudyMentorApi.Data.Models;

namespace StudyMentorApi.Data;

public class AppDbContext(DbContextOptions<AppDbContext> options) : DbContext(options)
{
    public DbSet<Major> Majors => Set<Major>();

    public DbSet<Subject> Subjects => Set<Subject>();

    public DbSet<Lecture> Lectures => Set<Lecture>();

    public DbSet<ChatMessage> ChatMessages => Set<ChatMessage>();

    public DbSet<Exercise> Exercises => Set<Exercise>();

    public DbSet<User> Users => Set<User>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.HasPostgresExtension("pgcrypto");

        ConfigureBaseEntity<Major>(modelBuilder);
        ConfigureBaseEntity<Subject>(modelBuilder);
        ConfigureBaseEntity<Lecture>(modelBuilder);
        ConfigureBaseEntity<ChatMessage>(modelBuilder);
        ConfigureBaseEntity<Exercise>(modelBuilder);
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
        });

        modelBuilder.Entity<ChatMessage>(entity =>
        {
            entity.ToTable("chat_messages");
            entity.Property(e => e.Content).IsRequired();
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

        modelBuilder.Entity<User>(entity =>
        {
            entity.ToTable("users");
            entity.Property(e => e.Name).IsRequired();
            entity.Property(e => e.Password).IsRequired();
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
