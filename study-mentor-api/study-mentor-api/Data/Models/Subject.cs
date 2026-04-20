namespace study_mentor_api.Data.Models;

public enum Major
{
    ComputerScience,
    Economics,
    Law
}

public class Subject
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public Major Major { get; set; }
}