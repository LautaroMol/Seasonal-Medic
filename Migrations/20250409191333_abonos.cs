using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace APISeasonalTicket.Migrations
{
    /// <inheritdoc />
    public partial class abonos : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "Plan",
                table: "Abonos",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Plan",
                table: "Abonos");
        }
    }
}
