namespace study_mentor_api.Extensions;

public static class ServiceExtensions
{
    public static IServiceCollection AddApplicationServices(
        this IServiceCollection services)
    {
        // Використовуємо Scoped для сервісів та репозиторіїв —
        // один екземпляр на HTTP-запит, що є оптимальним для роботи з DbContext.
        // services.AddScoped<IExampleRepository, ExampleRepository>();
        // services.AddScoped<IExampleService, ExampleService>();

        return services;
    }
}