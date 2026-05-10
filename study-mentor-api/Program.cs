using StudyMentorApi.Extensions;
using StudyMentorApi.Lectures;
using StudyMentorApi.Majors;
using StudyMentorApi.Services;
using StudyMentorApi.Subjects;
using StudyMentorApi.ChatMessages;
namespace StudyMentorApi;

public class Program
{
    public static void Main(string[] args)
    {
        var builder = WebApplication.CreateBuilder(args);

        builder.Services.AddControllers();
        builder.Services.AddOpenApi();
        builder.Services.AddSwaggerGen();
        builder.Services.AddApplicationServices();

        // Add MongoDB settings
        builder.Services.Configure<MongoDbSettings>(
            builder.Configuration.GetSection("MongoDbSettings"));
        builder.Services.AddSingleton<MongoDbService>();
        
        builder.Services.AddCors(options =>
        {
            options.AddDefaultPolicy(policy =>
            {
                policy
                    .SetIsOriginAllowed(origin =>
                    {
                        if (!Uri.TryCreate(origin, UriKind.Absolute, out var uri))
                        {
                            return false;
                        }

                        return uri.Host is "localhost" or "127.0.0.1";
                    })
                    .AllowAnyHeader()
                    .AllowAnyMethod();
            });
        });

        var app = builder.Build();

        if (app.Environment.IsDevelopment())
        {
            app.MapOpenApi();

            app.UseSwagger();
            app.UseSwaggerUI(c =>
            {
                c.SwaggerEndpoint("/swagger/v1/swagger.json", "Study Mentor API V1");
                c.RoutePrefix = string.Empty;
            });
        }
        
        app.UseCors();
        app.UseHttpsRedirection();
        app.UseAuthorization();
        app.MapControllers();
        app.MapMajorEndpoints();
        app.MapSubjectEndpoints();
        app.MapLectureEndpoints();
        app.MapChatMessageEndpoints();
        app.Run();
    }
}
