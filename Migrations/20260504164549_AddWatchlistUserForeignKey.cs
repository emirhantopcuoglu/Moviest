using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Moviest.Migrations
{
    /// <inheritdoc />
    public partial class AddWatchlistUserForeignKey : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddForeignKey(
                name: "FK_WatchlistItems_AspNetUsers_UserId",
                table: "WatchlistItems",
                column: "UserId",
                principalTable: "AspNetUsers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_WatchlistItems_AspNetUsers_UserId",
                table: "WatchlistItems");
        }
    }
}
