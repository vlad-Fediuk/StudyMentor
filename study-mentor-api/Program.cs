using Microsoft.EntityFrameworkCore;
using StudyMentorApi.AiChat;
using StudyMentorApi.Authentication;
using StudyMentorApi.Data;
using StudyMentorApi.ChatMessages;
using StudyMentorApi.ChatSessions;
using StudyMentorApi.Extensions;
using StudyMentorApi.Flashcards;
using StudyMentorApi.Groups;
using StudyMentorApi.Lectures;
using StudyMentorApi.Majors;
using StudyMentorApi.Subjects;
using StudyMentorApi.Users;

namespace StudyMentorApi;

public class Program
{
    public static async Task Main(string[] args)
    {
        var builder = WebApplication.CreateBuilder(args);

        builder.Services.AddControllers();
        builder.Services.AddOpenApi();
        builder.Services.AddSwaggerGen();

        builder.Services.AddDbContext<AppDbContext>(options =>
            options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection")));
        builder.Services.AddApplicationServices(builder.Configuration);

        builder.Services.AddCors(options =>
        {
            options.AddDefaultPolicy(policy =>
            {
                policy
                    .WithOrigins(
                        "http://localhost:4200",
                        "http://localhost:4300",
                        "http://localhost:5173",
                        "http://localhost:4173")
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

        app.UseGlobalExceptionHandler();
        app.UseCors();
        app.UseHttpsRedirection();
        app.UseAuthentication();
        app.UseAuthorization();
        app.MapControllers();
        app.MapAuthenticationEndpoints();
        app.MapMajorEndpoints();
        app.MapSubjectEndpoints();
        app.MapLectureEndpoints();
        app.MapChatMessageEndpoints();
        app.MapAiChatEndpoints();
        app.MapGroupEndpoints();
        app.MapUserEndpoints();
        app.MapChatSessionEndpoints();
        app.MapFlashcardEndpoints();
        app.Run();
    }
}
