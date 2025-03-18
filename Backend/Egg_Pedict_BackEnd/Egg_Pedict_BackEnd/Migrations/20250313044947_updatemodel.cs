using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Egg_Pedict_BackEnd.Migrations
{
    /// <inheritdoc />
    public partial class updatemodel : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<double>(
                name: "Egg_count",
                table: "SensorData",
                type: "double precision",
                nullable: false,
                defaultValue: 0.0);

            migrationBuilder.AddColumn<double>(
                name: "Feed_Quantity",
                table: "SensorData",
                type: "double precision",
                nullable: false,
                defaultValue: 0.0);

            migrationBuilder.AddColumn<string>(
                name: "Health_Status",
                table: "SensorData",
                type: "text",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<double>(
                name: "Hen_Age_weeks",
                table: "SensorData",
                type: "double precision",
                nullable: false,
                defaultValue: 0.0);

            migrationBuilder.AddColumn<double>(
                name: "Hen_Count",
                table: "SensorData",
                type: "double precision",
                nullable: false,
                defaultValue: 0.0);

            migrationBuilder.AddColumn<double>(
                name: "Light_Hours",
                table: "SensorData",
                type: "double precision",
                nullable: false,
                defaultValue: 0.0);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Egg_count",
                table: "SensorData");

            migrationBuilder.DropColumn(
                name: "Feed_Quantity",
                table: "SensorData");

            migrationBuilder.DropColumn(
                name: "Health_Status",
                table: "SensorData");

            migrationBuilder.DropColumn(
                name: "Hen_Age_weeks",
                table: "SensorData");

            migrationBuilder.DropColumn(
                name: "Hen_Count",
                table: "SensorData");

            migrationBuilder.DropColumn(
                name: "Light_Hours",
                table: "SensorData");
        }
    }
}
