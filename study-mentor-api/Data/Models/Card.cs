namespace StudyMentorApi.Data.Models;

public class Card : BaseEntity
{
    public required string Term { get; set; }

    public required string Definition { get; set; }

    public Guid FlashcardId { get; set; }

    public Flashcard Flashcard { get; set; } = null!;
}
