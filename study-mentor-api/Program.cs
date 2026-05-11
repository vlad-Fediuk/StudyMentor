using Microsoft.AspNetCore.Diagnostics;
using StudyMentorApi.ChatMessages;
using StudyMentorApi.Common;
using StudyMentorApi.Extensions;
using StudyMentorApi.Lectures;
using StudyMentorApi.Majors;
using StudyMentorApi.Services;
using StudyMentorApi.Subjects;

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

        builder.Services.Configure<MongoDbSettings>(
            builder.Configuration.GetSection("MongoDbSettings"));
        builder.Services.AddSingleton<MongoDbService>();

        builder.Services.AddCors(options =>
        {
            options.AddDefaultPolicy(policy =>
            {
                policy
                    .WithOrigins("http://localhost:5173", "http://localhost:4173")
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

        app.UseExceptionHandler(errorApp =>
        {
            errorApp.Run(async context =>
            {
                var exception = context.Features
                    .Get<IExceptionHandlerFeature>()?.Error;

                (int statusCode, string message) = exception switch
                {
                    NotFoundException ex => (404, ex.Message),
                    ValidationException ex => (400, ex.Message),
                    _ => (500, "An unexpected error occurred.")
                };

                context.Response.StatusCode = statusCode;
                context.Response.ContentType = "application/json";

                await context.Response.WriteAsJsonAsync(new
                {
                    error = message,
                    statusCode
                });
            });
        });

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
