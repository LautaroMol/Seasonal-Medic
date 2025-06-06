using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace APISeasonalMedic.Migrations
{
    /// <inheritdoc />
    public partial class emails : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<DateTime>(
                name: "VerificationCodeExpiry",
                table: "AspNetUsers",
                type: "datetime2",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "VerificationCodeExpiry",
                table: "AspNetUsers");
        }
    }
}
