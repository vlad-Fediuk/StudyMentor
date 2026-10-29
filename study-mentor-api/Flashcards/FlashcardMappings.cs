using StudyMentorApi.Data.Models;

namespace StudyMentorApi.Flashcards;

public static class FlashcardMappings
{
    public static FlashcardResponse ToResponse(this Flashcard flashcard) => new(
        flashcard.Id, flashcard.Name, flashcard.ChatMessageId,
        flashcard.Cards.Select(card => new CardResponse(card.Id, card.Term, card.Definition)).ToList());

    public static FlashcardV2Response ToV2Response(this Flashcard flashcard)
    {
        var response = flashcard.ToResponse();
        return new(response.Id, response.Name, response.ChatMessageId, response.Cards, response.Cards.Count);
    }
}
