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
        services.AddScoped<Flashcards.FlashcardService>();
        services.AddScoped<Tests.TestService>();
        services.AddSingleton<PromptTemplateService>();

        services.Configure<NvidiaAiSettings>(
            configuration.GetSection(NvidiaAiSettings.SectionName));
        services.AddHttpClient<IAiChatService, NvidiaAiChatService>((serviceProvider, client) =>
        {
            var settings = serviceProvider
                .GetRequiredService<Microsoft.Extensions.Options.IOptions<NvidiaAiSettings>>()
                .Value;

            client.Timeout = TimeSpan.FromSeconds(settings.TimeoutSeconds);
        });

        return services;
    }
}
