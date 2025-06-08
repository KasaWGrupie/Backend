using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace KasaWGrupie.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddCurrencyToPayRequest : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "CurrencyId",
                table: "PayRequests",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.CreateIndex(
                name: "IX_PayRequests_CurrencyId",
                table: "PayRequests",
                column: "CurrencyId");

            migrationBuilder.AddForeignKey(
                name: "FK_PayRequests_Currencies_CurrencyId",
                table: "PayRequests",
                column: "CurrencyId",
                principalTable: "Currencies",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_PayRequests_Currencies_CurrencyId",
                table: "PayRequests");

            migrationBuilder.DropIndex(
                name: "IX_PayRequests_CurrencyId",
                table: "PayRequests");

            migrationBuilder.DropColumn(
                name: "CurrencyId",
                table: "PayRequests");
        }
    }
}
