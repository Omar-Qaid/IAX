using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace IAX.IXApi.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddPendingVendorInvoiceTables : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "VendInvoiceInfoLine",
                columns: table => new
                {
                    RECID = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ParmId = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    TableRefId = table.Column<int>(type: "int", nullable: false),
                    OrigPurchId = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    LineNum = table.Column<decimal>(type: "decimal(18,6)", precision: 18, scale: 6, nullable: false),
                    ItemId = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    InventTransId = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    InventDimId = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    PurchUnit = table.Column<string>(type: "nvarchar(10)", maxLength: 10, nullable: false),
                    ReceiveNow = table.Column<decimal>(type: "decimal(18,6)", precision: 18, scale: 6, nullable: false),
                    PurchPrice = table.Column<decimal>(type: "decimal(18,4)", precision: 18, scale: 4, nullable: false),
                    LineAmount = table.Column<decimal>(type: "decimal(18,4)", precision: 18, scale: 4, nullable: false),
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
                    table.PrimaryKey("PK_VendInvoiceInfoLine", x => x.RECID);
                });

            migrationBuilder.CreateTable(
                name: "VendInvoiceInfoTable",
                columns: table => new
                {
                    RECID = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ParmId = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    TableRefId = table.Column<int>(type: "int", nullable: false),
                    Num = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    InvoiceAccount = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    OrderAccount = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    DocumentDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    TransDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CurrencyCode = table.Column<string>(type: "nvarchar(3)", maxLength: 3, nullable: false),
                    Description = table.Column<string>(type: "nvarchar(60)", maxLength: 60, nullable: false),
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
                    table.PrimaryKey("PK_VendInvoiceInfoTable", x => x.RECID);
                });

            migrationBuilder.CreateIndex(
                name: "IX_VendInvoiceInfoLine_DataAreaId_ParmId",
                table: "VendInvoiceInfoLine",
                columns: new[] { "DataAreaId", "ParmId" });

            migrationBuilder.CreateIndex(
                name: "IX_VendInvoiceInfoLine_DataAreaId_ParmId_OrigPurchId_InventTransId",
                table: "VendInvoiceInfoLine",
                columns: new[] { "DataAreaId", "ParmId", "OrigPurchId", "InventTransId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_VendInvoiceInfoTable_DataAreaId_InvoiceAccount_Num",
                table: "VendInvoiceInfoTable",
                columns: new[] { "DataAreaId", "InvoiceAccount", "Num" });

            migrationBuilder.CreateIndex(
                name: "IX_VendInvoiceInfoTable_DataAreaId_ParmId",
                table: "VendInvoiceInfoTable",
                columns: new[] { "DataAreaId", "ParmId" },
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "VendInvoiceInfoLine");

            migrationBuilder.DropTable(
                name: "VendInvoiceInfoTable");
        }
    }
}
