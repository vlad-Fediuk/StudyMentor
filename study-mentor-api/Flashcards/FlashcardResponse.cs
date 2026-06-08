namespace StudyMentorApi.Flashcards;

public record FlashcardResponse(
    Guid Id,
    string Name,
    Guid ChatMessageId,
    IReadOnlyCollection<CardResponse> Cards);
