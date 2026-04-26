namespace study_mentor_api.Extensions;

public static class ServiceExtensions
{
    public static IServiceCollection AddApplicationServices(
        this IServiceCollection services)
    {
        // We use Scoped for services and repositories
        // services.AddScoped<IExampleRepository, ExampleRepository>();

        return services;
    }
}