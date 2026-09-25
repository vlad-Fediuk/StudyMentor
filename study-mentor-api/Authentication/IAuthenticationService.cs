namespace StudyMentorApi.Authentication;

public interface IAuthenticationService<in TRequest>
{
    Task<string> AuthenticateAsync(TRequest request);
}
