namespace StudyMentorApi.Authentication.Jwt;

public class JwtAuthenticationRequest
{
    public JwtAuthenticationRequest()
    {
    }

    [System.Diagnostics.CodeAnalysis.SetsRequiredMembers]
    public JwtAuthenticationRequest(string token)
    {
        Token = token;
    }

    public required string Token { get; set; }
}
