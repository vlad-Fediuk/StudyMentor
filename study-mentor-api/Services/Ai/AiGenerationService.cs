using StudyMentorApi.Services.Ai.Prompts;
using StudyMentorApi.Services.Ai.StructuredOutput;

namespace StudyMentorApi.Services.Ai;

public sealed class AiGenerationService(
    IAiChatService aiChatService,
    IPromptComposer promptComposer,
    IAiStructuredOutputParser structuredOutputParser) : IAiGenerationService
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

    public async Task<TOutput> GenerateStructuredAsync<TOutput>(
        AiGenerationRequest request,
        CancellationToken cancellationToken = default)
    {
        var structuredRequest = request with
        {
            OutputFormat = AiOutputFormat.Json
        };

        var response = await GenerateAsync(structuredRequest, cancellationToken);
        try
        {
            return structuredOutputParser.ParseAndValidate<TOutput>(response.Content);
        }
        catch (AiStructuredOutputParseException firstException)
        {
            var repairResponse = await GenerateAsync(
                CreateRepairRequest<TOutput>(structuredRequest, response.Content, firstException),
                cancellationToken);

            return structuredOutputParser.ParseAndValidate<TOutput>(repairResponse.Content);
        }

    }

    private static AiGenerationRequest CreateRepairRequest<TOutput>(
        AiGenerationRequest request,
        string invalidContent,
        AiStructuredOutputParseException parseException)
    {
        return request with
        {
            OutputFormat = AiOutputFormat.Json,
            UserMessage = $"""
                Repair this AI response so it is valid JSON only.

                Rules:
                - Return JSON only.
                - Do not include markdown.
                - Do not include explanations.
                - Preserve the original intended data.
                - Target type: {typeof(TOutput).Name}
                - Parse error: {parseException.Message}

                Response schema or rules:
                {NormalizeOptional(request.ResponseSchema, "No explicit schema was provided.")}

                Invalid JSON content:
                {invalidContent}
                """
        };
    }

    private static string NormalizeOptional(string? value, string fallback)
    {
        return string.IsNullOrWhiteSpace(value) ? fallback : value.Trim();
    }
}
