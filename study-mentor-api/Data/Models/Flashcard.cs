namespace StudyMentorApi.Data.Models;

public class Flashcard : Exercise
{
    public List<Card> Cards { get; set; } = [];
}
