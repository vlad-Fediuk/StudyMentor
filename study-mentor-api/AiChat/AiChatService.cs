using StudyMentorApi.ChatMessages;
using StudyMentorApi.ChatSessions;
using StudyMentorApi.Common;
using StudyMentorApi.Data.Models;
using StudyMentorApi.Services.Ai;

namespace StudyMentorApi.AiChat;

public class AiChatService(
    ChatSessionService chatSessionService,
    ChatMessageService chatMessageService,
    IAiChatService aiChatService)
{
    public async Task<IReadOnlyCollection<AiChatMessageDto>> GetMessagesAsync(
        string chatId,
        CancellationToken cancellationToken)
    {
        var chatExists = await chatSessionService.ExistsAsync(chatId, cancellationToken);
        if (!chatExists)
        {
            throw new NotFoundException($"Chat with id '{chatId}' was not found.");
        }

        var messages = await chatMessageService.GetMessagesAsync(chatId, cancellationToken);

        return messages
            .Select(message => new AiChatMessageDto(
                message.Id,
                message.ChatSessionId,
                message.Role.ToString().ToLowerInvariant(),
                message.Content,
                message.Timestamp,
                message.Status))
            .ToList();
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

        var chatExists = await chatSessionService.ExistsAsync(request.ChatId, cancellationToken);
        if (!chatExists)
        {
            throw new NotFoundException($"Chat with id '{request.ChatId}' was not found.");
        }

        var nextSequenceNumber = await chatMessageService.GetNextSequenceNumberAsync(
            request.ChatId,
            cancellationToken);

        var userMessage = await chatMessageService.CreateAsync(new ChatMessage
        {
            ChatSessionId = request.ChatId,
            Content = request.Content.Trim(),
            Timestamp = DateTime.UtcNow,
            Role = MessageRole.User,
            SequenceNumber = nextSequenceNumber,
            Status = "completed"
        }, cancellationToken);

        try
        {
            var history = await chatMessageService.GetMessagesAsync(request.ChatId, cancellationToken);
            var aiResponse = await aiChatService.CompleteAsync(new AiChatRequest
            {
                Messages = history
                    .Select(message => new AiChatMessage(
                        message.Role.ToString().ToLowerInvariant(),
                        message.Content))
                    .ToList()
            }, cancellationToken);

            var assistantMessage = await chatMessageService.CreateAsync(new ChatMessage
            {
                ChatSessionId = request.ChatId,
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
                ChatSessionId = request.ChatId,
                Content = ex.Message,
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
            message.Id,
            message.ChatSessionId,
            message.Role.ToString().ToLowerInvariant(),
            message.Content,
            message.Timestamp,
            message.Status);
}
