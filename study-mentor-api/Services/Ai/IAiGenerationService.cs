namespace StudyMentorApi.Services.Ai;

public interface IAiGenerationService
{
    Task<AiGenerationResponse> GenerateAsync(
        AiGenerationRequest request,
        CancellationToken cancellationToken = default);

    Task<TOutput> GenerateStructuredAsync<TOutput>(
        AiGenerationRequest request,
        CancellationToken cancellationToken = default);
}
