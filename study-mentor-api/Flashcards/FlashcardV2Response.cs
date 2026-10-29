namespace StudyMentorApi.Flashcards;

public sealed record FlashcardV2Response(
    Guid Id,
    string Name,
    Guid ChatMessageId,
    IReadOnlyCollection<CardResponse> Cards,
    int CardsCount);

public sealed record FlashcardPageResponse(
    IReadOnlyCollection<FlashcardV2Response> Items,
    int Page,
    int PageSize,
    int Total);
