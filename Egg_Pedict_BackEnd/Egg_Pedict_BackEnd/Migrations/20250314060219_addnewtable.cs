using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace Egg_Pedict_BackEnd.Migrations
{
    /// <inheritdoc />
    public partial class addnewtable : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "hen_production",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Temperature = table.Column<double>(type: "double precision", nullable: false),
                    Humidity = table.Column<double>(type: "double precision", nullable: false),
                    Light_Hours = table.Column<double>(type: "double precision", nullable: false),
                    Hen_Age_weeks = table.Column<double>(type: "double precision", nullable: false),
                    Feed_Quantity = table.Column<double>(type: "double precision", nullable: false),
                    Health_Status = table.Column<string>(type: "text", nullable: false),
                    Hen_Count = table.Column<double>(type: "double precision", nullable: false),
                    Egg_count = table.Column<double>(type: "double precision", nullable: false),
                    Timestamp = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_hen_production", x => x.Id);
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "hen_production");
        }
    }
}
