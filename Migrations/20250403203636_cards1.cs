using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace APISeasonalTicket.Migrations
{
    /// <inheritdoc />
    public partial class cards1 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<decimal>(
                name: "Amount",
                table: "UserSubscriptions",
                type: "decimal(18,2)",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<int>(
                name: "CreditCardId",
                table: "UserSubscriptions",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<bool>(
                name: "IsActive",
                table: "CreditCards",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.CreateIndex(
                name: "IX_UserSubscriptions_CreditCardId",
                table: "UserSubscriptions",
                column: "CreditCardId");

            migrationBuilder.AddForeignKey(
                name: "FK_UserSubscriptions_CreditCards_CreditCardId",
                table: "UserSubscriptions",
                column: "CreditCardId",
                principalTable: "CreditCards",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_UserSubscriptions_CreditCards_CreditCardId",
                table: "UserSubscriptions");

            migrationBuilder.DropIndex(
                name: "IX_UserSubscriptions_CreditCardId",
                table: "UserSubscriptions");

            migrationBuilder.DropColumn(
                name: "Amount",
                table: "UserSubscriptions");

            migrationBuilder.DropColumn(
                name: "CreditCardId",
                table: "UserSubscriptions");

            migrationBuilder.DropColumn(
                name: "IsActive",
                table: "CreditCards");
        }
    }
}
