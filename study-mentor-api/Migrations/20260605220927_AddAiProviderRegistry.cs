using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace StudyMentorApi.Migrations
{
    /// <inheritdoc />
    public partial class AddAiProviderRegistry : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "ai_providers",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false, defaultValueSql: "gen_random_uuid()"),
                    Name = table.Column<string>(type: "text", nullable: false),
                    Type = table.Column<string>(type: "text", nullable: false),
                    BaseUrl = table.Column<string>(type: "text", nullable: false),
                    ApiKeyEnvironmentVariable = table.Column<string>(type: "text", nullable: true),
                    IsEnabled = table.Column<bool>(type: "boolean", nullable: false, defaultValue: true),
                    Priority = table.Column<int>(type: "integer", nullable: false),
                    TimeoutSeconds = table.Column<int>(type: "integer", nullable: false, defaultValue: 300),
                    SettingsJson = table.Column<string>(type: "jsonb", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ai_providers", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "ai_models",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false, defaultValueSql: "gen_random_uuid()"),
                    ProviderId = table.Column<Guid>(type: "uuid", nullable: false),
                    ModelName = table.Column<string>(type: "text", nullable: false),
                    DisplayName = table.Column<string>(type: "text", nullable: false),
                    IsEnabled = table.Column<bool>(type: "boolean", nullable: false, defaultValue: true),
                    Priority = table.Column<int>(type: "integer", nullable: false),
                    Temperature = table.Column<double>(type: "double precision", nullable: false),
                    TopP = table.Column<double>(type: "double precision", nullable: false, defaultValue: 1.0),
                    MaxOutputTokens = table.Column<int>(type: "integer", nullable: false, defaultValue: 2048),
                    ReasoningBudget = table.Column<int>(type: "integer", nullable: true),
                    EnableThinking = table.Column<bool>(type: "boolean", nullable: false),
                    CapabilitiesJson = table.Column<string>(type: "jsonb", nullable: true),
                    SettingsJson = table.Column<string>(type: "jsonb", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ai_models", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ai_models_ai_providers_ProviderId",
                        column: x => x.ProviderId,
                        principalTable: "ai_providers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.InsertData(
                table: "ai_providers",
                columns: new[] { "Id", "ApiKeyEnvironmentVariable", "BaseUrl", "IsEnabled", "Name", "Priority", "SettingsJson", "TimeoutSeconds", "Type" },
                values: new object[,]
                {
                    { new Guid("58f2f8b9-0d72-4c49-9dd0-6da81f4d4a01"), null, "http://localhost:1234/v1/chat/completions", true, "LmStudio", 1, null, 300, "lmstudio" },
                    { new Guid("cbe16bfc-6b2d-4d2f-a9e0-f0b3786c2102"), "NVIDIA_API_KEY", "https://integrate.api.nvidia.com/v1/chat/completions", true, "Nvidia", 2, null, 300, "nvidia" }
                });

            migrationBuilder.InsertData(
                table: "ai_models",
                columns: new[] { "Id", "CapabilitiesJson", "DisplayName", "EnableThinking", "IsEnabled", "MaxOutputTokens", "ModelName", "Priority", "ProviderId", "ReasoningBudget", "SettingsJson", "Temperature", "TopP" },
                values: new object[,]
                {
                    { new Guid("70de4e65-5337-4ee4-9224-7eceadf3ed2d"), null, "gemma-4-e2b-it", false, true, 2048, "gemma-4-e2b-it", 1, new Guid("58f2f8b9-0d72-4c49-9dd0-6da81f4d4a01"), null, null, 0.14999999999999999, 1.0 },
                    { new Guid("902ab4f1-c498-4f62-979a-99860b8548fe"), null, "mistralai/mistral-large-3-675b-instruct-2512", false, true, 2048, "mistralai/mistral-large-3-675b-instruct-2512", 1, new Guid("cbe16bfc-6b2d-4d2f-a9e0-f0b3786c2102"), 2048, null, 0.14999999999999999, 1.0 }
                });

            migrationBuilder.CreateIndex(
                name: "IX_ai_models_ProviderId_ModelName",
                table: "ai_models",
                columns: new[] { "ProviderId", "ModelName" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_ai_providers_Type",
                table: "ai_providers",
                column: "Type",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ai_models");

            migrationBuilder.DropTable(
                name: "ai_providers");
        }
    }
}
