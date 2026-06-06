using StudyMentorApi.Services.Ai.Prompts;

namespace StudyMentorApi.Services.Ai;

public sealed class AiGenerationService(
    IAiChatService aiChatService,
    IPromptComposer promptComposer) : IAiGenerationService
{
    public async Task<AiGenerationResponse> GenerateAsync(
        AiGenerationRequest request,
        CancellationToken cancellationToken = default)
    {
        var composedPrompt = await promptComposer.ComposeAsync(new PromptCompositionRequest
        {
            TaskType = request.TaskType,
            UserMessage = request.UserMessage,
            ConversationHistory = request.ConversationHistory,
            Context = request.Context,
            UserProfile = request.UserProfile,
            OutputFormat = request.OutputFormat,
            ResponseSchema = request.ResponseSchema
        }, cancellationToken);

        var aiResponse = await aiChatService.CompleteAsync(new AiChatRequest
        {
            Provider = request.PreferredProvider,
            Model = request.PreferredModel,
            Messages =
            [
                new AiChatMessage("user", composedPrompt.Content)
            ]
        }, cancellationToken);

        return new AiGenerationResponse(
            aiResponse.Content,
            aiResponse.Provider,
            aiResponse.Model,
            aiResponse.FallbackUsed,
            request.TaskType);
    }

    public Task<TOutput> GenerateStructuredAsync<TOutput>(
        AiGenerationRequest request,
        CancellationToken cancellationToken = default)
    {
        throw new NotSupportedException(
            "Structured AI generation is not implemented in this branch. It will be added in ai-structured-output.");
    }

}
