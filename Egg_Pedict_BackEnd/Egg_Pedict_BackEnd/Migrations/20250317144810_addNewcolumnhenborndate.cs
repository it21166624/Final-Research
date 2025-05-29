using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Egg_Pedict_BackEnd.Migrations
{
    /// <inheritdoc />
    public partial class addNewcolumnhenborndate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<DateTime>(
                name: "Born_Date",
                table: "LiveData",
                type: "timestamp with time zone",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Born_Date",
                table: "LiveData");
        }
    }
}
