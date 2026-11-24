using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace IAX.IXApi.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class VendInvoiceRegister : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "Approved",
                table: "VendTrans",
                type: "int",
                nullable: false,
                defaultValue: 1); // existing vendor transactions are approved (only the invoice register posts unapproved ones)

            migrationBuilder.AddColumn<long>(
                name: "PurchasingLedgerDimension",
                table: "VendLedgerAccounts",
                type: "bigint",
                nullable: false,
                defaultValue: 0L);

            migrationBuilder.AddColumn<long>(
                name: "PurchasingOffsetLedgerDimension",
                table: "VendLedgerAccounts",
                type: "bigint",
                nullable: false,
                defaultValue: 0L);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Approved",
                table: "VendTrans");

            migrationBuilder.DropColumn(
                name: "PurchasingLedgerDimension",
                table: "VendLedgerAccounts");

            migrationBuilder.DropColumn(
                name: "PurchasingOffsetLedgerDimension",
                table: "VendLedgerAccounts");
        }
    }
}
