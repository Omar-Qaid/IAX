using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace IAX.IXApi.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class PurchaseInvoiceTaxAndReceiptMatching : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<long>(
                name: "OriginalInvoiceTransRecId",
                table: "VendInvoiceTrans",
                type: "bigint",
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "TaxAmount",
                table: "VendInvoiceTrans",
                type: "decimal(18,4)",
                precision: 18,
                scale: 4,
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<string>(
                name: "TaxGroup",
                table: "VendInvoiceTrans",
                type: "nvarchar(30)",
                maxLength: 30,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "TaxItemGroup",
                table: "VendInvoiceTrans",
                type: "nvarchar(30)",
                maxLength: 30,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<DateTime>(
                name: "DueDate",
                table: "VendInvoiceJour",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "SumTax",
                table: "VendInvoiceJour",
                type: "decimal(18,4)",
                precision: 18,
                scale: 4,
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<string>(
                name: "TaxGroup",
                table: "PurchTable",
                type: "nvarchar(30)",
                maxLength: 30,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<long>(
                name: "OriginalInvoiceTransRecId",
                table: "PurchLine",
                type: "bigint",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "TaxGroup",
                table: "PurchLine",
                type: "nvarchar(30)",
                maxLength: 30,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "TaxItemGroup",
                table: "PurchLine",
                type: "nvarchar(30)",
                maxLength: 30,
                nullable: false,
                defaultValue: "");

            migrationBuilder.CreateTable(
                name: "VendInvoicePackingSlipQuantityMatch",
                columns: table => new
                {
                    RECID = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    InvoiceTransRecId = table.Column<long>(type: "bigint", nullable: false),
                    PackingSlipTransRecId = table.Column<long>(type: "bigint", nullable: false),
                    Quantity = table.Column<decimal>(type: "decimal(18,6)", precision: 18, scale: 6, nullable: false),
                    InventQuantity = table.Column<decimal>(type: "decimal(18,6)", precision: 18, scale: 6, nullable: false),
                    CreatedBy = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    LastModifiedBy = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    LastModifiedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    OwnerAccountId = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    RowVersion = table.Column<byte[]>(type: "rowversion", rowVersion: true, nullable: false),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    RecVersion = table.Column<int>(type: "int", nullable: false),
                    DataAreaId = table.Column<string>(type: "nvarchar(4)", maxLength: 4, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_VendInvoicePackingSlipQuantityMatch", x => x.RECID);
                    table.ForeignKey(
                        name: "FK_VendInvoicePackingSlipQuantityMatch_VendInvoiceTrans_InvoiceTransRecId",
                        column: x => x.InvoiceTransRecId,
                        principalTable: "VendInvoiceTrans",
                        principalColumn: "RECID",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_VendInvoicePackingSlipQuantityMatch_VendPackingSlipTrans_PackingSlipTransRecId",
                        column: x => x.PackingSlipTransRecId,
                        principalTable: "VendPackingSlipTrans",
                        principalColumn: "RECID",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_VendInvoiceTrans_OriginalInvoiceTransRecId",
                table: "VendInvoiceTrans",
                column: "OriginalInvoiceTransRecId");

            migrationBuilder.CreateIndex(
                name: "IX_PurchLine_OriginalInvoiceTransRecId",
                table: "PurchLine",
                column: "OriginalInvoiceTransRecId");

            migrationBuilder.CreateIndex(
                name: "IX_VendInvoicePackingSlipQuantityMatch_DataAreaId_InvoiceTransRecId_PackingSlipTransRecId",
                table: "VendInvoicePackingSlipQuantityMatch",
                columns: new[] { "DataAreaId", "InvoiceTransRecId", "PackingSlipTransRecId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_VendInvoicePackingSlipQuantityMatch_InvoiceTransRecId",
                table: "VendInvoicePackingSlipQuantityMatch",
                column: "InvoiceTransRecId");

            migrationBuilder.CreateIndex(
                name: "IX_VendInvoicePackingSlipQuantityMatch_PackingSlipTransRecId",
                table: "VendInvoicePackingSlipQuantityMatch",
                column: "PackingSlipTransRecId");

            migrationBuilder.AddForeignKey(
                name: "FK_PurchLine_VendInvoiceTrans_OriginalInvoiceTransRecId",
                table: "PurchLine",
                column: "OriginalInvoiceTransRecId",
                principalTable: "VendInvoiceTrans",
                principalColumn: "RECID",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_VendInvoiceTrans_VendInvoiceTrans_OriginalInvoiceTransRecId",
                table: "VendInvoiceTrans",
                column: "OriginalInvoiceTransRecId",
                principalTable: "VendInvoiceTrans",
                principalColumn: "RECID",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_PurchLine_VendInvoiceTrans_OriginalInvoiceTransRecId",
                table: "PurchLine");

            migrationBuilder.DropForeignKey(
                name: "FK_VendInvoiceTrans_VendInvoiceTrans_OriginalInvoiceTransRecId",
                table: "VendInvoiceTrans");

            migrationBuilder.DropTable(
                name: "VendInvoicePackingSlipQuantityMatch");

            migrationBuilder.DropIndex(
                name: "IX_VendInvoiceTrans_OriginalInvoiceTransRecId",
                table: "VendInvoiceTrans");

            migrationBuilder.DropIndex(
                name: "IX_PurchLine_OriginalInvoiceTransRecId",
                table: "PurchLine");

            migrationBuilder.DropColumn(
                name: "OriginalInvoiceTransRecId",
                table: "VendInvoiceTrans");

            migrationBuilder.DropColumn(
                name: "TaxAmount",
                table: "VendInvoiceTrans");

            migrationBuilder.DropColumn(
                name: "TaxGroup",
                table: "VendInvoiceTrans");

            migrationBuilder.DropColumn(
                name: "TaxItemGroup",
                table: "VendInvoiceTrans");

            migrationBuilder.DropColumn(
                name: "DueDate",
                table: "VendInvoiceJour");

            migrationBuilder.DropColumn(
                name: "SumTax",
                table: "VendInvoiceJour");

            migrationBuilder.DropColumn(
                name: "TaxGroup",
                table: "PurchTable");

            migrationBuilder.DropColumn(
                name: "OriginalInvoiceTransRecId",
                table: "PurchLine");

            migrationBuilder.DropColumn(
                name: "TaxGroup",
                table: "PurchLine");

            migrationBuilder.DropColumn(
                name: "TaxItemGroup",
                table: "PurchLine");
        }
    }
}
