using StudyMentorApi.Services.Ai;

namespace StudyMentorApi.Extensions;

public static class AiServiceCollectionExtensions
{
    public static IServiceCollection AddAiServices(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        services.Configure<NvidiaAiSettings>(
            configuration.GetSection(NvidiaAiSettings.SectionName));
        services.AddHttpClient<IAiChatService, NvidiaAiChatService>();

        return services;
    }
}
