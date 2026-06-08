namespace StudyMentorApi.Services.Ai.Prompts;

public interface IPromptComposer
{
    Task<ComposedPrompt> ComposeAsync(
        PromptCompositionRequest request,
        CancellationToken cancellationToken = default);
}
