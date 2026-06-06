using StudyMentorApi.ChatMessages;
using StudyMentorApi.ChatSessions;
using StudyMentorApi.Common;
using StudyMentorApi.Data.Models;
using StudyMentorApi.Services.Ai;
using StudyMentorApi.Users;

namespace StudyMentorApi.AiChat;

public class AiChatService(
    ChatSessionService chatSessionService,
    ChatMessageService chatMessageService,
    UserService userService,
    IAiGenerationService aiGenerationService)
{
    private const int MaxHistoryMessagesForAi = 20;

    public async Task<IReadOnlyCollection<AiChatMessageDto>> GetMessagesAsync(
        string chatId,
        CancellationToken cancellationToken)
    {
        var parsedChatId = ParseChatId(chatId);
        var chatExists = await chatSessionService.ExistsAsync(parsedChatId, cancellationToken);
        if (!chatExists)
        {
            throw new NotFoundException($"Chat with id '{chatId}' was not found.");
        }

        var messages = await chatMessageService.GetMessagesAsync(parsedChatId, cancellationToken);

        return messages.Select(ToDto).ToList();
    }

    public async Task<AiChatSendMessageResponse> SendMessageAsync(
        AiChatSendMessageRequest request,
        CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(request.ChatId))
        {
            throw new ValidationException("chatId is required.");
        }

        if (string.IsNullOrWhiteSpace(request.Content))
        {
            throw new ValidationException("content is required.");
        }

        var parsedChatId = ParseChatId(request.ChatId);
        var chat = await chatSessionService.GetByIdAsync(parsedChatId, cancellationToken);

        var nextSequenceNumber = await chatMessageService.GetNextSequenceNumberAsync(
            parsedChatId,
            cancellationToken);

        var userMessage = await chatMessageService.CreateAsync(new ChatMessage
        {
            ChatSessionId = parsedChatId,
            Content = request.Content.Trim(),
            Timestamp = DateTime.UtcNow,
            Role = MessageRole.User,
            SequenceNumber = nextSequenceNumber,
            Status = "completed"
        }, cancellationToken);

        try
        {
            var history = await chatMessageService.GetMessagesAsync(parsedChatId, cancellationToken);
            var recentHistory = history.TakeLast(MaxHistoryMessagesForAi).ToList();
            var user = await userService.GetByIdAsync(chat.UserId, cancellationToken);

            var aiResponse = await aiGenerationService.GenerateAsync(new AiGenerationRequest
            {
                TaskType = AiTaskType.ChatAnswer,
                UserMessage = request.Content.Trim(),
                ConversationHistory = ToAiChatMessages(recentHistory),
                UserProfile = BuildUserProfile(chat, user),
                OutputFormat = AiOutputFormat.Text
            }, cancellationToken);

            var assistantMessage = await chatMessageService.CreateAsync(new ChatMessage
            {
                ChatSessionId = parsedChatId,
                Content = aiResponse.Content,
                Timestamp = DateTime.UtcNow,
                Role = MessageRole.Assistant,
                SequenceNumber = nextSequenceNumber + 1,
                Status = "completed"
            }, cancellationToken);

            return new AiChatSendMessageResponse(
                "completed",
                ToDto(userMessage),
                ToDto(assistantMessage));
        }
        catch (Exception ex)
        {
            await chatMessageService.CreateAsync(new ChatMessage
            {
                ChatSessionId = parsedChatId,
                Content = string.IsNullOrWhiteSpace(ex.Message)
                    ? "AI service did not return a response"
                    : ex.Message,
                Timestamp = DateTime.UtcNow,
                Role = MessageRole.Assistant,
                SequenceNumber = nextSequenceNumber + 1,
                Status = "failed"
            }, cancellationToken);

            throw new AiChatFailedException(
                string.IsNullOrWhiteSpace(ex.Message)
                    ? "AI service did not return a response"
                    : ex.Message);
        }
    }

    private static AiChatMessageDto ToDto(ChatMessage message) =>
        new(
            message.Id.ToString(),
            message.ChatSessionId.ToString(),
            message.Role.ToString().ToLowerInvariant(),
            message.Content,
            message.Timestamp,
            message.Status);

    private static Guid ParseChatId(string chatId)
    {
        if (!Guid.TryParse(chatId, out var parsedChatId))
        {
            throw new ValidationException("chatId must be a valid guid.");
        }

        return parsedChatId;
    }

    private static IReadOnlyCollection<AiChatMessage> ToAiChatMessages(IEnumerable<ChatMessage> messages)
    {
        return messages
            .Select(message => new AiChatMessage(
                message.Role.ToString().ToLowerInvariant(),
                message.Content))
            .ToList();
    }

    private static string BuildUserProfile(ChatSession chat, User user)
    {
        return $"""
            User:
            UserId: {chat.UserId}
            Name: {user.Name}
            GroupId: {user.GroupId}

            Chat:
            ChatId: {chat.Id}
            LectureId: {chat.LectureId}
            """;
    }
}
