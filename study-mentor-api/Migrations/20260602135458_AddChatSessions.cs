using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace StudyMentorApi.Migrations
{
    /// <inheritdoc />
    public partial class AddChatSessions : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "Status",
                table: "chat_messages",
                type: "text",
                nullable: false,
                defaultValue: "completed");

            migrationBuilder.CreateTable(
                name: "chat_sessions",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false, defaultValueSql: "gen_random_uuid()"),
                    UserId = table.Column<Guid>(type: "uuid", nullable: false),
                    LectureId = table.Column<Guid>(type: "uuid", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_chat_sessions", x => x.Id);
                    table.ForeignKey(
                        name: "FK_chat_sessions_lectures_LectureId",
                        column: x => x.LectureId,
                        principalTable: "lectures",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_chat_sessions_users_UserId",
                        column: x => x.UserId,
                        principalTable: "users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_chat_messages_ChatSessionId",
                table: "chat_messages",
                column: "ChatSessionId");

            migrationBuilder.CreateIndex(
                name: "IX_chat_sessions_LectureId",
                table: "chat_sessions",
                column: "LectureId");

            migrationBuilder.CreateIndex(
                name: "IX_chat_sessions_UserId",
                table: "chat_sessions",
                column: "UserId");

            migrationBuilder.AddForeignKey(
                name: "FK_chat_messages_chat_sessions_ChatSessionId",
                table: "chat_messages",
                column: "ChatSessionId",
                principalTable: "chat_sessions",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_chat_messages_chat_sessions_ChatSessionId",
                table: "chat_messages");

            migrationBuilder.DropTable(
                name: "chat_sessions");

            migrationBuilder.DropIndex(
                name: "IX_chat_messages_ChatSessionId",
                table: "chat_messages");

            migrationBuilder.DropColumn(
                name: "Status",
                table: "chat_messages");
        }
    }
}
