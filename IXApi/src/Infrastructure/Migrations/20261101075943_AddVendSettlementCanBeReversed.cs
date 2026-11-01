using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace IAX.IXApi.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddVendSettlementCanBeReversed : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_VendSettlement_DataAreaId_InvoiceTransRecId_PaymentTransRecId",
                table: "VendSettlement");

            migrationBuilder.AddColumn<int>(
                name: "CanBeReversed",
                table: "VendSettlement",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.CreateIndex(
                name: "IX_VendSettlement_DataAreaId_InvoiceTransRecId_PaymentTransRecId_Voucher",
                table: "VendSettlement",
                columns: new[] { "DataAreaId", "InvoiceTransRecId", "PaymentTransRecId", "Voucher" },
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_VendSettlement_DataAreaId_InvoiceTransRecId_PaymentTransRecId_Voucher",
                table: "VendSettlement");

            migrationBuilder.DropColumn(
                name: "CanBeReversed",
                table: "VendSettlement");

            migrationBuilder.CreateIndex(
                name: "IX_VendSettlement_DataAreaId_InvoiceTransRecId_PaymentTransRecId",
                table: "VendSettlement",
                columns: new[] { "DataAreaId", "InvoiceTransRecId", "PaymentTransRecId" },
                unique: true);
        }
    }
}
