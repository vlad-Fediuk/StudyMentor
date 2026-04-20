using Microsoft.EntityFrameworkCore;
using study_mentor_api.Data;

namespace study_mentor_api.Extensions;

public static class ServiceExtensions
{
    public static IServiceCollection AddApplicationServices(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        // Використовуємо Scoped для сервісів та репозиторіїв —
        // один екземпляр на HTTP-запит, що є оптимальним для роботи з DbContext.
        services.AddDbContext<AppDbContext>(options =>
            options.UseSqlServer(configuration.GetConnectionString("DefaultConnection")));

        return services;
    }
}