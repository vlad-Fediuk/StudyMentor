using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace StudyMentorApi.Migrations
{
    /// <inheritdoc />
    public partial class AddUserAuthFields : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "Email",
                table: "users",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string[]>(
                name: "Roles",
                table: "users",
                type: "text[]",
                nullable: false,
                defaultValue: new[] { "User" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Email",
                table: "users");

            migrationBuilder.DropColumn(
                name: "Roles",
                table: "users");
        }
    }
}
