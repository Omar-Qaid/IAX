using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace IAX.IXApi.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class VendInvoiceInfoSubTableAndPaymModeFields : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "AllowPaymentCopies",
                table: "VendPaymModeTable",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<string>(
                name: "BankTransType",
                table: "VendPaymModeTable",
                type: "nvarchar(10)",
                maxLength: 10,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<int>(
                name: "BridgingAccountByBank",
                table: "VendPaymModeTable",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<long>(
                name: "DimensionAttributeSet",
                table: "VendPaymModeTable",
                type: "bigint",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "DirectDebit",
                table: "VendPaymModeTable",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "DiscGraceDays",
                table: "VendPaymModeTable",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "ExportOnInvoice",
                table: "VendPaymModeTable",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "FurtherPosting",
                table: "VendPaymModeTable",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<long>(
                name: "InterCompanyLedgerDimension",
                table: "VendPaymModeTable",
                type: "bigint",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "LastSequenceNumDate",
                table: "VendPaymModeTable",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "LastSequenceNumToday",
                table: "VendPaymModeTable",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "LastSequenceNumber",
                table: "VendPaymModeTable",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "PaymGenerationLineLimit",
                table: "VendPaymModeTable",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "PaymOnInvoice",
                table: "VendPaymModeTable",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "PaymStatus",
                table: "VendPaymModeTable",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "PaymSumBy",
                table: "VendPaymModeTable",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "PdcClearingPosting",
                table: "VendPaymModeTable",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "TypeOfDraft",
                table: "VendPaymModeTable",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.CreateTable(
                name: "VendInvoiceInfoSubTable",
                columns: table => new
                {
                    RECID = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ParmId = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    TableRefId = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    OrigPurchId = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    PurchName = table.Column<string>(type: "nvarchar(60)", maxLength: 60, nullable: false),
                    SourceAccountingEvent = table.Column<long>(type: "bigint", nullable: false),
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
                    table.PrimaryKey("PK_VendInvoiceInfoSubTable", x => x.RECID);
                });

            migrationBuilder.CreateIndex(
                name: "IX_VendInvoiceInfoSubTable_DataAreaId_ParmId_OrigPurchId",
                table: "VendInvoiceInfoSubTable",
                columns: new[] { "DataAreaId", "ParmId", "OrigPurchId" },
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "VendInvoiceInfoSubTable");

            migrationBuilder.DropColumn(
                name: "AllowPaymentCopies",
                table: "VendPaymModeTable");

            migrationBuilder.DropColumn(
                name: "BankTransType",
                table: "VendPaymModeTable");

            migrationBuilder.DropColumn(
                name: "BridgingAccountByBank",
                table: "VendPaymModeTable");

            migrationBuilder.DropColumn(
                name: "DimensionAttributeSet",
                table: "VendPaymModeTable");

            migrationBuilder.DropColumn(
                name: "DirectDebit",
                table: "VendPaymModeTable");

            migrationBuilder.DropColumn(
                name: "DiscGraceDays",
                table: "VendPaymModeTable");

            migrationBuilder.DropColumn(
                name: "ExportOnInvoice",
                table: "VendPaymModeTable");

            migrationBuilder.DropColumn(
                name: "FurtherPosting",
                table: "VendPaymModeTable");

            migrationBuilder.DropColumn(
                name: "InterCompanyLedgerDimension",
                table: "VendPaymModeTable");

            migrationBuilder.DropColumn(
                name: "LastSequenceNumDate",
                table: "VendPaymModeTable");

            migrationBuilder.DropColumn(
                name: "LastSequenceNumToday",
                table: "VendPaymModeTable");

            migrationBuilder.DropColumn(
                name: "LastSequenceNumber",
                table: "VendPaymModeTable");

            migrationBuilder.DropColumn(
                name: "PaymGenerationLineLimit",
                table: "VendPaymModeTable");

            migrationBuilder.DropColumn(
                name: "PaymOnInvoice",
                table: "VendPaymModeTable");

            migrationBuilder.DropColumn(
                name: "PaymStatus",
                table: "VendPaymModeTable");

            migrationBuilder.DropColumn(
                name: "PaymSumBy",
                table: "VendPaymModeTable");

            migrationBuilder.DropColumn(
                name: "PdcClearingPosting",
                table: "VendPaymModeTable");

            migrationBuilder.DropColumn(
                name: "TypeOfDraft",
                table: "VendPaymModeTable");
        }
    }
}
