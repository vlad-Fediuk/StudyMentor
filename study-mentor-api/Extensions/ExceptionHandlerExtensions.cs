using Microsoft.AspNetCore.Diagnostics;
using StudyMentorApi.Common;

namespace StudyMentorApi.Extensions;

public static class ExceptionHandlerExtensions
{
    public static void UseGlobalExceptionHandler(this WebApplication app)
    {
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
    }
}
