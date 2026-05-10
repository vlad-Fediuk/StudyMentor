using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using Microsoft.Extensions.Options;
using StudyMentorApi.Extensions;

namespace StudyMentorApi.Services.Ai;

public sealed class NvidiaAiChatService : IAiChatService
{
    private const string ProviderName = "nvidia";

    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.SnakeCaseLower,
        DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull
    };

    private readonly HttpClient _httpClient;
    private readonly NvidiaAiSettings _settings;

    public NvidiaAiChatService(
        HttpClient httpClient,
        IOptions<NvidiaAiSettings> settings)
    {
        _httpClient = httpClient;
        _settings = settings.Value;
    }

    public async Task<AiChatResponse> CompleteAsync(
        AiChatRequest request,
        CancellationToken cancellationToken = default)
    {
        if (request.Messages.Count == 0)
        {
            throw new ArgumentException("AI chat request must contain at least one message.");
        }

        var apiKey = GetApiKey();
        if (string.IsNullOrWhiteSpace(apiKey))
        {
            throw new InvalidOperationException(
                "NVIDIA API key is not configured. Set NVIDIA_API_KEY or AiProviders:Nvidia:ApiKey.");
        }

        var model = ResolveModel(request.Model);
        var payload = new NvidiaChatCompletionRequest(
            model,
            request.Messages.Select(message => new NvidiaChatMessage(message.Role, message.Content)).ToArray(),
            _settings.MaxOutputTokens,
            _settings.Temperature,
            _settings.TopP,
            false,
            new NvidiaChatTemplateOptions(_settings.EnableThinking),
            _settings.EnableThinking ? _settings.ReasoningBudget : null);

        using var httpRequest = new HttpRequestMessage(HttpMethod.Post, _settings.InvokeUrl)
        {
            Content = new StringContent(
                JsonSerializer.Serialize(payload, JsonOptions),
                Encoding.UTF8,
                "application/json")
        };

        httpRequest.Headers.Authorization = new AuthenticationHeaderValue("Bearer", apiKey);
        httpRequest.Headers.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

        using var response = await _httpClient.SendAsync(httpRequest, cancellationToken);
        var responseBody = await response.Content.ReadAsStringAsync(cancellationToken);

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

        return new AiChatResponse(ProviderName, completion?.Model ?? model, content);
    }

    private string GetApiKey()
    {
        return !string.IsNullOrWhiteSpace(_settings.ApiKey)
            ? _settings.ApiKey
            : Environment.GetEnvironmentVariable("NVIDIA_API_KEY") ?? string.Empty;
    }

    private string ResolveModel(string? requestedModel)
    {
        var model = string.IsNullOrWhiteSpace(requestedModel)
            ? _settings.DefaultModel
            : requestedModel;

        if (!_settings.SupportedModels.Contains(model, StringComparer.OrdinalIgnoreCase))
        {
            throw new ArgumentException(
                $"AI model '{model}' is not supported. Supported models: {string.Join(", ", _settings.SupportedModels)}.");
        }

        return model;
    }

    private sealed record NvidiaChatCompletionRequest(
        string Model,
        IReadOnlyCollection<NvidiaChatMessage> Messages,
        int MaxTokens,
        double Temperature,
        double TopP,
        bool Stream,
        [property: JsonPropertyName("chat_template_kwargs")]
        NvidiaChatTemplateOptions ChatTemplateOptions,
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
