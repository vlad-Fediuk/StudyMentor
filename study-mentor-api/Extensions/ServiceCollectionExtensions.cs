using StudyMentorApi.Services;
using StudyMentorApi.Services.Ai;

namespace StudyMentorApi.Extensions;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddStudyMentorServices(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        services.Configure<MongoDbSettings>(
            configuration.GetSection("MongoDbSettings"));
        services.AddSingleton<MongoDbService>();

        services.Configure<NvidiaAiSettings>(
            configuration.GetSection(NvidiaAiSettings.SectionName));
        services.AddHttpClient<IAiChatService, NvidiaAiChatService>();

        return services;
    }
}
