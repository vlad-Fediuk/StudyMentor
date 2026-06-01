namespace StudyMentorApi.Services.Ai;

public interface IAiChatService
{
    Task<AiChatResponse> CompleteAsync(
        AiChatRequest request,
        CancellationToken cancellationToken = default);
}
