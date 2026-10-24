using System.Security.Claims;

namespace StudyMentorApi.Extensions;

public static class ClaimsPrincipalExtensions
{
    public static Guid? GetUserId(this ClaimsPrincipal? principal)
    {
        var value = principal?
            .FindFirst(ClaimTypes.NameIdentifier)?.Value;

        return Guid.TryParse(value, out var id) ? id : null;
    }

    public static bool IsAdmin(this ClaimsPrincipal? principal)
    {
        return principal?.IsInRole("Admin") == true;
    }
}
