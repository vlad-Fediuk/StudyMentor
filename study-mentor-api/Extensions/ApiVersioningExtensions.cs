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
        }).AddApiExplorer(options =>
        {
            options.GroupNameFormat = "'v'VVV";
            options.SubstituteApiVersionInUrl = true;
        }).AddOpenApi();
        services.AddSwaggerGen(options =>
        {
            options.SwaggerDoc("v1", new() { Title = "Study Mentor API", Version = "v1" });
            options.SwaggerDoc("v2", new() { Title = "Study Mentor API", Version = "v2" });
        });
        return services;
    }
}
