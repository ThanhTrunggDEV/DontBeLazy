using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace DontBeLazy.SqliteDataAccess.Migrations
{
    /// <inheritdoc />
    public partial class RemoveGiveUpQuotes : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "GiveUpQuotes",
                table: "Settings");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "GiveUpQuotes",
                table: "Settings",
                type: "TEXT",
                nullable: false,
                defaultValue: "");
        }
    }
}
