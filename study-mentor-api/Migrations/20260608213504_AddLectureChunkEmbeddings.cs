using Microsoft.EntityFrameworkCore.Migrations;
using Pgvector;

#nullable disable

namespace StudyMentorApi.Migrations
{
    /// <inheritdoc />
    public partial class AddLectureChunkEmbeddings : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_lecture_chunks_LectureId",
                table: "lecture_chunks");

            migrationBuilder.AlterDatabase()
                .Annotation("Npgsql:PostgresExtension:pgcrypto", ",,")
                .Annotation("Npgsql:PostgresExtension:vector", ",,")
                .OldAnnotation("Npgsql:PostgresExtension:pgcrypto", ",,");

            migrationBuilder.AddColumn<Vector>(
                name: "Embedding",
                table: "lecture_chunks",
                type: "vector",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "EmbeddingDimensions",
                table: "lecture_chunks",
                type: "integer",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "EmbeddingModel",
                table: "lecture_chunks",
                type: "character varying(256)",
                maxLength: 256,
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_lecture_chunks_LectureId_EmbeddingModel_EmbeddingDimensions",
                table: "lecture_chunks",
                columns: new[] { "LectureId", "EmbeddingModel", "EmbeddingDimensions" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_lecture_chunks_LectureId_EmbeddingModel_EmbeddingDimensions",
                table: "lecture_chunks");

            migrationBuilder.DropColumn(
                name: "Embedding",
                table: "lecture_chunks");

            migrationBuilder.DropColumn(
                name: "EmbeddingDimensions",
                table: "lecture_chunks");

            migrationBuilder.DropColumn(
                name: "EmbeddingModel",
                table: "lecture_chunks");

            migrationBuilder.AlterDatabase()
                .Annotation("Npgsql:PostgresExtension:pgcrypto", ",,")
                .OldAnnotation("Npgsql:PostgresExtension:pgcrypto", ",,")
                .OldAnnotation("Npgsql:PostgresExtension:vector", ",,");

            migrationBuilder.CreateIndex(
                name: "IX_lecture_chunks_LectureId",
                table: "lecture_chunks",
                column: "LectureId");
        }
    }
}
