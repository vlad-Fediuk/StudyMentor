using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace StudyMentorApi.Migrations
{
    /// <inheritdoc />
    public partial class RemoveDefaultStudyRecords : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("""DELETE FROM lectures WHERE "Name" = 'Default lecture';""");
            migrationBuilder.Sql("""DELETE FROM subjects WHERE "Name" = 'Default subject';""");
            migrationBuilder.Sql("""DELETE FROM majors WHERE "Name" = 'Default major';""");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
        }
    }
}
