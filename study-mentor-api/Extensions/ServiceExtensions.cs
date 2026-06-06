using StudyMentorApi.Services.Ai;
using StudyMentorApi.Services.Ai.Prompts;

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
        services.AddScoped<ChatSessions.ChatSessionService>();
        services.AddScoped<AiChat.AiChatService>();
        services.AddScoped<Groups.GroupService>();
        services.AddScoped<Users.UserService>();
        services.AddScoped<IPromptComposer, PromptComposer>();
        services.AddScoped<IAiGenerationService, AiGenerationService>();
        services.AddScoped<IAiChatService, AiModelRouter>();
        services.AddHttpClient<IAiProviderClient, LmStudioProviderClient>(client =>
        {
            client.Timeout = Timeout.InfiniteTimeSpan;
        });
        services.AddHttpClient<IAiProviderClient, NvidiaProviderClient>(client =>
        {
            client.Timeout = Timeout.InfiniteTimeSpan;
        });

        return services;
    }
}
