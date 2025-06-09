using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace KasaWGrupie.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddExchangeRate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "ExchangeRates",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    FromCurrencyId = table.Column<int>(type: "integer", nullable: false),
                    ToCurrencyId = table.Column<int>(type: "integer", nullable: false),
                    Rate = table.Column<decimal>(type: "numeric(18,6)", precision: 18, scale: 6, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ExchangeRates", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ExchangeRates_Currencies_FromCurrencyId",
                        column: x => x.FromCurrencyId,
                        principalTable: "Currencies",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_ExchangeRates_Currencies_ToCurrencyId",
                        column: x => x.ToCurrencyId,
                        principalTable: "Currencies",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.InsertData(
                table: "Currencies",
                columns: new[] { "Id", "Name" },
                values: new object[,]
                {
                    { 1, "PLN" },
                    { 2, "EUR" },
                    { 3, "USD" },
                    { 4, "GBP" }
                });

            migrationBuilder.InsertData(
                table: "ExchangeRates",
                columns: new[] { "Id", "FromCurrencyId", "Rate", "ToCurrencyId" },
                values: new object[,]
                {
                    { 1, 1, 0.22m, 2 },
                    { 2, 1, 0.25m, 3 },
                    { 3, 1, 0.19m, 4 },
                    { 4, 2, 4.55m, 1 },
                    { 5, 2, 1.12m, 3 },
                    { 6, 2, 0.85m, 4 },
                    { 7, 3, 4.00m, 1 },
                    { 8, 3, 0.89m, 2 },
                    { 9, 3, 0.76m, 4 },
                    { 10, 4, 5.10m, 1 },
                    { 11, 4, 1.17m, 2 },
                    { 12, 4, 1.32m, 3 }
                });

            migrationBuilder.CreateIndex(
                name: "IX_ExchangeRates_FromCurrencyId_ToCurrencyId",
                table: "ExchangeRates",
                columns: new[] { "FromCurrencyId", "ToCurrencyId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_ExchangeRates_ToCurrencyId",
                table: "ExchangeRates",
                column: "ToCurrencyId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ExchangeRates");

            migrationBuilder.DeleteData(
                table: "Currencies",
                keyColumn: "Id",
                keyValue: 1);

            migrationBuilder.DeleteData(
                table: "Currencies",
                keyColumn: "Id",
                keyValue: 2);

            migrationBuilder.DeleteData(
                table: "Currencies",
                keyColumn: "Id",
                keyValue: 3);

            migrationBuilder.DeleteData(
                table: "Currencies",
                keyColumn: "Id",
                keyValue: 4);
        }
    }
}
