using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace StudyMentorApi.Migrations
{
    /// <inheritdoc />
    public partial class SeedStudySubjectsAndLectures : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.InsertData(
                table: "majors",
                columns: new[] { "Id", "Name" },
                values: new object[] { new Guid("b03b7164-1f6a-4f9f-b5de-078f394a42e1"), "Комп'ютерні науки" });

            migrationBuilder.InsertData(
                table: "subjects",
                columns: new[] { "Id", "MajorId", "Name" },
                values: new object[,]
                {
                    { new Guid("05a89d91-f68b-46ab-b8ff-870e5a9d6114"), new Guid("b03b7164-1f6a-4f9f-b5de-078f394a42e1"), "ООП" },
                    { new Guid("ff026d26-6ba6-4a84-9056-9f8f35dd7701"), new Guid("b03b7164-1f6a-4f9f-b5de-078f394a42e1"), "Тестування" }
                });

            migrationBuilder.InsertData(
                table: "lectures",
                columns: new[] { "Id", "Name", "SubjectId" },
                values: new object[,]
                {
                    { new Guid("647fda35-6dc0-46d3-8f10-829d4f670189"), "Основи тестування", new Guid("ff026d26-6ba6-4a84-9056-9f8f35dd7701") },
                    { new Guid("7298c84c-3a33-4827-b643-8a12d5523c09"), "Вступ до ООП", new Guid("05a89d91-f68b-46ab-b8ff-870e5a9d6114") },
                    { new Guid("ee89226b-5a41-4dd8-a898-44a38941c1d2"), "Unit-тестування", new Guid("ff026d26-6ba6-4a84-9056-9f8f35dd7701") },
                    { new Guid("ef92d905-02ed-42ad-9583-6d1470461522"), "Інкапсуляція та наслідування", new Guid("05a89d91-f68b-46ab-b8ff-870e5a9d6114") }
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                table: "lectures",
                keyColumn: "Id",
                keyValue: new Guid("647fda35-6dc0-46d3-8f10-829d4f670189"));

            migrationBuilder.DeleteData(
                table: "lectures",
                keyColumn: "Id",
                keyValue: new Guid("7298c84c-3a33-4827-b643-8a12d5523c09"));

            migrationBuilder.DeleteData(
                table: "lectures",
                keyColumn: "Id",
                keyValue: new Guid("ee89226b-5a41-4dd8-a898-44a38941c1d2"));

            migrationBuilder.DeleteData(
                table: "lectures",
                keyColumn: "Id",
                keyValue: new Guid("ef92d905-02ed-42ad-9583-6d1470461522"));

            migrationBuilder.DeleteData(
                table: "subjects",
                keyColumn: "Id",
                keyValue: new Guid("05a89d91-f68b-46ab-b8ff-870e5a9d6114"));

            migrationBuilder.DeleteData(
                table: "subjects",
                keyColumn: "Id",
                keyValue: new Guid("ff026d26-6ba6-4a84-9056-9f8f35dd7701"));

            migrationBuilder.DeleteData(
                table: "majors",
                keyColumn: "Id",
                keyValue: new Guid("b03b7164-1f6a-4f9f-b5de-078f394a42e1"));
        }
    }
}
