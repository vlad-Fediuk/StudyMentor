using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace StudyMentorApi.Migrations
{
    /// <inheritdoc />
    public partial class AddTests : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<Guid>(
                name: "SourceFlashcardId",
                table: "exercises",
                type: "uuid",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "test_questions",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false, defaultValueSql: "gen_random_uuid()"),
                    Prompt = table.Column<string>(type: "text", nullable: false),
                    Order = table.Column<int>(type: "integer", nullable: false),
                    TestId = table.Column<Guid>(type: "uuid", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_test_questions", x => x.Id);
                    table.ForeignKey(
                        name: "FK_test_questions_exercises_TestId",
                        column: x => x.TestId,
                        principalTable: "exercises",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "test_answer_variants",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false, defaultValueSql: "gen_random_uuid()"),
                    Text = table.Column<string>(type: "text", nullable: false),
                    IsCorrect = table.Column<bool>(type: "boolean", nullable: false),
                    Order = table.Column<int>(type: "integer", nullable: false),
                    TestQuestionId = table.Column<Guid>(type: "uuid", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_test_answer_variants", x => x.Id);
                    table.ForeignKey(
                        name: "FK_test_answer_variants_test_questions_TestQuestionId",
                        column: x => x.TestQuestionId,
                        principalTable: "test_questions",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_exercises_SourceFlashcardId",
                table: "exercises",
                column: "SourceFlashcardId");

            migrationBuilder.CreateIndex(
                name: "IX_test_answer_variants_TestQuestionId",
                table: "test_answer_variants",
                column: "TestQuestionId");

            migrationBuilder.CreateIndex(
                name: "IX_test_questions_TestId",
                table: "test_questions",
                column: "TestId");

            migrationBuilder.AddForeignKey(
                name: "FK_exercises_exercises_SourceFlashcardId",
                table: "exercises",
                column: "SourceFlashcardId",
                principalTable: "exercises",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_exercises_exercises_SourceFlashcardId",
                table: "exercises");

            migrationBuilder.DropTable(
                name: "test_answer_variants");

            migrationBuilder.DropTable(
                name: "test_questions");

            migrationBuilder.DropIndex(
                name: "IX_exercises_SourceFlashcardId",
                table: "exercises");

            migrationBuilder.DropColumn(
                name: "SourceFlashcardId",
                table: "exercises");
        }
    }
}
