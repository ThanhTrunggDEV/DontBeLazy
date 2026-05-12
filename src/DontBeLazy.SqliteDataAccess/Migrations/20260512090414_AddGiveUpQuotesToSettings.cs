using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace DontBeLazy.SqliteDataAccess.Migrations
{
    /// <inheritdoc />
    public partial class AddGiveUpQuotesToSettings : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "GiveUpQuotes",
                table: "Settings",
                type: "TEXT",
                nullable: false,
                defaultValue: "[]");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "GiveUpQuotes",
                table: "Settings");
        }
    }
}
