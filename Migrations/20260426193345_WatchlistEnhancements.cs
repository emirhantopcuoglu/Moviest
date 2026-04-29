using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Moviest.Migrations
{
    /// <inheritdoc />
    public partial class WatchlistEnhancements : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "IsWatched",
                table: "WatchlistItems",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<int>(
                name: "PersonalRating",
                table: "WatchlistItems",
                type: "int",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "IsWatched",
                table: "WatchlistItems");

            migrationBuilder.DropColumn(
                name: "PersonalRating",
                table: "WatchlistItems");
        }
    }
}
