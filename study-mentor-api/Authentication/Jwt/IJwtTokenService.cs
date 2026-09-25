using StudyMentorApi.Data.Models;

namespace StudyMentorApi.Authentication.Jwt;

public interface IJwtTokenService
{
    Task<string> CreateTokenAsync(User user);
}
