using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace APISeasonalTicket.Migrations
{
    /// <inheritdoc />
    public partial class payments : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "PaymentId",
                table: "MovimientosAbonos",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "PaymentMethod",
                table: "MovimientosAbonos",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "PaymentStatus",
                table: "MovimientosAbonos",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "PaymentId",
                table: "MovimientosAbonos");

            migrationBuilder.DropColumn(
                name: "PaymentMethod",
                table: "MovimientosAbonos");

            migrationBuilder.DropColumn(
                name: "PaymentStatus",
                table: "MovimientosAbonos");
        }
    }
}
