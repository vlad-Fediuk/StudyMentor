using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using Microsoft.Extensions.Options;

namespace StudyMentorApi.Services.Ai.Embeddings;

public sealed class LmStudioEmbeddingService(
    HttpClient httpClient,
    IOptions<AiEmbeddingOptions> options) : IAiEmbeddingService
{
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.SnakeCaseLower,
        DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull
    };

    public async Task<AiEmbeddingResult> CreateEmbeddingAsync(
        string input,
        CancellationToken cancellationToken = default)
    {
        var embeddingOptions = options.Value;
        if (!string.Equals(embeddingOptions.Provider, "lmstudio", StringComparison.OrdinalIgnoreCase))
        {
            throw new AiEmbeddingUnavailableException(
                $"Unsupported embedding provider '{embeddingOptions.Provider}'. Only 'lmstudio' is configured.");
        }

        var lmStudio = embeddingOptions.LmStudio;
        if (string.IsNullOrWhiteSpace(lmStudio.BaseUrl))
        {
            throw new AiEmbeddingUnavailableException("LM Studio embeddings BaseUrl is not configured.");
        }

        if (string.IsNullOrWhiteSpace(lmStudio.Model))
        {
            throw new AiEmbeddingUnavailableException(
                "LM Studio embeddings model is not configured. Set AiEmbeddings:LmStudio:Model.");
        }

        if (string.IsNullOrWhiteSpace(input))
        {
            throw new AiEmbeddingUnavailableException("Embedding input is empty.");
        }

        var payload = new LmStudioEmbeddingRequest(
            lmStudio.Model.Trim(),
            input.Trim(),
            "float");

        using var requestContent = new StringContent(
            JsonSerializer.Serialize(payload, JsonOptions),
            Encoding.UTF8,
            "application/json");
        requestContent.Headers.ContentType = new MediaTypeHeaderValue("application/json");

        using var httpRequest = new HttpRequestMessage(HttpMethod.Post, lmStudio.BaseUrl)
        {
            Content = requestContent
        };
        httpRequest.Headers.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

        using var timeout = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
        timeout.CancelAfter(TimeSpan.FromSeconds(Math.Max(1, lmStudio.TimeoutSeconds)));

        try
        {
            using var response = await httpClient.SendAsync(httpRequest, timeout.Token);
            var responseBody = await response.Content.ReadAsStringAsync(timeout.Token);

            if (!response.IsSuccessStatusCode)
            {
                throw new AiEmbeddingUnavailableException(
                    $"LM Studio embeddings request failed with status {(int)response.StatusCode}: {responseBody}");
            }

            var embeddingResponse = JsonSerializer.Deserialize<LmStudioEmbeddingResponse>(
                responseBody,
                JsonOptions);

            var vector = embeddingResponse?.Data?.FirstOrDefault()?.Embedding;
            if (vector is null || vector.Length == 0)
            {
                throw new AiEmbeddingUnavailableException(
                    "LM Studio embeddings response did not contain a vector.");
            }

            var model = string.IsNullOrWhiteSpace(embeddingResponse?.Model)
                ? lmStudio.Model.Trim()
                : embeddingResponse.Model.Trim();

            return new AiEmbeddingResult(model, vector);
        }
        catch (AiEmbeddingUnavailableException)
        {
            throw;
        }
        catch (Exception ex) when (ex is not OperationCanceledException || !cancellationToken.IsCancellationRequested)
        {
            throw new AiEmbeddingUnavailableException("LM Studio embeddings request failed.", ex);
        }
    }

    private sealed record LmStudioEmbeddingRequest(
        string Model,
        string Input,
        string EncodingFormat);

    private sealed record LmStudioEmbeddingResponse(
        string? Model,
        IReadOnlyCollection<LmStudioEmbeddingData>? Data);

    private sealed record LmStudioEmbeddingData(
        float[] Embedding);
}
