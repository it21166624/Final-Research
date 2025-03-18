using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace Egg_Pedict_BackEnd.Migrations
{
    /// <inheritdoc />
    public partial class addnewStressDataTable : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "StressData",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Temperature = table.Column<double>(type: "double precision", nullable: false),
                    Humidity = table.Column<double>(type: "double precision", nullable: false),
                    FeedIntakePerHen = table.Column<double>(type: "double precision", nullable: false),
                    WaterIntakePerHen = table.Column<double>(type: "double precision", nullable: false),
                    AirQuality = table.Column<string>(type: "text", nullable: false),
                    Lighting = table.Column<double>(type: "double precision", nullable: false),
                    CageDensity = table.Column<double>(type: "double precision", nullable: false),
                    Vocalization = table.Column<double>(type: "double precision", nullable: false),
                    BodyTemperature = table.Column<double>(type: "double precision", nullable: false),
                    Heartbeat = table.Column<int>(type: "integer", nullable: false),
                    StressLevel = table.Column<int>(type: "integer", nullable: false),
                    Timestamp = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_StressData", x => x.Id);
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "StressData");
        }
    }
}
