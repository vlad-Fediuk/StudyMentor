using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace StudyMentorApi.Migrations
{
    /// <inheritdoc />
    public partial class RemoveDebugStudyRecords : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("""DELETE FROM lectures WHERE "Name" ILIKE 'Debug%';""");
            migrationBuilder.Sql("""DELETE FROM subjects WHERE "Name" ILIKE 'Debug%';""");
            migrationBuilder.Sql("""DELETE FROM majors WHERE "Name" ILIKE 'Debug%';""");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
        }
    }
}
