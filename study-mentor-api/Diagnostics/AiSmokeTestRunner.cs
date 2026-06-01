using Microsoft.Extensions.DependencyInjection;
using StudyMentorApi.Services.Ai;

namespace StudyMentorApi.Diagnostics;

internal static class AiSmokeTestRunner
{
    public static async Task<int> RunAsync(
        IServiceProvider services,
        CancellationToken cancellationToken = default)
    {
        var aiChatService = services.GetRequiredService<IAiChatService>();
        var prompt = Environment.GetEnvironmentVariable("AI_SMOKE_TEST_PROMPT");
        var model = Environment.GetEnvironmentVariable("AI_SMOKE_TEST_MODEL");

        var request = new AiChatRequest
        {
            Model = string.IsNullOrWhiteSpace(model) ? null : model,
            Messages =
            [
                new AiChatMessage(
                    "user",
                    string.IsNullOrWhiteSpace(prompt)
                        ? "Reply with a short confirmation that the NVIDIA integration works."
                        : prompt)
            ]
        };

        var response = await aiChatService.CompleteAsync(request, cancellationToken);

        Console.WriteLine($"Provider: {response.Provider}");
        Console.WriteLine($"Model: {response.Model}");
        Console.WriteLine("Content:");
        Console.WriteLine(response.Content);

        return 0;
    }
}
