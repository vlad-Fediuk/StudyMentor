namespace StudyMentorApi.Flashcards;

public record CardResponse(
    Guid Id,
    string Term,
    string Definition);
