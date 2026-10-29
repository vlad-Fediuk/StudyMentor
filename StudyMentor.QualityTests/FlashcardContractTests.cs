using System.Net;
using System.Text.Json;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.TestHost;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using StudyMentorApi.ChatMessages;
using StudyMentorApi.Data;
using StudyMentorApi.Data.Models;
using StudyMentorApi.Extensions;
using StudyMentorApi.Flashcards;

namespace StudyMentor.QualityTests;

public class FlashcardContractTests
{
    private static async Task<WebApplication> CreateApp()
    {
        var builder = WebApplication.CreateBuilder();
        builder.WebHost.UseTestServer();
        var databaseName = Guid.NewGuid().ToString();
        builder.Services.AddScoped<AppDbContext>(_ => new ContractDbContext(
            new DbContextOptionsBuilder<AppDbContext>().UseInMemoryDatabase(databaseName).Options));
        builder.Services.AddScoped<ChatMessageService>();
        builder.Services.AddScoped<FlashcardService>();
        var app = builder.Build();
        app.UseGlobalExceptionHandler();
        app.MapFlashcardEndpoints();
        await app.StartAsync();
        return app;
    }

    // These tests exercise Flashcards over HTTP; vector retrieval requires PostgreSQL separately.
    private sealed class ContractDbContext(DbContextOptions<AppDbContext> options) : AppDbContext(options)
    {
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);
            modelBuilder.Entity<LectureChunk>().Ignore(chunk => chunk.Embedding);
        }
    }

    [Fact]
    public async Task LegacyListHasOriginalFields()
    {
        await using var app = await CreateApp();
        using var scope = app.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        db.Flashcards.Add(new Flashcard
        {
            Name = "Sample", ChatMessageId = Guid.NewGuid(),
            Cards = [new Card { Term = "Term", Definition = "Definition" }]
        });
        await db.SaveChangesAsync();
        var response = await app.GetTestClient().GetAsync("/flashcards");
        response.EnsureSuccessStatusCode();
        using var json = JsonDocument.Parse(await response.Content.ReadAsStringAsync());
        var item = Assert.Single(json.RootElement.EnumerateArray());
        Assert.Equal(new[] { "cards", "chatMessageId", "id", "name" }, item.EnumerateObject().Select(p => p.Name).Order());
        Assert.Single(item.GetProperty("cards").EnumerateArray());
    }

    [Theory]
    [InlineData("GET")]
    [InlineData("DELETE")]
    public async Task MissingLegacyResourcePreservesErrorContract(string method)
    {
        await using var app = await CreateApp();
        var response = await app.GetTestClient().SendAsync(new HttpRequestMessage(new HttpMethod(method), $"/flashcards/{Guid.NewGuid()}"));
        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
        using var json = JsonDocument.Parse(await response.Content.ReadAsStringAsync());
        Assert.Equal(new[] { "error" }, json.RootElement.EnumerateObject().Select(p => p.Name));
    }
}

