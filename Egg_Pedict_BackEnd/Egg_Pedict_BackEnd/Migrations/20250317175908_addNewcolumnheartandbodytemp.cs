using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Egg_Pedict_BackEnd.Migrations
{
    /// <inheritdoc />
    public partial class addNewcolumnheartandbodytemp : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<double>(
                name: "BodyTemp",
                table: "LiveData",
                type: "double precision",
                nullable: false,
                defaultValue: 0.0);

            migrationBuilder.AddColumn<double>(
                name: "HeartRate",
                table: "LiveData",
                type: "double precision",
                nullable: false,
                defaultValue: 0.0);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "BodyTemp",
                table: "LiveData");

            migrationBuilder.DropColumn(
                name: "HeartRate",
                table: "LiveData");
        }
    }
}
