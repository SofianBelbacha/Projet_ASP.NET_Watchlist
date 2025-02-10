using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Watchlist.Migrations
{
    /// <inheritdoc />
    public partial class AddDureePropertiesClassFilm : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "Autheur",
                table: "ModeleVueFilm",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<TimeOnly>(
                name: "Durée",
                table: "ModeleVueFilm",
                type: "time",
                nullable: false,
                defaultValue: new TimeOnly(0, 0, 0));

            migrationBuilder.AddColumn<TimeOnly>(
                name: "Durée",
                table: "Films",
                type: "time",
                nullable: false,
                defaultValue: new TimeOnly(0, 0, 0));
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Autheur",
                table: "ModeleVueFilm");

            migrationBuilder.DropColumn(
                name: "Durée",
                table: "ModeleVueFilm");

            migrationBuilder.DropColumn(
                name: "Durée",
                table: "Films");
        }
    }
}
