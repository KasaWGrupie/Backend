using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace KasaWGrupie.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddDateAndRenameStatusInPayRequest : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "payRequstStatus",
                table: "PayRequests",
                newName: "PayRequestStatus");

            migrationBuilder.AddColumn<DateTime>(
                name: "EndDate",
                table: "PayRequests",
                type: "timestamp with time zone",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "EndDate",
                table: "PayRequests");

            migrationBuilder.RenameColumn(
                name: "PayRequestStatus",
                table: "PayRequests",
                newName: "payRequstStatus");
        }
    }
}
