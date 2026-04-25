namespace StudyMentorApi.DTOs.Ai;

public sealed record AiChatResponseDto(
    string Provider,
    string Model,
    string Content);
