using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Watchlist.Migrations
{
    /// <inheritdoc />
    public partial class AddCimagePropertiesClassFilm : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "CouvertureUrl",
                table: "ModeleVueFilm",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "CouvertureUrl",
                table: "Films",
                type: "nvarchar(max)",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "CouvertureUrl",
                table: "ModeleVueFilm");

            migrationBuilder.DropColumn(
                name: "CouvertureUrl",
                table: "Films");
        }
    }
}
