using System.Net;
using System.Net.Http.Json;
using study_mentor_api.IntegrationTests.Infrastructure;
using StudyMentorApi.Flashcards;

namespace study_mentor_api.IntegrationTests.FlashcardsEndpoints;

[TestFixture]
public class FlashcardIntegrationTests
{
    private CustomWebApplicationFactory _factory = null!;
    private HttpClient _client = null!;

    [SetUp]
    public void SetUp()
    {
        _factory = new CustomWebApplicationFactory();
        _client = _factory.CreateClient();
    }

    [TearDown]
    public void TearDown()
    {
        _client.Dispose();
        _factory.Dispose();
    }

    [Test]
    public async Task PostFlashcards_ShouldReturn201Created_WhenPayloadIsValid()
    {
        var request = new FlashcardRequest(
            "Software Architecture",
            _factory.SeededChatMessageId,
            [
                new CardRequest("Microservices", "Independently deployable services"),
                new CardRequest("Monolith", "Single unified codebase")
            ]
        );

        var response = await _client.PostAsJsonAsync("/flashcards", request);

        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.Created));

        var created = await response.Content.ReadFromJsonAsync<FlashcardResponse>();
        Assert.That(created, Is.Not.Null);
        Assert.That(created!.Id, Is.Not.EqualTo(Guid.Empty));
        Assert.That(created.Name, Is.EqualTo("Software Architecture"));
        Assert.That(created.Cards, Has.Count.EqualTo(2));
    }

    [Test]
    public async Task GetFlashcardById_ShouldReturn200OkWithCards_WhenExists()
    {
        var createRequest = new FlashcardRequest(
            "Operating Systems",
            _factory.SeededChatMessageId,
            [
                new CardRequest("Thread", "Lightweight process"),
                new CardRequest("Deadlock", "Permanent blocked state")
            ]
        );

        var createResponse = await _client.PostAsJsonAsync("/flashcards", createRequest);
        var created = await createResponse.Content.ReadFromJsonAsync<FlashcardResponse>();
        Assert.That(created, Is.Not.Null);

        var getResponse = await _client.GetAsync($"/flashcards/{created!.Id}");

        Assert.That(getResponse.StatusCode, Is.EqualTo(HttpStatusCode.OK));

        var fetched = await getResponse.Content.ReadFromJsonAsync<FlashcardResponse>();
        Assert.That(fetched, Is.Not.Null);
        Assert.That(fetched!.Id, Is.EqualTo(created.Id));
        Assert.That(fetched.Cards, Has.Count.EqualTo(2));
        Assert.That(fetched.Cards.Any(c => c.Term == "Thread" && c.Definition == "Lightweight process"), Is.True);
    }

    [Test]
    public async Task DeleteFlashcard_ShouldReturn204NoContentAndSubsequentGetShouldReturn404()
    {
        var createRequest = new FlashcardRequest(
            "Temporary Card Deck",
            _factory.SeededChatMessageId,
            [
                new CardRequest("Discard", "To get rid of")
            ]
        );

        var createResponse = await _client.PostAsJsonAsync("/flashcards", createRequest);
        var created = await createResponse.Content.ReadFromJsonAsync<FlashcardResponse>();
        Assert.That(created, Is.Not.Null);

        var deleteResponse = await _client.DeleteAsync($"/flashcards/{created!.Id}");
        Assert.That(deleteResponse.StatusCode, Is.EqualTo(HttpStatusCode.NoContent));

        var subsequentGetResponse = await _client.GetAsync($"/flashcards/{created.Id}");
        Assert.That(subsequentGetResponse.StatusCode, Is.EqualTo(HttpStatusCode.NotFound));
    }
}
