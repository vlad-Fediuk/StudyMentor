using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace StudyMentorApi.Services.Ai;

public sealed class NvidiaProviderClient(HttpClient httpClient) : IAiProviderClient
{
    public string ProviderType => "nvidia";

    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.SnakeCaseLower,
        DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull
    };

    public async Task<AiProviderResponse> CompleteAsync(
        AiProviderRequest request,
        CancellationToken cancellationToken = default)
    {
        var apiKey = GetApiKey(request.ApiKeyEnvironmentVariable);
        if (string.IsNullOrWhiteSpace(apiKey))
        {
            throw new InvalidOperationException("NVIDIA API key is not configured. Set NVIDIA_API_KEY.");
        }

        var payload = new NvidiaChatCompletionRequest(
            request.Model,
            request.Messages.Select(message => new NvidiaChatMessage(message.Role, message.Content)).ToArray(),
            request.MaxOutputTokens,
            request.Temperature,
            request.TopP,
            false,
            request.EnableThinking
                ? new NvidiaChatTemplateOptions(request.EnableThinking)
                : null,
            request.EnableThinking ? request.ReasoningBudget : null);

        using var requestContent = new StringContent(
            JsonSerializer.Serialize(payload, JsonOptions),
            Encoding.UTF8,
            "application/json");
        requestContent.Headers.ContentType = new MediaTypeHeaderValue("application/json");

        using var httpRequest = new HttpRequestMessage(HttpMethod.Post, request.BaseUrl)
        {
            Content = requestContent
        };

        httpRequest.Headers.Authorization = new AuthenticationHeaderValue("Bearer", apiKey);
        httpRequest.Headers.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

        using var timeout = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
        timeout.CancelAfter(TimeSpan.FromSeconds(Math.Max(1, request.TimeoutSeconds)));

        using var response = await httpClient.SendAsync(httpRequest, timeout.Token);
        var responseBody = await response.Content.ReadAsStringAsync(timeout.Token);

        if (!response.IsSuccessStatusCode)
        {
            throw new HttpRequestException(
                $"NVIDIA AI request failed with status {(int)response.StatusCode}: {responseBody}");
        }

        var completion = JsonSerializer.Deserialize<NvidiaChatCompletionResponse>(
            responseBody,
            JsonOptions);

        var content = completion?.Choices?.FirstOrDefault()?.Message.Content;
        if (string.IsNullOrWhiteSpace(content))
        {
            throw new InvalidOperationException("NVIDIA AI response did not contain a completion message.");
        }

        return new AiProviderResponse(request.ProviderType, completion?.Model ?? request.Model, content);
    }

    private static string GetApiKey(string? environmentVariable)
    {
        var variableName = string.IsNullOrWhiteSpace(environmentVariable)
            ? "NVIDIA_API_KEY"
            : environmentVariable;

        return Environment.GetEnvironmentVariable(variableName) ?? string.Empty;
    }

    private sealed record NvidiaChatCompletionRequest(
        string Model,
        IReadOnlyCollection<NvidiaChatMessage> Messages,
        int MaxTokens,
        double Temperature,
        double TopP,
        bool Stream,
        [property: JsonPropertyName("chat_template_kwargs")]
        NvidiaChatTemplateOptions? ChatTemplateOptions,
        [property: JsonPropertyName("reasoning_budget")]
        int? ReasoningBudget);

    private sealed record NvidiaChatTemplateOptions(
        bool EnableThinking);

    private sealed record NvidiaChatMessage(
        string Role,
        string Content);

    private sealed record NvidiaChatCompletionResponse(
        string? Model,
        IReadOnlyCollection<NvidiaChoice>? Choices);

    private sealed record NvidiaChoice(
        NvidiaResponseMessage Message);

    private sealed record NvidiaResponseMessage(
        string? Content);
}
