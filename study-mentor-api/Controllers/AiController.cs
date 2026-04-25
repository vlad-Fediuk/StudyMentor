using Microsoft.AspNetCore.Mvc;
using StudyMentorApi.DTOs.Ai;
using StudyMentorApi.Services.Ai;

namespace StudyMentorApi.Controllers;

[ApiController]
[Route("api/ai")]
public sealed class AiController : ControllerBase
{
    private readonly IAiChatService _aiChatService;

    public AiController(IAiChatService aiChatService)
    {
        _aiChatService = aiChatService;
    }

    [HttpPost("chat")]
    public async Task<ActionResult<AiChatResponseDto>> CompleteChatAsync(
        AiChatRequestDto request,
        CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(request.Message))
        {
            return BadRequest("Message is required.");
        }

        var messages = new List<AiChatMessage>();

        if (!string.IsNullOrWhiteSpace(request.SystemPrompt))
        {
            messages.Add(new AiChatMessage("system", request.SystemPrompt));
        }

        messages.Add(new AiChatMessage("user", request.Message));

        var response = await _aiChatService.CompleteAsync(
            new AiChatRequest
            {
                Model = request.Model,
                Messages = messages
            },
            cancellationToken);

        return Ok(new AiChatResponseDto(
            response.Provider,
            response.Model,
            response.Content));
    }
}
