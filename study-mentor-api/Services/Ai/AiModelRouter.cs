using Microsoft.EntityFrameworkCore;
using StudyMentorApi.Data;
using StudyMentorApi.Data.Models;

namespace StudyMentorApi.Services.Ai;

public sealed class AiModelRouter(
    AppDbContext dbContext,
    IEnumerable<IAiProviderClient> providerClients,
    ILogger<AiModelRouter> logger) : IAiChatService
{
    public async Task<AiChatResponse> CompleteAsync(
        AiChatRequest request,
        CancellationToken cancellationToken = default)
    {
        if (request.Messages.Count == 0)
        {
            throw new ArgumentException("AI chat request must contain at least one message.");
        }

        var candidates = await GetCandidatesAsync(request, cancellationToken);
        if (candidates.Count == 0)
        {
            throw new AiGenerationFailedException("No enabled AI provider/model candidates are configured.");
        }

        var clients = providerClients.ToDictionary(
            client => client.ProviderType,
            StringComparer.OrdinalIgnoreCase);
        Exception? lastException = null;

        for (var index = 0; index < candidates.Count; index++)
        {
            var candidate = candidates[index];
            if (!clients.TryGetValue(candidate.Provider.Type, out var client))
            {
                logger.LogWarning(
                    "No AI provider client is registered for provider type {ProviderType}.",
                    candidate.Provider.Type);
                continue;
            }

            try
            {
                var response = await client.CompleteAsync(
                    CreateProviderRequest(request, candidate),
                    cancellationToken);

                return new AiChatResponse(
                    response.Provider,
                    response.Model,
                    response.Content,
                    FallbackUsed: index > 0);
            }
            catch (Exception ex) when (ex is not OperationCanceledException || !cancellationToken.IsCancellationRequested)
            {
                lastException = ex;
                logger.LogError(
                    ex,
                    "AI provider {ProviderName} with model {ModelName} failed. Trying next candidate.",
                    candidate.Provider.Name,
                    candidate.Model.ModelName);
            }
        }

        var lastMessage = lastException?.Message;
        throw new AiGenerationFailedException(
            string.IsNullOrWhiteSpace(lastMessage)
                ? "All AI providers failed."
                : $"All AI providers failed. Last error: {lastMessage}");
    }

    private async Task<List<AiCandidate>> GetCandidatesAsync(
        AiChatRequest request,
        CancellationToken cancellationToken)
    {
        var orderedCandidates = await (
            from provider in dbContext.AiProviders.AsNoTracking()
            where provider.IsEnabled
            from model in provider.Models
            where model.IsEnabled
            orderby provider.Priority, provider.Name, model.Priority, model.ModelName
            select new AiCandidate(provider, model))
            .ToListAsync(cancellationToken);

        var preferredCandidate = orderedCandidates.FirstOrDefault(candidate =>
            MatchesPreferredProvider(request.Provider, candidate.Provider)
            && MatchesPreferredModel(request.Model, candidate.Model));

        if (preferredCandidate is null)
        {
            return orderedCandidates;
        }

        return orderedCandidates
            .Where(candidate => candidate.Model.Id != preferredCandidate.Model.Id)
            .Prepend(preferredCandidate)
            .ToList();
    }

    private static bool MatchesPreferredProvider(string? preferredProvider, AiProvider provider)
    {
        return string.IsNullOrWhiteSpace(preferredProvider)
            || string.Equals(provider.Type, preferredProvider, StringComparison.OrdinalIgnoreCase)
            || string.Equals(provider.Name, preferredProvider, StringComparison.OrdinalIgnoreCase);
    }

    private static bool MatchesPreferredModel(string? preferredModel, AiModel model)
    {
        return string.IsNullOrWhiteSpace(preferredModel)
            || string.Equals(model.ModelName, preferredModel, StringComparison.OrdinalIgnoreCase)
            || string.Equals(model.DisplayName, preferredModel, StringComparison.OrdinalIgnoreCase);
    }

    private static AiProviderRequest CreateProviderRequest(
        AiChatRequest request,
        AiCandidate candidate)
    {
        return new AiProviderRequest
        {
            ProviderName = candidate.Provider.Name,
            ProviderType = candidate.Provider.Type,
            BaseUrl = candidate.Provider.BaseUrl,
            ApiKeyEnvironmentVariable = candidate.Provider.ApiKeyEnvironmentVariable,
            Model = candidate.Model.ModelName,
            Messages = request.Messages,
            MaxOutputTokens = candidate.Model.MaxOutputTokens,
            ReasoningBudget = candidate.Model.ReasoningBudget,
            TimeoutSeconds = candidate.Provider.TimeoutSeconds,
            Temperature = candidate.Model.Temperature,
            TopP = candidate.Model.TopP,
            EnableThinking = candidate.Model.EnableThinking
        };
    }

    private sealed record AiCandidate(AiProvider Provider, AiModel Model);
}
