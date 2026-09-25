using System;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace StudyMentorApi.Migrations
{
    /// <inheritdoc />
    [DbContext(typeof(StudyMentorApi.Data.AppDbContext))]
    [Migration("20260609120000_UpdateGenerationPromptTemplates")]
    public partial class UpdateGenerationPromptTemplates : Migration
    {
        private const string TestGenerationTemplate = "Generate a study test from the user's request and available lecture context.\nReturn valid JSON only: no markdown, no prose, no comments, no surrounding text.\nUse Ukrainian unless the user explicitly asks for another language.\nCreate clear single-choice questions with plausible distractors.\nEach question must have exactly one correct answer, and correctAnswer must exactly match one option.\nFollow this schema and rules:\n{{response_schema}}";

        private const string FlashcardGenerationTemplate = "Generate study flashcards from the user's request and available lecture context.\nReturn valid JSON only: no markdown, no prose, no comments, no surrounding text.\nUse Ukrainian unless the user explicitly asks for another language.\nMake each front concise and each back useful for memorization.\nFollow this schema and rules:\n{{response_schema}}";

        private const string PreviousTestGenerationTemplate = "Generate a study test for the requested topic.\nReturn JSON only. Do not include markdown, explanations, or text outside JSON.\nInclude clear questions, answer options when relevant, and correct answers.\nUse this schema or rules if provided:\n{{response_schema}}";

        private const string PreviousFlashcardGenerationTemplate = "Generate study flashcards for the requested topic.\nReturn JSON only. Do not include markdown, explanations, or text outside JSON.\nEach flashcard must have a front/question and back/answer.\nUse this schema or rules if provided:\n{{response_schema}}";

        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            UpdateTemplate(
                migrationBuilder,
                "4517ed60-84ac-46b5-a8e9-64f61b09ca29",
                TestGenerationTemplate);

            UpdateTemplate(
                migrationBuilder,
                "4da3b451-4245-455f-84d6-794de4e71cda",
                FlashcardGenerationTemplate);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            UpdateTemplate(
                migrationBuilder,
                "4517ed60-84ac-46b5-a8e9-64f61b09ca29",
                PreviousTestGenerationTemplate);

            UpdateTemplate(
                migrationBuilder,
                "4da3b451-4245-455f-84d6-794de4e71cda",
                PreviousFlashcardGenerationTemplate);
        }

        private static void UpdateTemplate(
            MigrationBuilder migrationBuilder,
            string id,
            string template)
        {
            migrationBuilder.Sql(
                $"""
                UPDATE prompt_templates
                SET "Template" = '{EscapeSqlLiteral(template)}'
                WHERE "Id" = '{id}'::uuid;
                """);
        }

        private static string EscapeSqlLiteral(string value)
        {
            return value.Replace("'", "''");
        }
    }
}
