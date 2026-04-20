namespace study_mentor_api.Data.Models;

public class User
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Password { get; set; } = string.Empty;
    public int GroupId { get; set; }
    public Group Group { get; set; } = null!;
}