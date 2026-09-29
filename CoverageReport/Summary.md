# Summary

|||
|:---|:---|
| Generated on: | 29.09.2026 - 17:12:49 |
| Coverage date: | 29.09.2026 - 16:14:09 - 29.09.2026 - 17:12:48 |
| Parser: | MultiReport (4x Cobertura) |
| Assemblies: | 1 |
| Classes: | 146 |
| Files: | 142 |
| **Line coverage:** | 14.7% (1305 of 8845) |
| Covered lines: | 1305 |
| Uncovered lines: | 7540 |
| Coverable lines: | 8845 |
| Total lines: | 12173 |
| **Branch coverage:** | 11.2% (72 of 640) |
| Covered branches: | 72 |
| Total branches: | 640 |
| **Method coverage:** | [Feature is only available for sponsors](https://reportgenerator.io/pro) |

# Risk Hotspots

| **Assembly** | **Class** | **Method** | **Crap Score** | **Cyclomatic complexity** |
|:---|:---|:---|---:|---:|
| study-mentor-api | Microsoft.AspNetCore.OpenApi.Generated | TransformAsync(...) | 8930 | 94 || study-mentor-api | Microsoft.AspNetCore.OpenApi.Generated | TransformAsync(...) | 1190 | 34 || study-mentor-api | Microsoft.AspNetCore.OpenApi.Generated | GetTypeDocId(...) | 812 | 28 || study-mentor-api | StudyMentorApi.Services.Ai.Embeddings.LmStudioEmbeddingService | CreateEmbeddingAsync() | 600 | 24 || study-mentor-api | StudyMentorApi.Services.Ai.NvidiaProviderClient | CompleteAsync() | 420 | 20 || study-mentor-api | Microsoft.AspNetCore.OpenApi.Generated | CreateDocumentationId(...) | 342 | 18 || study-mentor-api | StudyMentorApi.LearningContent.LearningContentGenerationService | GenerateForChatAsync() | 272 | 16 || study-mentor-api | StudyMentorApi.Tests.TestEndpoints | ToEntity(...) | 272 | 16 || study-mentor-api | StudyMentorApi.Tests.TestService | ValidateAsync() | 272 | 16 || study-mentor-api | StudyMentorApi.Flashcards.FlashcardEndpoints | ToEntity(...) | 210 | 14 || study-mentor-api | StudyMentorApi.LearningContent.LearningContentGenerationService | Validate(...) | 210 | 14 || study-mentor-api | StudyMentorApi.Services.Ai.LmStudioProviderClient | CompleteAsync() | 210 | 14 || study-mentor-api | StudyMentorApi.Authentication.Jwt.JwtAuthenticationService | ExtractAndValidateEmail(...) | 156 | 12 || study-mentor-api | StudyMentorApi.LearningContent.LearningContentGenerationService | NormalizeType(...) | 156 | 12 || study-mentor-api | Microsoft.AspNetCore.OpenApi.Generated | CreateDocumentationId(...) | 110 | 10 || study-mentor-api | StudyMentorApi.Authentication.Jwt.JwtAuthenticationService | GetOrCreateUserByEmailAsync() | 110 | 10 || study-mentor-api | StudyMentorApi.LectureChunks.LectureChunkRetrievalService | FormatContext(...) | 110 | 10 || study-mentor-api | StudyMentorApi.LectureChunks.LectureChunkService | SplitIntoChunks(...) | 110 | 10 || study-mentor-api | StudyMentorApi.Authentication.Jwt.JwtTokenService | CreateTokenAsync() | 72 | 8 || study-mentor-api | StudyMentorApi.LectureChunks.LectureChunkService | CreateFromFileAsync() | 72 | 8 || study-mentor-api | Microsoft.AspNetCore.OpenApi.Generated | NormalizeDocId(...) | 42 | 6 || study-mentor-api | Microsoft.AspNetCore.OpenApi.Generated | UnwrapOpenApiParameter(...) | 42 | 6 || study-mentor-api | StudyMentorApi.Diagnostics.AiSmokeTestRunner | RunAsync() | 42 | 6 || study-mentor-api | StudyMentorApi.Flashcards.FlashcardService | ValidateAsync() | 42 | 6 || study-mentor-api | StudyMentorApi.LearningContent.LearningContentGenerationService | BuildAnswerVariants(...) | 42 | 6 || study-mentor-api | StudyMentorApi.LearningContent.LearningContentGenerationService | BuildSuccessMessage(...) | 42 | 6 || study-mentor-api | StudyMentorApi.LearningContent.LearningContentGenerationService | BuildContextAsync() | 42 | 6 || study-mentor-api | StudyMentorApi.LectureChunks.LectureChunkService | ValidateFile(...) | 42 | 6 || study-mentor-api | StudyMentorApi.Services.Ai.Prompts.PromptTemplateProvider | GetActiveTemplateAsync() | 42 | 6 || study-mentor-api | StudyMentorApi.Tests.TestService | CheckAnswerAsync() | 42 | 6 || study-mentor-api | StudyMentorApi.Services.Ai.AiModelRouter | CompleteAsync() | 34 | 12 |
# Coverage

| **Name** | **Covered** | **Uncovered** | **Coverable** | **Total** | **Line coverage** | **Covered** | **Total** | **Branch coverage** |
|:---|---:|---:|---:|---:|---:|---:|---:|---:|
| **study-mentor-api** | **1305** | **7540** | **8845** | **12571** | **14.7%** | **72** | **640** | **11.2%** |
| Microsoft.AspNetCore.OpenApi.Generated | 4 | 377 | 381 | 606 | 1% | 0 | 206 | 0% |
| StudyMentorApi.AiChat.AiChatEndpoints | 27 | 14 | 41 | 72 | 65.8% | 2 | 2 | 100% |
| StudyMentorApi.AiChat.AiChatErrorResponse | 0 | 3 | 3 | 5 | 0% | 0 | 0 |  |
| StudyMentorApi.AiChat.AiChatFailedException | 0 | 1 | 1 | 3 | 0% | 0 | 0 |  |
| StudyMentorApi.AiChat.AiChatMessageDto | 7 | 0 | 7 | 9 | 100% | 0 | 0 |  |
| StudyMentorApi.AiChat.AiChatSendMessageRequest | 3 | 0 | 3 | 5 | 100% | 0 | 0 |  |
| StudyMentorApi.AiChat.AiChatSendMessageResponse | 4 | 0 | 4 | 6 | 100% | 0 | 0 |  |
| StudyMentorApi.AiChat.AiChatService | 98 | 35 | 133 | 185 | 73.6% | 7 | 14 | 50% |
| StudyMentorApi.Authentication.AuthenticationEndpoints | 6 | 13 | 19 | 34 | 31.5% | 0 | 0 |  |
| StudyMentorApi.Authentication.AuthenticationException | 0 | 1 | 1 | 4 | 0% | 0 | 0 |  |
| StudyMentorApi.Authentication.Jwt.JwtAuthenticationRequest | 0 | 1 | 1 | 6 | 0% | 0 | 0 |  |
| StudyMentorApi.Authentication.Jwt.JwtAuthenticationService | 0 | 49 | 49 | 77 | 0% | 0 | 26 | 0% |
| StudyMentorApi.Authentication.Jwt.JwtAuthenticationValidator | 0 | 25 | 25 | 44 | 0% | 0 | 4 | 0% |
| StudyMentorApi.Authentication.Jwt.JwtTokenService | 0 | 47 | 47 | 72 | 0% | 0 | 14 | 0% |
| StudyMentorApi.ChatMessages.ChatMessageEndpoints | 12 | 83 | 95 | 162 | 12.6% | 0 | 2 | 0% |
| StudyMentorApi.ChatMessages.ChatMessageRequest | 0 | 6 | 6 | 10 | 0% | 0 | 0 |  |
| StudyMentorApi.ChatMessages.ChatMessageResponse | 0 | 8 | 8 | 12 | 0% | 0 | 0 |  |
| StudyMentorApi.ChatMessages.ChatMessageService | 19 | 24 | 43 | 76 | 44.1% | 2 | 2 | 100% |
| StudyMentorApi.ChatSessions.ChatSessionEndpoints | 27 | 19 | 46 | 87 | 58.6% | 0 | 0 |  |
| StudyMentorApi.ChatSessions.ChatSessionRequest | 3 | 0 | 3 | 6 | 100% | 0 | 0 |  |
| StudyMentorApi.ChatSessions.ChatSessionResponse | 4 | 0 | 4 | 7 | 100% | 0 | 0 |  |
| StudyMentorApi.ChatSessions.ChatSessionService | 19 | 12 | 31 | 55 | 61.2% | 0 | 0 |  |
| StudyMentorApi.Common.NotFoundException | 1 | 0 | 1 | 5 | 100% | 0 | 0 |  |
| StudyMentorApi.Common.ValidationException | 1 | 0 | 1 | 5 | 100% | 0 | 0 |  |
| StudyMentorApi.Data.AppDbContext | 356 | 26 | 382 | 437 | 93.1% | 3 | 4 | 75% |
| StudyMentorApi.Data.Models.AiModel | 11 | 2 | 13 | 30 | 84.6% | 0 | 0 |  |
| StudyMentorApi.Data.Models.AiProvider | 8 | 1 | 9 | 22 | 88.8% | 0 | 0 |  |
| StudyMentorApi.Data.Models.BaseEntity | 1 | 0 | 1 | 6 | 100% | 0 | 0 |  |
| StudyMentorApi.Data.Models.Card | 0 | 4 | 4 | 12 | 0% | 0 | 0 |  |
| StudyMentorApi.Data.Models.ChatMessage | 8 | 0 | 8 | 27 | 100% | 0 | 0 |  |
| StudyMentorApi.Data.Models.ChatSession | 5 | 0 | 5 | 14 | 100% | 0 | 0 |  |
| StudyMentorApi.Data.Models.Exercise | 0 | 3 | 3 | 10 | 0% | 0 | 0 |  |
| StudyMentorApi.Data.Models.Flashcard | 0 | 1 | 1 | 6 | 0% | 0 | 0 |  |
| StudyMentorApi.Data.Models.Group | 0 | 1 | 1 | 6 | 0% | 0 | 0 |  |
| StudyMentorApi.Data.Models.Lecture | 4 | 1 | 5 | 14 | 80% | 0 | 0 |  |
| StudyMentorApi.Data.Models.LectureChunk | 0 | 7 | 7 | 20 | 0% | 0 | 0 |  |
| StudyMentorApi.Data.Models.Major | 2 | 0 | 2 | 8 | 100% | 0 | 0 |  |
| StudyMentorApi.Data.Models.PromptTemplate | 0 | 7 | 7 | 20 | 0% | 0 | 0 |  |
| StudyMentorApi.Data.Models.Subject | 4 | 0 | 4 | 12 | 100% | 0 | 0 |  |
| StudyMentorApi.Data.Models.Test | 0 | 3 | 3 | 10 | 0% | 0 | 0 |  |
| StudyMentorApi.Data.Models.TestAnswerVariant | 0 | 5 | 5 | 14 | 0% | 0 | 0 |  |
| StudyMentorApi.Data.Models.TestQuestion | 0 | 5 | 5 | 14 | 0% | 0 | 0 |  |
| StudyMentorApi.Data.Models.User | 6 | 0 | 6 | 16 | 100% | 0 | 0 |  |
| StudyMentorApi.Diagnostics.AiSmokeTestRunner | 0 | 25 | 25 | 40 | 0% | 0 | 6 | 0% |
| StudyMentorApi.Extensions.ExceptionHandlerExtensions | 24 | 2 | 26 | 35 | 92.3% | 0 | 0 |  |
| StudyMentorApi.Extensions.JwtAuthenticationExtensions | 37 | 0 | 37 | 55 | 100% | 1 | 2 | 50% |
| StudyMentorApi.Extensions.ServiceExtensions | 39 | 0 | 39 | 56 | 100% | 0 | 0 |  |
| StudyMentorApi.Flashcards.CardRequest | 0 | 3 | 3 | 5 | 0% | 0 | 0 |  |
| StudyMentorApi.Flashcards.CardResponse | 0 | 4 | 4 | 6 | 0% | 0 | 0 |  |
| StudyMentorApi.Flashcards.FlashcardEndpoints | 10 | 68 | 78 | 131 | 12.8% | 0 | 14 | 0% |
| StudyMentorApi.Flashcards.FlashcardRequest | 0 | 4 | 4 | 6 | 0% | 0 | 0 |  |
| StudyMentorApi.Flashcards.FlashcardResponse | 0 | 5 | 5 | 7 | 0% | 0 | 0 |  |
| StudyMentorApi.Flashcards.FlashcardService | 0 | 53 | 53 | 100 | 0% | 0 | 10 | 0% |
| StudyMentorApi.Groups.GroupEndpoints | 10 | 23 | 33 | 69 | 30.3% | 0 | 0 |  |
| StudyMentorApi.Groups.GroupRequest | 0 | 1 | 1 | 3 | 0% | 0 | 0 |  |
| StudyMentorApi.Groups.GroupResponse | 0 | 1 | 1 | 3 | 0% | 0 | 0 |  |
| StudyMentorApi.Groups.GroupService | 0 | 23 | 23 | 43 | 0% | 0 | 0 |  |
| StudyMentorApi.LearningContent.GenerateLearningContentChatRequest | 0 | 8 | 8 | 10 | 0% | 0 | 0 |  |
| StudyMentorApi.LearningContent.GenerateLearningContentChatResponse | 0 | 6 | 6 | 10 | 0% | 0 | 0 |  |
| StudyMentorApi.LearningContent.GenerateLearningContentRequest | 0 | 6 | 6 | 8 | 0% | 0 | 0 |  |
| StudyMentorApi.LearningContent.GenerateLearningContentResponse | 0 | 3 | 3 | 5 | 0% | 0 | 0 |  |
| StudyMentorApi.LearningContent.LearningContentEndpoints | 8 | 32 | 40 | 71 | 20% | 0 | 0 |  |
| StudyMentorApi.LearningContent.LearningContentGenerationService | 0 | 298 | 298 | 455 | 0% | 0 | 76 | 0% |
| StudyMentorApi.LectureChunks.LectureChunkEndpoints | 9 | 47 | 56 | 104 | 16% | 0 | 2 | 0% |
| StudyMentorApi.LectureChunks.LectureChunkRequest | 0 | 1 | 1 | 3 | 0% | 0 | 0 |  |
| StudyMentorApi.LectureChunks.LectureChunkResponse | 0 | 1 | 1 | 3 | 0% | 0 | 0 |  |
| StudyMentorApi.LectureChunks.LectureChunkRetrievalService | 5 | 61 | 66 | 101 | 7.5% | 0 | 12 | 0% |
| StudyMentorApi.LectureChunks.LectureChunkService | 0 | 94 | 94 | 145 | 0% | 0 | 26 | 0% |
| StudyMentorApi.Lectures.LectureEndpoints | 10 | 76 | 86 | 163 | 11.6% | 0 | 0 |  |
| StudyMentorApi.Lectures.LectureRequest | 0 | 1 | 1 | 3 | 0% | 0 | 0 |  |
| StudyMentorApi.Lectures.LectureResponse | 0 | 1 | 1 | 5 | 0% | 0 | 0 |  |
| StudyMentorApi.Lectures.LectureService | 0 | 34 | 34 | 67 | 0% | 0 | 0 |  |
| StudyMentorApi.Majors.MajorEndpoints | 11 | 22 | 33 | 67 | 33.3% | 0 | 0 |  |
| StudyMentorApi.Majors.MajorRequest | 0 | 1 | 1 | 3 | 0% | 0 | 0 |  |
| StudyMentorApi.Majors.MajorResponse | 0 | 1 | 1 | 3 | 0% | 0 | 0 |  |
| StudyMentorApi.Majors.MajorService | 0 | 23 | 23 | 43 | 0% | 0 | 0 |  |
| StudyMentorApi.Migrations.AddAiProviderRegistry | 0 | 506 | 506 | 578 | 0% | 0 | 0 |  |
| StudyMentorApi.Migrations.AddChatSessions | 0 | 305 | 305 | 372 | 0% | 0 | 0 |  |
| StudyMentorApi.Migrations.AddFlashcards | 0 | 349 | 349 | 416 | 0% | 0 | 0 |  |
| StudyMentorApi.Migrations.AddGroups | 0 | 274 | 274 | 334 | 0% | 0 | 0 |  |
| StudyMentorApi.Migrations.AddLectureChunkEmbeddings | 0 | 706 | 706 | 788 | 0% | 0 | 0 |  |
| StudyMentorApi.Migrations.AddLectureChunks | 0 | 29 | 29 | 51 | 0% | 0 | 0 |  |
| StudyMentorApi.Migrations.AddPromptTemplates | 0 | 538 | 538 | 607 | 0% | 0 | 0 |  |
| StudyMentorApi.Migrations.AddTests | 0 | 487 | 487 | 569 | 0% | 0 | 0 |  |
| StudyMentorApi.Migrations.AddUserAuthFields | 0 | 21 | 21 | 39 | 0% | 0 | 0 |  |
| StudyMentorApi.Migrations.AppDbContextModelSnapshot | 0 | 752 | 752 | 812 | 0% | 0 | 0 |  |
| StudyMentorApi.Migrations.HardenAiPromptTemplates | 0 | 39 | 39 | 81 | 0% | 0 | 0 |  |
| StudyMentorApi.Migrations.InitialPostgresCreate | 0 | 296 | 296 | 363 | 0% | 0 | 0 |  |
| StudyMentorApi.Migrations.RemoveDebugStudyRecords | 0 | 7 | 7 | 23 | 0% | 0 | 0 |  |
| StudyMentorApi.Migrations.RemoveDefaultStudyRecords | 0 | 7 | 7 | 23 | 0% | 0 | 0 |  |
| StudyMentorApi.Migrations.ScopeChatPromptToLecture | 0 | 17 | 17 | 44 | 0% | 0 | 0 |  |
| StudyMentorApi.Migrations.SeedStudySubjectsAndLectures | 0 | 587 | 587 | 665 | 0% | 0 | 0 |  |
| StudyMentorApi.Migrations.UpdateGenerationPromptTemplates | 0 | 31 | 31 | 68 | 0% | 0 | 0 |  |
| StudyMentorApi.Program | 56 | 0 | 56 | 86 | 100% | 2 | 2 | 100% |
| StudyMentorApi.Services.Ai.AiChatMessage | 2 | 1 | 3 | 21 | 66.6% | 0 | 0 |  |
| StudyMentorApi.Services.Ai.AiChatRequest | 4 | 0 | 4 | 21 | 100% | 0 | 0 |  |
| StudyMentorApi.Services.Ai.AiChatResponse | 3 | 2 | 5 | 21 | 60% | 0 | 0 |  |
| StudyMentorApi.Services.Ai.AiGenerationFailedException | 0 | 1 | 1 | 3 | 0% | 0 | 0 |  |
| StudyMentorApi.Services.Ai.AiGenerationOptions | 0 | 6 | 6 | 53 | 0% | 0 | 0 |  |
| StudyMentorApi.Services.Ai.AiGenerationRequest | 6 | 4 | 10 | 53 | 60% | 0 | 0 |  |
| StudyMentorApi.Services.Ai.AiGenerationResponse | 3 | 3 | 6 | 53 | 50% | 0 | 0 |  |
| StudyMentorApi.Services.Ai.AiGenerationService | 4 | 68 | 72 | 101 | 5.5% | 0 | 2 | 0% |
| StudyMentorApi.Services.Ai.AiModelRouter | 72 | 26 | 98 | 137 | 73.4% | 10 | 24 | 41.6% |
| StudyMentorApi.Services.Ai.AiProviderRequest | 13 | 0 | 13 | 34 | 100% | 0 | 0 |  |
| StudyMentorApi.Services.Ai.AiProviderResponse | 4 | 0 | 4 | 34 | 100% | 0 | 0 |  |
| StudyMentorApi.Services.Ai.Embeddings.AiEmbeddingOptions | 3 | 0 | 3 | 32 | 100% | 0 | 0 |  |
| StudyMentorApi.Services.Ai.Embeddings.AiEmbeddingResult | 0 | 4 | 4 | 8 | 0% | 0 | 0 |  |
| StudyMentorApi.Services.Ai.Embeddings.AiEmbeddingUnavailableException | 0 | 1 | 1 | 4 | 0% | 0 | 0 |  |
| StudyMentorApi.Services.Ai.Embeddings.LmStudioEmbeddingOptions | 3 | 0 | 3 | 32 | 100% | 0 | 0 |  |
| StudyMentorApi.Services.Ai.Embeddings.LmStudioEmbeddingService | 3 | 73 | 76 | 116 | 3.9% | 0 | 24 | 0% |
| StudyMentorApi.Services.Ai.Embeddings.RagRetrievalOptions | 3 | 1 | 4 | 32 | 75% | 0 | 0 |  |
| StudyMentorApi.Services.Ai.LmStudioProviderClient | 1 | 58 | 59 | 88 | 1.6% | 0 | 14 | 0% |
| StudyMentorApi.Services.Ai.NvidiaProviderClient | 1 | 79 | 80 | 116 | 1.2% | 0 | 24 | 0% |
| StudyMentorApi.Services.Ai.Prompts.ComposedPrompt | 3 | 1 | 4 | 25 | 75% | 0 | 0 |  |
| StudyMentorApi.Services.Ai.Prompts.PromptBuilder | 65 | 0 | 65 | 98 | 100% | 4 | 4 | 100% |
| StudyMentorApi.Services.Ai.Prompts.PromptComposer | 101 | 35 | 136 | 196 | 74.2% | 7 | 12 | 58.3% |
| StudyMentorApi.Services.Ai.Prompts.PromptCompositionRequest | 7 | 0 | 7 | 25 | 100% | 0 | 0 |  |
| StudyMentorApi.Services.Ai.Prompts.PromptContext | 0 | 9 | 9 | 11 | 0% | 0 | 0 |  |
| StudyMentorApi.Services.Ai.Prompts.PromptTemplateProvider | 1 | 23 | 24 | 42 | 4.1% | 0 | 6 | 0% |
| StudyMentorApi.Services.Ai.Prompts.PromptTemplateService | 0 | 35 | 35 | 56 | 0% | 0 | 6 | 0% |
| StudyMentorApi.Services.Ai.StructuredOutput.AiStructuredOutputParseException | 1 | 0 | 1 | 4 | 100% | 0 | 0 |  |
| StudyMentorApi.Services.Ai.StructuredOutput.AiStructuredOutputParser | 88 | 15 | 103 | 157 | 85.4% | 32 | 48 | 66.6% |
| StudyMentorApi.Services.Ai.StructuredOutput.GeneratedFlashcardDto | 2 | 0 | 2 | 33 | 100% | 0 | 0 |  |
| StudyMentorApi.Services.Ai.StructuredOutput.GeneratedFlashcardsDto | 1 | 0 | 1 | 33 | 100% | 0 | 0 |  |
| StudyMentorApi.Services.Ai.StructuredOutput.GeneratedQuestionDto | 5 | 0 | 5 | 33 | 100% | 0 | 0 |  |
| StudyMentorApi.Services.Ai.StructuredOutput.GeneratedTestDto | 2 | 0 | 2 | 33 | 100% | 0 | 0 |  |
| StudyMentorApi.Services.BaseCrudService<T> | 12 | 20 | 32 | 94 | 37.5% | 2 | 6 | 33.3% |
| StudyMentorApi.Subjects.SubjectEndpoints | 10 | 67 | 77 | 138 | 12.9% | 0 | 0 |  |
| StudyMentorApi.Subjects.SubjectRequest | 0 | 1 | 1 | 3 | 0% | 0 | 0 |  |
| StudyMentorApi.Subjects.SubjectResponse | 0 | 1 | 1 | 5 | 0% | 0 | 0 |  |
| StudyMentorApi.Subjects.SubjectService | 0 | 34 | 34 | 67 | 0% | 0 | 0 |  |
| StudyMentorApi.Tests.TestAnswerRequest | 0 | 1 | 1 | 3 | 0% | 0 | 0 |  |
| StudyMentorApi.Tests.TestAnswerResultResponse | 0 | 5 | 5 | 7 | 0% | 0 | 0 |  |
| StudyMentorApi.Tests.TestAnswerVariantRequest | 0 | 3 | 3 | 5 | 0% | 0 | 0 |  |
| StudyMentorApi.Tests.TestAnswerVariantResponse | 0 | 3 | 3 | 5 | 0% | 0 | 0 |  |
| StudyMentorApi.Tests.TestEndpoints | 11 | 105 | 116 | 184 | 9.4% | 0 | 20 | 0% |
| StudyMentorApi.Tests.TestQuestionRequest | 0 | 3 | 3 | 5 | 0% | 0 | 0 |  |
| StudyMentorApi.Tests.TestQuestionResponse | 0 | 4 | 4 | 6 | 0% | 0 | 0 |  |
| StudyMentorApi.Tests.TestRequest | 0 | 5 | 5 | 7 | 0% | 0 | 0 |  |
| StudyMentorApi.Tests.TestResponse | 0 | 6 | 6 | 8 | 0% | 0 | 0 |  |
| StudyMentorApi.Tests.TestService | 0 | 89 | 89 | 146 | 0% | 0 | 14 | 0% |
| StudyMentorApi.Users.UserEndpoints | 10 | 43 | 53 | 91 | 18.8% | 0 | 10 | 0% |
| StudyMentorApi.Users.UserRequest | 0 | 6 | 6 | 9 | 0% | 0 | 0 |  |
| StudyMentorApi.Users.UserResponse | 0 | 6 | 6 | 9 | 0% | 0 | 0 |  |
| StudyMentorApi.Users.UserService | 2 | 27 | 29 | 53 | 6.8% | 0 | 2 | 0% |
| System.Runtime.CompilerServices | 0 | 3 | 3 | 23 | 0% | 0 | 0 |  |

