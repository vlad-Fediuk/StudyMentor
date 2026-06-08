namespace StudyMentorApi.Flashcards;

public record FlashcardRequest(
    string? Name,
    Guid ChatMessageId,
    IReadOnlyCollection<CardRequest>? Cards);
