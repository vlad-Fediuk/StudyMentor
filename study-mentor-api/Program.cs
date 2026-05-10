using StudyMentorApi.Extensions;
using StudyMentorApi.Diagnostics;
using StudyMentorApi.Services;
namespace study_mentor_api;

public class Program
{
    public static async Task Main(string[] args)
    {
        var builder = WebApplication.CreateBuilder(args);

        // Add services to the container.
        // Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
        builder.Services.AddOpenApi();
        builder.Services.AddStudyMentorServices(builder.Configuration);

        var app = builder.Build();

        if (args.Contains("--ai-smoke-test", StringComparer.OrdinalIgnoreCase))
        {
            Environment.ExitCode = await AiSmokeTestRunner.RunAsync(app.Services);
            return;
        }

        // Configure the HTTP request pipeline.
        if (app.Environment.IsDevelopment())
        {
            app.MapOpenApi();
        }

        app.UseHttpsRedirection();

        app.UseAuthorization();

        app.Run();
    }
}
