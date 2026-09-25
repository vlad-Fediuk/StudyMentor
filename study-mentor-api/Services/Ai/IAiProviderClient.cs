namespace StudyMentorApi.Services.Ai;

public interface IAiProviderClient
{
    string ProviderType { get; }

    Task<AiProviderResponse> CompleteAsync(
        AiProviderRequest request,
        CancellationToken cancellationToken = default);
}
