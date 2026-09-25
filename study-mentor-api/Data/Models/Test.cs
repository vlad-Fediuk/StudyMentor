namespace StudyMentorApi.Data.Models;

public class Test : Exercise
{
    public Guid? SourceFlashcardId { get; set; }

    public Flashcard? SourceFlashcard { get; set; }

    public List<TestQuestion> Questions { get; set; } = [];
}
