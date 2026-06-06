using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace StudyMentorApi.Services.Ai;

public sealed class LmStudioProviderClient(HttpClient httpClient) : IAiProviderClient
{
    public string ProviderType => "lmstudio";

    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.SnakeCaseLower,
        DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull
    };

    public async Task<AiProviderResponse> CompleteAsync(
        AiProviderRequest request,
        CancellationToken cancellationToken = default)
    {
        var payload = new LmStudioChatCompletionRequest(
            request.Model,
            request.Messages.Select(message => new LmStudioChatMessage(message.Role, message.Content)).ToArray(),
            request.MaxOutputTokens,
            request.Temperature,
            request.TopP,
            false);

        using var requestContent = new StringContent(
            JsonSerializer.Serialize(payload, JsonOptions),
            Encoding.UTF8,
            "application/json");
        requestContent.Headers.ContentType = new MediaTypeHeaderValue("application/json");

        using var httpRequest = new HttpRequestMessage(HttpMethod.Post, request.BaseUrl)
        {
            Content = requestContent
        };
        httpRequest.Headers.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

        using var timeout = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
        timeout.CancelAfter(TimeSpan.FromSeconds(Math.Max(1, request.TimeoutSeconds)));

        using var response = await httpClient.SendAsync(httpRequest, timeout.Token);
        var responseBody = await response.Content.ReadAsStringAsync(timeout.Token);

        if (!response.IsSuccessStatusCode)
        {
            throw new HttpRequestException(
                $"LM Studio AI request failed with status {(int)response.StatusCode}: {responseBody}");
        }

        var completion = JsonSerializer.Deserialize<LmStudioChatCompletionResponse>(
            responseBody,
            JsonOptions);

        var content = completion?.Choices?.FirstOrDefault()?.Message.Content;
        if (string.IsNullOrWhiteSpace(content))
        {
            throw new InvalidOperationException("LM Studio AI response did not contain a completion message.");
        }

        return new AiProviderResponse(request.ProviderType, completion?.Model ?? request.Model, content);
    }

    private sealed record LmStudioChatCompletionRequest(
        string Model,
        IReadOnlyCollection<LmStudioChatMessage> Messages,
        int MaxTokens,
        double Temperature,
        double TopP,
        bool Stream);

    private sealed record LmStudioChatMessage(
        string Role,
        string Content);

    private sealed record LmStudioChatCompletionResponse(
        string? Model,
        IReadOnlyCollection<LmStudioChoice>? Choices);

    private sealed record LmStudioChoice(
        LmStudioResponseMessage Message);

    private sealed record LmStudioResponseMessage(
        string? Content);
}
