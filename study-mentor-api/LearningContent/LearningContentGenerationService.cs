using StudyMentorApi.Common;
using StudyMentorApi.AiChat;
using StudyMentorApi.ChatMessages;
using StudyMentorApi.ChatSessions;
using StudyMentorApi.Data;
using StudyMentorApi.Data.Models;
using StudyMentorApi.Flashcards;
using StudyMentorApi.Lectures;
using StudyMentorApi.Services.Ai;
using StudyMentorApi.Services.Ai.StructuredOutput;
using StudyMentorApi.Tests;

namespace StudyMentorApi.LearningContent;

public class LearningContentGenerationService(
    IAiGenerationService aiGenerationService,
    LectureService lectureService,
    ChatSessionService chatSessionService,
    ChatMessageService chatMessageService,
    FlashcardService flashcardService,
    TestService testService,
    AppDbContext dbContext)
{
    private const string TestType = "test";
    private const string FlashcardType = "flashcard";

    private const string TestResponseSchema = """
        {
          "title": "string",
          "questions": [
            {
              "text": "string",
              "type": "single_choice",
              "options": ["string", "string", "string", "string"],
              "correctAnswer": "string",
              "explanation": "string"
            }
          ]
        }

        Rules:
        - Return one JSON object only.
        - Every question must have exactly one correctAnswer.
        - correctAnswer must match one value from options exactly.
        - Options must be plausible and non-empty.
        - Use Ukrainian unless the user asks for another language.
        """;

    private const string FlashcardResponseSchema = """
        {
          "cards": [
            {
              "front": "string",
              "back": "string"
            }
          ]
        }

        Rules:
        - Return one JSON object only.
        - front is the term, question, or concept.
        - back is the short answer or definition.
        - Use Ukrainian unless the user asks for another language.
        """;

    public async Task<GenerateLearningContentResponse> GenerateAsync(
        GenerateLearningContentRequest request,
        CancellationToken cancellationToken)
    {
        var type = NormalizeType(request.Type);
        Validate(request, type);

        var context = await BuildContextAsync(request, cancellationToken);

        return type switch
        {
            TestType => new GenerateLearningContentResponse(
                TestType,
                await GenerateTestAsync(request, context, cancellationToken)),
            FlashcardType => new GenerateLearningContentResponse(
                FlashcardType,
                await GenerateFlashcardsAsync(request, context, cancellationToken)),
            _ => throw new ValidationException("type must be either 'test' or 'flashcard'.")
        };
    }

    public async Task<GenerateLearningContentChatResponse> GenerateForChatAsync(
        GenerateLearningContentChatRequest request,
        CancellationToken cancellationToken)
    {
        if (!request.ExplicitGeneration)
        {
            throw new ValidationException("explicitGeneration must be true for chat learning-content generation.");
        }

        var type = NormalizeType(request.Type);
        if (string.IsNullOrWhiteSpace(request.ChatId))
        {
            throw new ValidationException("chatId is required.");
        }

        if (!Guid.TryParse(request.ChatId, out var chatId))
        {
            throw new ValidationException("chatId must be a valid guid.");
        }

        var chat = await chatSessionService.GetByIdAsync(chatId, cancellationToken);
        var count = request.Count is > 0
            ? request.Count.Value
            : type == TestType ? 5 : 8;

        Validate(new GenerateLearningContentRequest(
            type,
            request.Message,
            count,
            request.LectureId ?? chat.LectureId,
            request.Context), type);

        var nextSequenceNumber = await chatMessageService.GetNextSequenceNumberAsync(
            chatId,
            cancellationToken);

        var userMessage = await chatMessageService.CreateAsync(new ChatMessage
        {
            ChatSessionId = chatId,
            Content = request.Message!.Trim(),
            Timestamp = DateTime.UtcNow,
            Role = MessageRole.User,
            SequenceNumber = nextSequenceNumber,
            Status = "completed"
        }, cancellationToken);

        try
        {
            var generationRequest = new GenerateLearningContentRequest(
                type,
                request.Message,
                count,
                request.LectureId ?? chat.LectureId,
                request.Context);
            var generated = await GenerateAsync(generationRequest, cancellationToken);
            var createdContent = await PersistGeneratedContentAsync(
                generated,
                userMessage.Id,
                request.Message!,
                cancellationToken);

            var systemMessage = await CreateSystemMessageAsync(
                chatId,
                nextSequenceNumber + 1,
                BuildSuccessMessage(type, createdContent),
                "completed",
                cancellationToken);

            return new GenerateLearningContentChatResponse(
                "completed",
                type,
                ToDto(userMessage),
                ToDto(systemMessage),
                createdContent);
        }
        catch (Exception ex)
        {
            dbContext.ChangeTracker.Clear();

            var systemMessage = await CreateSystemMessageAsync(
                chatId,
                nextSequenceNumber + 1,
                BuildFailureMessage(type, ex),
                "failed",
                cancellationToken);

            return new GenerateLearningContentChatResponse(
                "failed",
                type,
                ToDto(userMessage),
                ToDto(systemMessage),
                null);
        }
    }

    private async Task<GeneratedTestDto> GenerateTestAsync(
        GenerateLearningContentRequest request,
        string context,
        CancellationToken cancellationToken)
    {
        return await aiGenerationService.GenerateStructuredAsync<GeneratedTestDto>(
            new AiGenerationRequest
            {
                TaskType = AiTaskType.TestGeneration,
                UserMessage = BuildUserMessage(request, "questions"),
                Context = context,
                OutputFormat = AiOutputFormat.Json,
                ResponseSchema = TestResponseSchema
            },
            cancellationToken);
    }

    private async Task<GeneratedFlashcardsDto> GenerateFlashcardsAsync(
        GenerateLearningContentRequest request,
        string context,
        CancellationToken cancellationToken)
    {
        return await aiGenerationService.GenerateStructuredAsync<GeneratedFlashcardsDto>(
            new AiGenerationRequest
            {
                TaskType = AiTaskType.FlashcardGeneration,
                UserMessage = BuildUserMessage(request, "flashcards"),
                Context = context,
                OutputFormat = AiOutputFormat.Json,
                ResponseSchema = FlashcardResponseSchema
            },
            cancellationToken);
    }

    private async Task<string> BuildContextAsync(
        GenerateLearningContentRequest request,
        CancellationToken cancellationToken)
    {
        var contextParts = new List<string>();

        if (!string.IsNullOrWhiteSpace(request.Context))
        {
            contextParts.Add(request.Context.Trim());
        }

        if (request.LectureId is { } lectureId)
        {
            var lecture = await lectureService.GetByIdAsync(lectureId, cancellationToken);
            contextParts.Add($"""
                Lecture:
                Id: {lecture.Id}
                Name: {lecture.Name}
                SubjectId: {lecture.SubjectId}
                """);
        }

        return string.Join(Environment.NewLine + Environment.NewLine, contextParts);
    }

    private static string BuildUserMessage(
        GenerateLearningContentRequest request,
        string itemName)
    {
        return $"""
            {request.Message!.Trim()}

            Generate exactly {request.Count!.Value} {itemName}.
            """;
    }

    private async Task<object> PersistGeneratedContentAsync(
        GenerateLearningContentResponse generated,
        Guid userMessageId,
        string sourceMessage,
        CancellationToken cancellationToken)
    {
        return generated.Type switch
        {
            FlashcardType => await PersistFlashcardsAsync(
                (GeneratedFlashcardsDto)generated.GeneratedContent,
                userMessageId,
                sourceMessage,
                cancellationToken),
            TestType => await PersistTestAsync(
                (GeneratedTestDto)generated.GeneratedContent,
                userMessageId,
                sourceMessage,
                cancellationToken),
            _ => throw new ValidationException("Unsupported generated content type.")
        };
    }

    private async Task<FlashcardResponse> PersistFlashcardsAsync(
        GeneratedFlashcardsDto generated,
        Guid userMessageId,
        string sourceMessage,
        CancellationToken cancellationToken)
    {
        var flashcard = await flashcardService.CreateAsync(new Flashcard
        {
            Name = BuildExerciseName("Картки", sourceMessage),
            ChatMessageId = userMessageId,
            Cards = generated.Cards!
                .Select(card => new Card
                {
                    Term = card.Front!.Trim(),
                    Definition = card.Back!.Trim()
                })
                .ToList()
        }, cancellationToken);

        return new FlashcardResponse(
            flashcard.Id,
            flashcard.Name,
            flashcard.ChatMessageId,
            flashcard.Cards
                .Select(card => new CardResponse(card.Id, card.Term, card.Definition))
                .ToList());
    }

    private async Task<TestResponse> PersistTestAsync(
        GeneratedTestDto generated,
        Guid userMessageId,
        string sourceMessage,
        CancellationToken cancellationToken)
    {
        var test = await testService.CreateAsync(new StudyMentorApi.Data.Models.Test
        {
            Name = string.IsNullOrWhiteSpace(generated.Title)
                ? BuildExerciseName("Тест", sourceMessage)
                : generated.Title.Trim(),
            ChatMessageId = userMessageId,
            Questions = generated.Questions!
                .Select((question, questionIndex) => new TestQuestion
                {
                    Prompt = question.Text!.Trim(),
                    Order = questionIndex,
                    AnswerVariants = BuildAnswerVariants(question)
                })
                .ToList()
        }, cancellationToken);

        return new TestResponse(
            test.Id,
            test.Name,
            test.ChatMessageId,
            test.SourceFlashcardId,
            test.Questions
                .OrderBy(question => question.Order)
                .Select(question => new TestQuestionResponse(
                    question.Id,
                    question.Prompt,
                    question.AnswerVariants
                        .OrderBy(answer => answer.Order)
                        .Select(answer => new TestAnswerVariantResponse(answer.Id, answer.Text))
                        .ToList()))
                .ToList());
    }

    private static List<TestAnswerVariant> BuildAnswerVariants(GeneratedQuestionDto question)
    {
        var correctAnswer = question.CorrectAnswer!.Trim();
        var options = (question.Options ?? [])
            .Select(option => option.Trim())
            .Where(option => !string.IsNullOrWhiteSpace(option))
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .ToList();

        if (!options.Any(option => string.Equals(option, correctAnswer, StringComparison.OrdinalIgnoreCase)))
        {
            options.Insert(0, correctAnswer);
        }

        while (options.Count < 2)
        {
            options.Add($"Варіант {options.Count + 1}");
        }

        return options
            .Select((option, index) => new TestAnswerVariant
            {
                Text = option,
                IsCorrect = string.Equals(option, correctAnswer, StringComparison.OrdinalIgnoreCase),
                Order = index
            })
            .ToList();
    }

    private async Task<ChatMessage> CreateSystemMessageAsync(
        Guid chatId,
        int sequenceNumber,
        string content,
        string status,
        CancellationToken cancellationToken)
    {
        return await chatMessageService.CreateAsync(new ChatMessage
        {
            ChatSessionId = chatId,
            Content = content,
            Timestamp = DateTime.UtcNow,
            Role = MessageRole.System,
            SequenceNumber = sequenceNumber,
            Status = status
        }, cancellationToken);
    }

    private static string BuildExerciseName(string prefix, string sourceMessage)
    {
        var normalized = sourceMessage.Trim();
        var title = normalized.Length > 48 ? normalized[..48].Trim() + "..." : normalized;
        return $"{prefix}: {title}";
    }

    private static string BuildSuccessMessage(string type, object createdContent)
    {
        return type == FlashcardType && createdContent is FlashcardResponse flashcard
            ? $"Готово: створено набір карток \"{flashcard.Name}\" ({flashcard.Cards.Count}). Відкрийте \"Практичні завдання\" в боковому меню."
            : createdContent is TestResponse test
                ? $"Готово: створено тест \"{test.Name}\" ({test.Questions.Count} питань). Відкрийте \"Практичні завдання\" в боковому меню."
                : "Готово: завдання створено. Відкрийте \"Практичні завдання\" в боковому меню.";
    }

    private static string BuildFailureMessage(string type, Exception exception)
    {
        var name = type == FlashcardType ? "карток" : "тесту";
        var message = string.IsNullOrWhiteSpace(exception.Message)
            ? "невідома помилка"
            : exception.Message;

        return $"Не вдалося виконати генерацію {name}: {message}";
    }

    private static AiChatMessageDto ToDto(ChatMessage message)
    {
        return new AiChatMessageDto(
            message.Id.ToString(),
            message.ChatSessionId.ToString(),
            message.Role.ToString().ToLowerInvariant(),
            message.Content,
            message.Timestamp,
            message.Status);
    }

    private static void Validate(GenerateLearningContentRequest request, string type)
    {
        if (string.IsNullOrWhiteSpace(type))
        {
            throw new ValidationException("type is required.");
        }

        if (type is not TestType and not FlashcardType)
        {
            throw new ValidationException("type must be either 'test' or 'flashcard'.");
        }

        if (string.IsNullOrWhiteSpace(request.Message))
        {
            throw new ValidationException("message is required.");
        }

        if (request.Count is null or <= 0)
        {
            throw new ValidationException("count must be greater than zero.");
        }
    }

    private static string NormalizeType(string? type)
    {
        var normalized = type?.Trim().ToLowerInvariant() ?? string.Empty;
        return normalized is "flashcards" or "card" or "cards"
            ? FlashcardType
            : normalized;
    }
}
