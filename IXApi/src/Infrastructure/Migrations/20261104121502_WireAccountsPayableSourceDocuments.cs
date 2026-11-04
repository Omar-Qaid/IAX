using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace IAX.IXApi.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class WireAccountsPayableSourceDocuments : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<long>(
                name: "SourceDocumentLine",
                table: "VendPackingSlipTrans",
                type: "bigint",
                nullable: true);

            migrationBuilder.AddColumn<long>(
                name: "SourceDocumentHeader",
                table: "VendPackingSlipJour",
                type: "bigint",
                nullable: true);

            migrationBuilder.AddColumn<long>(
                name: "SourceDocumentLine",
                table: "VendInvoiceTrans",
                type: "bigint",
                nullable: true);

            migrationBuilder.AddColumn<long>(
                name: "SourceDocumentHeader",
                table: "VendInvoiceJour",
                type: "bigint",
                nullable: true);

            migrationBuilder.AddColumn<long>(
                name: "SourceDocumentHeader",
                table: "VendInvoiceInfoTable",
                type: "bigint",
                nullable: true);

            migrationBuilder.AddColumn<long>(
                name: "SourceDocumentLine",
                table: "VendInvoiceInfoLine",
                type: "bigint",
                nullable: true);

            migrationBuilder.AddColumn<long>(
                name: "SourceDocumentHeader",
                table: "PurchTable",
                type: "bigint",
                nullable: true);

            migrationBuilder.AddColumn<long>(
                name: "SourceDocumentLine",
                table: "PurchLine",
                type: "bigint",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_VendPackingSlipTrans_SourceDocumentLine",
                table: "VendPackingSlipTrans",
                column: "SourceDocumentLine");

            migrationBuilder.CreateIndex(
                name: "IX_VendPackingSlipJour_SourceDocumentHeader",
                table: "VendPackingSlipJour",
                column: "SourceDocumentHeader");

            migrationBuilder.CreateIndex(
                name: "IX_VendInvoiceTrans_SourceDocumentLine",
                table: "VendInvoiceTrans",
                column: "SourceDocumentLine");

            migrationBuilder.CreateIndex(
                name: "IX_VendInvoiceJour_SourceDocumentHeader",
                table: "VendInvoiceJour",
                column: "SourceDocumentHeader");

            migrationBuilder.CreateIndex(
                name: "IX_VendInvoiceInfoTable_SourceDocumentHeader",
                table: "VendInvoiceInfoTable",
                column: "SourceDocumentHeader");

            migrationBuilder.CreateIndex(
                name: "IX_VendInvoiceInfoLine_SourceDocumentLine",
                table: "VendInvoiceInfoLine",
                column: "SourceDocumentLine");

            migrationBuilder.CreateIndex(
                name: "IX_PurchTable_SourceDocumentHeader",
                table: "PurchTable",
                column: "SourceDocumentHeader");

            migrationBuilder.CreateIndex(
                name: "IX_PurchLine_SourceDocumentLine",
                table: "PurchLine",
                column: "SourceDocumentLine");

            migrationBuilder.AddForeignKey(
                name: "FK_PurchLine_SourceDocumentLine_SourceDocumentLine",
                table: "PurchLine",
                column: "SourceDocumentLine",
                principalTable: "SourceDocumentLine",
                principalColumn: "RECID",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_PurchTable_SourceDocumentHeader_SourceDocumentHeader",
                table: "PurchTable",
                column: "SourceDocumentHeader",
                principalTable: "SourceDocumentHeader",
                principalColumn: "RECID",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_VendInvoiceInfoLine_SourceDocumentLine_SourceDocumentLine",
                table: "VendInvoiceInfoLine",
                column: "SourceDocumentLine",
                principalTable: "SourceDocumentLine",
                principalColumn: "RECID",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_VendInvoiceInfoTable_SourceDocumentHeader_SourceDocumentHeader",
                table: "VendInvoiceInfoTable",
                column: "SourceDocumentHeader",
                principalTable: "SourceDocumentHeader",
                principalColumn: "RECID",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_VendInvoiceJour_SourceDocumentHeader_SourceDocumentHeader",
                table: "VendInvoiceJour",
                column: "SourceDocumentHeader",
                principalTable: "SourceDocumentHeader",
                principalColumn: "RECID",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_VendInvoiceTrans_SourceDocumentLine_SourceDocumentLine",
                table: "VendInvoiceTrans",
                column: "SourceDocumentLine",
                principalTable: "SourceDocumentLine",
                principalColumn: "RECID",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_VendPackingSlipJour_SourceDocumentHeader_SourceDocumentHeader",
                table: "VendPackingSlipJour",
                column: "SourceDocumentHeader",
                principalTable: "SourceDocumentHeader",
                principalColumn: "RECID",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_VendPackingSlipTrans_SourceDocumentLine_SourceDocumentLine",
                table: "VendPackingSlipTrans",
                column: "SourceDocumentLine",
                principalTable: "SourceDocumentLine",
                principalColumn: "RECID",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_PurchLine_SourceDocumentLine_SourceDocumentLine",
                table: "PurchLine");

            migrationBuilder.DropForeignKey(
                name: "FK_PurchTable_SourceDocumentHeader_SourceDocumentHeader",
                table: "PurchTable");

            migrationBuilder.DropForeignKey(
                name: "FK_VendInvoiceInfoLine_SourceDocumentLine_SourceDocumentLine",
                table: "VendInvoiceInfoLine");

            migrationBuilder.DropForeignKey(
                name: "FK_VendInvoiceInfoTable_SourceDocumentHeader_SourceDocumentHeader",
                table: "VendInvoiceInfoTable");

            migrationBuilder.DropForeignKey(
                name: "FK_VendInvoiceJour_SourceDocumentHeader_SourceDocumentHeader",
                table: "VendInvoiceJour");

            migrationBuilder.DropForeignKey(
                name: "FK_VendInvoiceTrans_SourceDocumentLine_SourceDocumentLine",
                table: "VendInvoiceTrans");

            migrationBuilder.DropForeignKey(
                name: "FK_VendPackingSlipJour_SourceDocumentHeader_SourceDocumentHeader",
                table: "VendPackingSlipJour");

            migrationBuilder.DropForeignKey(
                name: "FK_VendPackingSlipTrans_SourceDocumentLine_SourceDocumentLine",
                table: "VendPackingSlipTrans");

            migrationBuilder.DropIndex(
                name: "IX_VendPackingSlipTrans_SourceDocumentLine",
                table: "VendPackingSlipTrans");

            migrationBuilder.DropIndex(
                name: "IX_VendPackingSlipJour_SourceDocumentHeader",
                table: "VendPackingSlipJour");

            migrationBuilder.DropIndex(
                name: "IX_VendInvoiceTrans_SourceDocumentLine",
                table: "VendInvoiceTrans");

            migrationBuilder.DropIndex(
                name: "IX_VendInvoiceJour_SourceDocumentHeader",
                table: "VendInvoiceJour");

            migrationBuilder.DropIndex(
                name: "IX_VendInvoiceInfoTable_SourceDocumentHeader",
                table: "VendInvoiceInfoTable");

            migrationBuilder.DropIndex(
                name: "IX_VendInvoiceInfoLine_SourceDocumentLine",
                table: "VendInvoiceInfoLine");

            migrationBuilder.DropIndex(
                name: "IX_PurchTable_SourceDocumentHeader",
                table: "PurchTable");

            migrationBuilder.DropIndex(
                name: "IX_PurchLine_SourceDocumentLine",
                table: "PurchLine");

            migrationBuilder.DropColumn(
                name: "SourceDocumentLine",
                table: "VendPackingSlipTrans");

            migrationBuilder.DropColumn(
                name: "SourceDocumentHeader",
                table: "VendPackingSlipJour");

            migrationBuilder.DropColumn(
                name: "SourceDocumentLine",
                table: "VendInvoiceTrans");

            migrationBuilder.DropColumn(
                name: "SourceDocumentHeader",
                table: "VendInvoiceJour");

            migrationBuilder.DropColumn(
                name: "SourceDocumentHeader",
                table: "VendInvoiceInfoTable");

            migrationBuilder.DropColumn(
                name: "SourceDocumentLine",
                table: "VendInvoiceInfoLine");

            migrationBuilder.DropColumn(
                name: "SourceDocumentHeader",
                table: "PurchTable");

            migrationBuilder.DropColumn(
                name: "SourceDocumentLine",
                table: "PurchLine");
        }
    }
}
