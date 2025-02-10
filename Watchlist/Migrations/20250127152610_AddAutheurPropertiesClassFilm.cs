using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Watchlist.Migrations
{
    /// <inheritdoc />
    public partial class AddAutheurPropertiesClassFilm : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<string>(
                name: "Titre",
                table: "ModeleVueFilm",
                type: "nvarchar(max)",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)");

            migrationBuilder.AddColumn<double>(
                name: "MoyenneNote",
                table: "ModeleVueFilm",
                type: "float",
                nullable: false,
                defaultValue: 0.0);

            migrationBuilder.AddColumn<string>(
                name: "Autheur",
                table: "Films",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "MoyenneNote",
                table: "ModeleVueFilm");

            migrationBuilder.DropColumn(
                name: "Autheur",
                table: "Films");

            migrationBuilder.AlterColumn<string>(
                name: "Titre",
                table: "ModeleVueFilm",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "nvarchar(max)",
                oldNullable: true);
        }
    }
}
