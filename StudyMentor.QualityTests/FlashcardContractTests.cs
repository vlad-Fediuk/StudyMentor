using System.Net;
using System.Net.Http.Json;
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
    private static readonly string[] LegacyFields = ["cards", "chatMessageId", "id", "name"];
    private static readonly string[] ErrorFields = ["error"];
    private static async Task<WebApplication> CreateApp()
    {
        var builder = WebApplication.CreateBuilder();
        builder.WebHost.UseTestServer();
        var databaseName = Guid.NewGuid().ToString();
        builder.Services.AddScoped<AppDbContext>(_ => new ContractDbContext(
            new DbContextOptionsBuilder<AppDbContext>().UseInMemoryDatabase(databaseName).Options));
        builder.Services.AddScoped<ChatMessageService>();
        builder.Services.AddScoped<FlashcardService>();
        builder.Services.AddApplicationApiVersioning();
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
    public async Task V1CrudRoundTripKeepsLegacyAliasAndTrimsInput()
    {
        await using var app = await CreateApp();
        Guid messageId;
        using (var scope = app.Services.CreateScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
            var message = new ChatMessage { Content = "Topic", ChatSessionId = Guid.NewGuid() };
            db.ChatMessages.Add(message);
            await db.SaveChangesAsync();
            messageId = message.Id;
        }
        var client = app.GetTestClient();
        var created = await client.PostAsJsonAsync("/api/v1/flashcards",
            new FlashcardRequest(" Set ", messageId, [new(" Term ", " Definition ")]));
        Assert.Equal(HttpStatusCode.Created, created.StatusCode);
        using var initial = JsonDocument.Parse(await created.Content.ReadAsStringAsync());
        var id = initial.RootElement.GetProperty("id").GetString();
        Assert.Equal($"/flashcards/{id}", created.Headers.Location!.ToString());
        Assert.Equal("Set", initial.RootElement.GetProperty("name").GetString());
        Assert.Equal("Term", initial.RootElement.GetProperty("cards")[0].GetProperty("term").GetString());
        var updated = await client.PutAsJsonAsync($"/api/v1/flashcards/{id}",
            new FlashcardRequest(" Updated ", messageId, [new("New", "Meaning")]));
        updated.EnsureSuccessStatusCode();
        using var fetched = JsonDocument.Parse(await client.GetStringAsync($"/flashcards/{id}"));
        Assert.Equal("Updated", fetched.RootElement.GetProperty("name").GetString());
        Assert.Equal("New", fetched.RootElement.GetProperty("cards")[0].GetProperty("term").GetString());
        Assert.Equal(HttpStatusCode.NoContent, (await client.DeleteAsync($"/api/v1/flashcards/{id}")).StatusCode);
        Assert.Equal(HttpStatusCode.NotFound, (await client.GetAsync($"/api/v2/flashcards/{id}")).StatusCode);
    }

    [Fact]
    public async Task V2EmptyPageUsesDocumentedDefaults()
    {
        await using var app = await CreateApp();
        using var json = JsonDocument.Parse(await app.GetTestClient().GetStringAsync("/api/v2/flashcards"));
        Assert.Equal(1, json.RootElement.GetProperty("page").GetInt32());
        Assert.Equal(20, json.RootElement.GetProperty("pageSize").GetInt32());
        Assert.Equal(0, json.RootElement.GetProperty("total").GetInt32());
        Assert.Empty(json.RootElement.GetProperty("items").EnumerateArray());
    }

    [Theory]
    [InlineData("/flashcards")]
    [InlineData("/api/v1/flashcards")]
    public async Task LegacyListHasOriginalFields(string route)
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
        var response = await app.GetTestClient().GetAsync(route);
        response.EnsureSuccessStatusCode();
        using var json = JsonDocument.Parse(await response.Content.ReadAsStringAsync());
        var item = Assert.Single(json.RootElement.EnumerateArray());
        Assert.Equal(LegacyFields, item.EnumerateObject().Select(p => p.Name).Order());
        Assert.Single(item.GetProperty("cards").EnumerateArray());
    }

    [Fact]
    public async Task V2PaginatesAndAddsCardCount()
    {
        await using var app = await CreateApp();
        using var scope = app.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        for (var i = 0; i < 3; i++)
        {
            db.Flashcards.Add(new Flashcard { Name = $"Set {i}", Cards = [new Card { Term = "A", Definition = "B" }] });
        }
        await db.SaveChangesAsync();
        var client = app.GetTestClient();
        var response = await client.GetAsync("/api/v2/flashcards?page=2&pageSize=2");
        response.EnsureSuccessStatusCode();
        using var json = JsonDocument.Parse(await response.Content.ReadAsStringAsync());
        var root = json.RootElement;
        Assert.Equal(3, root.GetProperty("total").GetInt32());
        Assert.Equal(2, root.GetProperty("page").GetInt32());
        Assert.Equal(2, root.GetProperty("pageSize").GetInt32());
        var item = Assert.Single(root.GetProperty("items").EnumerateArray());
        Assert.Equal(1, item.GetProperty("cardsCount").GetInt32());
        var detail = await client.GetAsync($"/api/v2/flashcards/{item.GetProperty("id").GetString()}");
        detail.EnsureSuccessStatusCode();
        using var detailJson = JsonDocument.Parse(await detail.Content.ReadAsStringAsync());
        Assert.Equal(item.GetProperty("id").GetString(), detailJson.RootElement.GetProperty("id").GetString());
        Assert.Equal(1, detailJson.RootElement.GetProperty("cardsCount").GetInt32());
        var firstPage = await client.GetStringAsync("/api/v2/flashcards?page=1&pageSize=2");
        using var firstJson = JsonDocument.Parse(firstPage);
        Assert.DoesNotContain(firstJson.RootElement.GetProperty("items").EnumerateArray(),
            first => first.GetProperty("id").GetString() == item.GetProperty("id").GetString());
        Assert.True(response.Headers.Contains("api-supported-versions"));
    }

    [Theory]
    [InlineData("page=0")]
    [InlineData("pageSize=101")]
    [InlineData("pageSize=-1")]
    [InlineData("page=2147483647&pageSize=100")]
    public async Task V2RejectsInvalidPagination(string query)
    {
        await using var app = await CreateApp();
        var response = await app.GetTestClient().GetAsync($"/api/v2/flashcards?{query}");
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task UnsupportedUrlVersionIsNotFound()
    {
        await using var app = await CreateApp();
        var response = await app.GetTestClient().GetAsync("/api/v3/flashcards");
        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Theory]
    [InlineData("/flashcards")]
    [InlineData("/api/v1/flashcards")]
    public async Task InvalidCreateKeepsLegacyErrorShape(string route)
    {
        await using var app = await CreateApp();
        var response = await app.GetTestClient().PostAsJsonAsync(route,
            new FlashcardRequest(" ", Guid.NewGuid(), []));
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        using var json = JsonDocument.Parse(await response.Content.ReadAsStringAsync());
        Assert.Equal(ErrorFields, json.RootElement.EnumerateObject().Select(p => p.Name));
        Assert.Equal("Flashcard name is required.", json.RootElement.GetProperty("error").GetString());
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
        Assert.Equal(ErrorFields, json.RootElement.EnumerateObject().Select(p => p.Name));
    }
}

