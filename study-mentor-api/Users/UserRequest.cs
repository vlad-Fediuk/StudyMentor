using System.ComponentModel.DataAnnotations;

namespace StudyMentorApi.Users;

public record UserRequest(
    string Name,
    [Required][EmailAddress] string Email,
    [Required] string Password,
    string GroupId);

public record LoginRequest(
    [Required][EmailAddress] string Email,
    [Required] string Password);
    