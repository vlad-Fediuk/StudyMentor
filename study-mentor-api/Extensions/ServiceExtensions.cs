using StudyMentorApi.Services;
using StudyMentorApi.Services.Ai;

namespace StudyMentorApi.Extensions;

public static class ServiceExtensions
{
    public static IServiceCollection AddApplicationServices(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        // We use Scoped for services and repositories
        services.AddScoped<Majors.MajorService>();
        services.AddScoped<Subjects.SubjectService>();
        services.AddScoped<Lectures.LectureService>();
        services.AddScoped<ChatMessages.ChatMessageService>();
        services.AddScoped<Users.UserService>();

        services.Configure<MongoDbSettings>(
            configuration.GetSection("MongoDbSettings"));
        services.AddSingleton<MongoDbService>();

        services.Configure<NvidiaAiSettings>(
            configuration.GetSection(NvidiaAiSettings.SectionName));
        services.AddHttpClient<IAiChatService, NvidiaAiChatService>();

        return services;
    }
}
