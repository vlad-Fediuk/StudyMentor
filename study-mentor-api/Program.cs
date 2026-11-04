using Microsoft.EntityFrameworkCore;
using Pgvector.EntityFrameworkCore;
using StudyMentorApi.AiChat;
using StudyMentorApi.Authentication;
using StudyMentorApi.Data;
using StudyMentorApi.ChatMessages;
using StudyMentorApi.ChatSessions;
using StudyMentorApi.Extensions;
using StudyMentorApi.Flashcards;
using StudyMentorApi.Groups;
using StudyMentorApi.LearningContent;
using StudyMentorApi.LectureChunks;
using StudyMentorApi.Lectures;
using StudyMentorApi.Majors;
using StudyMentorApi.Subjects;
using StudyMentorApi.Tests;
using StudyMentorApi.Users;

namespace StudyMentorApi;

public class Program
{
    protected Program()
    {
    }

    public static async Task Main(string[] args)
    {
        DotNetEnv.Env.TraversePath().Load();

        var builder = WebApplication.CreateBuilder(args);

        var rawConnectionString = builder.Configuration.GetConnectionString("DefaultConnection")
            ?? builder.Configuration["DefaultConnection"]
            ?? builder.Configuration["ConnectionStrings__DefaultConnection"];

        var connectionString = NormalizeConnectionString(rawConnectionString);

        if (string.IsNullOrWhiteSpace(connectionString))
        {
            if (builder.Environment.IsProduction())
            {
                throw new InvalidOperationException(
                    "Connection string 'DefaultConnection' is not configured. " +
                    "Please configure ConnectionStrings__DefaultConnection via .env or environment variables.");
            }

            connectionString = "Host=localhost;Database=test;Username=postgres;Password=postgres;";
        }

        if (builder.Environment.IsEnvironment("Testing"))
        {
            builder.Services.AddDbContext<AppDbContext>(options =>
                options.UseInMemoryDatabase(builder.Configuration["Testing:DatabaseName"] ?? "TestingDb"));
        }
        else
        {
            builder.Services.AddDbContext<AppDbContext>(options =>
                options.UseNpgsql(
                    connectionString,
                    npgsqlOptions => npgsqlOptions.UseVector()));
        }

        builder.Services.AddApplicationServices(builder.Configuration, builder.Environment);
        builder.Services.AddApplicationApiVersioning();

        builder.Services.AddCors(options =>
        {
            options.AddDefaultPolicy(policy =>
            {
                policy
                    .SetIsOriginAllowed(origin =>
                    {
                        if (string.IsNullOrEmpty(origin))
                        {
                            return false;
                        }

                        if (Uri.TryCreate(origin, UriKind.Absolute, out var uri))
                        {
                            var host = uri.Host;
                            return host.Equals("localhost", StringComparison.OrdinalIgnoreCase)
                                || host.EndsWith(".pages.dev", StringComparison.OrdinalIgnoreCase)
                                || host.EndsWith(".onrender.com", StringComparison.OrdinalIgnoreCase);
                        }

                        return false;
                    })
                    .AllowAnyHeader()
                    .AllowAnyMethod();
            });
        });

        builder.Services.AddRateLimiting(builder.Configuration);

        var app = builder.Build();

        if (app.Environment.IsDevelopment())
        {
            app.MapOpenApi().WithDocumentPerVersion();
            app.UseSwagger();
            app.UseSwaggerUI(c =>
            {
                c.SwaggerEndpoint("/swagger/v1/swagger.json", "Study Mentor API V1");
                c.SwaggerEndpoint("/swagger/v2/swagger.json", "Study Mentor API V2");
                c.RoutePrefix = string.Empty;
            });
        }

        app.UseGlobalExceptionHandler();
        app.UseCors();
        app.UseHttpsRedirection();
        app.UseAuthentication();
        app.UseRateLimiting();
        app.UseAuthorization();
        app.MapAuthenticationEndpoints();
        app.MapMajorEndpoints();
        app.MapSubjectEndpoints();
        app.MapLectureEndpoints();
        app.MapLectureChunkEndpoints();
        app.MapChatMessageEndpoints();
        app.MapAiChatEndpoints();
        app.MapGroupEndpoints();
        app.MapUserEndpoints();
        app.MapChatSessionEndpoints();
        app.MapFlashcardEndpoints();
        app.MapLearningContentEndpoints();
        app.MapTestEndpoints();

        if (!app.Environment.IsEnvironment("Testing"))
        {
            using var scope = app.Services.CreateScope();
            var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
            if (db.Database.IsNpgsql())
            {
                try
                {
                    await db.Database.MigrateAsync();
                }
                catch
                {
                }
            }
        }

        await app.RunAsync();
    }

    private static string NormalizeConnectionString(string? raw)
    {
        if (string.IsNullOrWhiteSpace(raw))
        {
            return string.Empty;
        }

        var trimmed = raw.Trim().Trim('"', '\'');
        if (trimmed.StartsWith("ConnectionStrings__DefaultConnection=", StringComparison.OrdinalIgnoreCase))
        {
            trimmed = trimmed["ConnectionStrings__DefaultConnection=".Length..].Trim().Trim('"', '\'');
        }

        if (trimmed.StartsWith("DefaultConnection=", StringComparison.OrdinalIgnoreCase))
        {
            trimmed = trimmed["DefaultConnection=".Length..].Trim().Trim('"', '\'');
        }

        if (trimmed.StartsWith("postgres://", StringComparison.OrdinalIgnoreCase) ||
            trimmed.StartsWith("postgresql://", StringComparison.OrdinalIgnoreCase))
        {
            try
            {
                var uri = new Uri(trimmed);
                var userInfo = uri.UserInfo.Split(':', 2);
                var username = userInfo.Length > 0 ? Uri.UnescapeDataString(userInfo[0]) : string.Empty;
                var password = userInfo.Length > 1 ? Uri.UnescapeDataString(userInfo[1]) : string.Empty;
                var port = uri.Port > 0 ? uri.Port : 5432;
                var database = uri.AbsolutePath.TrimStart('/');
                if (string.IsNullOrEmpty(database))
                {
                    database = "postgres";
                }

                return $"Host={uri.Host};Port={port};Database={database};Username={username};Password={password};";
            }
            catch
            {
                return trimmed;
            }
        }

        return trimmed;
    }
}
