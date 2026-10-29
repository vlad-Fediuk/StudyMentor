using System.Security.Claims;
using Microsoft.AspNetCore.Http;
using StudyMentorApi.Users;

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
        return principal?.IsInRole(UserRoles.Admin) == true;
    }

    public static IResult? AuthorizeOwnerOrAdmin(this ClaimsPrincipal? principal, Guid resourceOwnerId)
    {
        var currentUserId = principal.GetUserId();
        if (currentUserId == null)
        {
            return Results.Unauthorized();
        }

        if (currentUserId.Value != resourceOwnerId && !principal.IsAdmin())
        {
            return Results.Forbid();
        }

        return null;
    }
}
