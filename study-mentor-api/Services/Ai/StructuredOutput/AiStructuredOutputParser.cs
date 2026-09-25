using System.ComponentModel.DataAnnotations;
using System.Text.Json;

namespace StudyMentorApi.Services.Ai.StructuredOutput;

public sealed class AiStructuredOutputParser : IAiStructuredOutputParser
{
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNameCaseInsensitive = true,
        ReadCommentHandling = JsonCommentHandling.Skip,
        AllowTrailingCommas = true
    };

    public TOutput ParseAndValidate<TOutput>(string content)
    {
        var json = StripMarkdownFence(content);
        if (string.IsNullOrWhiteSpace(json))
        {
            throw new AiStructuredOutputParseException("Structured AI response is empty.");
        }

        TOutput? output;
        try
        {
            output = JsonSerializer.Deserialize<TOutput>(json, JsonOptions);
        }
        catch (JsonException ex)
        {
            throw new AiStructuredOutputParseException("Structured AI response is not valid JSON.", ex);
        }

        if (output is null)
        {
            throw new AiStructuredOutputParseException("Structured AI response parsed to null.");
        }

        ValidateKnownOutput(output);
        ValidateDataAnnotations(output);

        return output;
    }

    private static string StripMarkdownFence(string content)
    {
        if (string.IsNullOrWhiteSpace(content))
        {
            return string.Empty;
        }

        var trimmed = content.Trim();
        if (!trimmed.StartsWith("```", StringComparison.Ordinal))
        {
            return trimmed;
        }

        var firstLineEnd = trimmed.IndexOf('\n');
        if (firstLineEnd < 0)
        {
            return string.Empty;
        }

        var withoutOpeningFence = trimmed[(firstLineEnd + 1)..].Trim();
        if (withoutOpeningFence.EndsWith("```", StringComparison.Ordinal))
        {
            withoutOpeningFence = withoutOpeningFence[..^3].Trim();
        }

        return withoutOpeningFence;
    }

    private static void ValidateKnownOutput<TOutput>(TOutput output)
    {
        switch (output)
        {
            case GeneratedTestDto test:
                ValidateGeneratedTest(test);
                break;
            case GeneratedFlashcardsDto flashcards:
                ValidateGeneratedFlashcards(flashcards);
                break;
        }
    }

    private static void ValidateGeneratedTest(GeneratedTestDto test)
    {
        RequireText(test.Title, "Generated test title is required.");
        if (test.Questions is null || test.Questions.Count == 0)
        {
            throw new AiStructuredOutputParseException("Generated test questions are required.");
        }

        var index = 0;
        foreach (var question in test.Questions)
        {
            index++;
            RequireText(question.Text, $"Question {index} text is required.");
            RequireText(question.Type, $"Question {index} type is required.");
            RequireText(question.CorrectAnswer, $"Question {index} correct answer is required.");

            if (RequiresOptions(question.Type) && (question.Options is null || question.Options.Count == 0))
            {
                throw new AiStructuredOutputParseException(
                    $"Question {index} options are required for question type '{question.Type}'.");
            }
        }
    }

    private static void ValidateGeneratedFlashcards(GeneratedFlashcardsDto flashcards)
    {
        if (flashcards.Cards is null || flashcards.Cards.Count == 0)
        {
            throw new AiStructuredOutputParseException("Generated flashcards are required.");
        }

        var index = 0;
        foreach (var card in flashcards.Cards)
        {
            index++;
            RequireText(card.Front, $"Flashcard {index} front is required.");
            RequireText(card.Back, $"Flashcard {index} back is required.");
        }
    }

    private static void ValidateDataAnnotations<TOutput>(TOutput output)
    {
        var validationContext = new ValidationContext(output!);
        var validationResults = new List<ValidationResult>();
        if (!Validator.TryValidateObject(output!, validationContext, validationResults, validateAllProperties: true))
        {
            var errors = string.Join("; ", validationResults.Select(result => result.ErrorMessage));
            throw new AiStructuredOutputParseException($"Structured AI response validation failed: {errors}");
        }
    }

    private static bool RequiresOptions(string? questionType)
    {
        if (string.IsNullOrWhiteSpace(questionType))
        {
            return false;
        }

        var normalized = questionType.Trim().ToLowerInvariant();
        return normalized.Contains("choice", StringComparison.Ordinal)
            || normalized.Contains("multiple", StringComparison.Ordinal)
            || normalized.Contains("single", StringComparison.Ordinal)
            || normalized.Contains("mcq", StringComparison.Ordinal);
    }

    private static void RequireText(string? value, string message)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            throw new AiStructuredOutputParseException(message);
        }
    }
}
