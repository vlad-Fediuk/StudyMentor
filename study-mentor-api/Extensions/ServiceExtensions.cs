using StudyMentorApi.Authentication.Jwt;
using StudyMentorApi.Services.Ai;
using StudyMentorApi.Services.Ai.Prompts;
using StudyMentorApi.Services.Ai.StructuredOutput;
using StudyMentorApi.LearningContent;

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
        services.AddScoped<LectureChunks.LectureChunkService>();
        services.AddScoped<ChatMessages.ChatMessageService>();
        services.AddScoped<ChatSessions.ChatSessionService>();
        services.AddScoped<AiChat.AiChatService>();
        services.AddScoped<Groups.GroupService>();
        services.AddScoped<Users.UserService>();
        services.AddScoped<IJwtAuthenticationService, JwtAuthenticationService>();
        services.AddScoped<IJwtTokenService, JwtTokenService>();
        services.AddSingleton<JwtAuthenticationValidator>();
        services.AddScoped<Flashcards.FlashcardService>();
        services.AddSingleton<PromptTemplateService>();
        services.AddScoped<LearningContentGenerationService>();
        services.AddScoped<IPromptTemplateProvider, PromptTemplateProvider>();
        services.AddScoped<IPromptComposer, PromptComposer>();
        services.AddScoped<IAiStructuredOutputParser, AiStructuredOutputParser>();
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
        services.AddJwtAuthentication(configuration);

        return services;
    }
}
