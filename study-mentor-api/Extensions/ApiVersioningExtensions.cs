using Asp.Versioning;

namespace StudyMentorApi.Extensions;

public static class ApiVersioningExtensions
{
    public static IServiceCollection AddApplicationApiVersioning(this IServiceCollection services)
    {
        services.AddProblemDetails();
        services.AddApiVersioning(options =>
        {
            options.AssumeDefaultVersionWhenUnspecified = true;
            options.ReportApiVersions = true;
            options.ApiVersionReader = new UrlSegmentApiVersionReader();
        });
        return services;
    }
}
