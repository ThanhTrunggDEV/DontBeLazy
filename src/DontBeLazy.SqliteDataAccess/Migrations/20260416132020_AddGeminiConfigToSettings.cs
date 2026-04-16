using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace DontBeLazy.SqliteDataAccess.Migrations
{
    /// <inheritdoc />
    public partial class AddGeminiConfigToSettings : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "GeminiApiKey",
                table: "Settings",
                type: "TEXT",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "GeminiModel",
                table: "Settings",
                type: "TEXT",
                nullable: false,
                defaultValue: "");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "GeminiApiKey",
                table: "Settings");

            migrationBuilder.DropColumn(
                name: "GeminiModel",
                table: "Settings");
        }
    }
}
