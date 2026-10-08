using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace IAX.IXApi.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddSalesOrderAutoChargeAndOriginLinks : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "InventTransOriginSalesLine",
                columns: table => new
                {
                    RECID = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    InventTransOrigin = table.Column<long>(type: "bigint", nullable: false),
                    SalesLineDataAreaId = table.Column<string>(type: "nvarchar(4)", maxLength: 4, nullable: false),
                    SalesLineInventTransId = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
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
                    table.PrimaryKey("PK_InventTransOriginSalesLine", x => x.RECID);
                });

            migrationBuilder.CreateTable(
                name: "MarkupAutoLine",
                columns: table => new
                {
                    RECID = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    CurrencyCode = table.Column<string>(type: "nvarchar(3)", maxLength: 3, nullable: false),
                    CustomsAssessableValue_IN = table.Column<int>(type: "int", nullable: false),
                    FromAmount = table.Column<decimal>(type: "decimal(18,4)", precision: 18, scale: 4, nullable: false),
                    Keep = table.Column<int>(type: "int", nullable: false),
                    LineNum = table.Column<decimal>(type: "decimal(18,4)", precision: 18, scale: 4, nullable: false),
                    MarkupCategory = table.Column<int>(type: "int", nullable: false),
                    MarkupCode = table.Column<string>(type: "nvarchar(10)", maxLength: 10, nullable: false),
                    MarkupCurrencyCode = table.Column<string>(type: "nvarchar(3)", maxLength: 3, nullable: false),
                    MCRReturnMarkup = table.Column<int>(type: "int", nullable: false),
                    ModuleCategory = table.Column<int>(type: "int", nullable: false),
                    ModuleType = table.Column<int>(type: "int", nullable: false),
                    NotionalCharges_IN = table.Column<int>(type: "int", nullable: false),
                    NotionalPct_IN = table.Column<decimal>(type: "decimal(18,4)", precision: 18, scale: 4, nullable: false),
                    TableRecId = table.Column<long>(type: "bigint", nullable: false),
                    TableTableId = table.Column<int>(type: "int", nullable: false),
                    TaxGroup = table.Column<string>(type: "nvarchar(10)", maxLength: 10, nullable: false),
                    TaxItemGroup = table.Column<string>(type: "nvarchar(10)", maxLength: 10, nullable: false),
                    ToAmount = table.Column<decimal>(type: "decimal(18,4)", precision: 18, scale: 4, nullable: false),
                    Txt = table.Column<string>(type: "nvarchar(150)", maxLength: 150, nullable: false),
                    Value = table.Column<decimal>(type: "decimal(18,4)", precision: 18, scale: 4, nullable: false),
                    InventSiteId = table.Column<string>(type: "nvarchar(10)", maxLength: 10, nullable: false),
                    InventLocationId = table.Column<string>(type: "nvarchar(10)", maxLength: 10, nullable: false),
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
                    table.PrimaryKey("PK_MarkupAutoLine", x => x.RECID);
                });

            migrationBuilder.CreateTable(
                name: "MarkupAutoTable",
                columns: table => new
                {
                    RECID = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    AccountCode = table.Column<int>(type: "int", nullable: false),
                    AccountRelation = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    DlvModeCode = table.Column<int>(type: "int", nullable: false),
                    DlvModeRelation = table.Column<string>(type: "nvarchar(10)", maxLength: 10, nullable: false),
                    ItemCode = table.Column<int>(type: "int", nullable: false),
                    ItemRelation = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    MarkupReturn = table.Column<int>(type: "int", nullable: false),
                    ModuleCategory = table.Column<int>(type: "int", nullable: false),
                    ModuleType = table.Column<int>(type: "int", nullable: false),
                    ReturnRelation = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    RetailConcessionFeeLegacy = table.Column<int>(type: "int", nullable: false),
                    RetailConcessionFee = table.Column<int>(type: "int", nullable: false),
                    SHA256Hash = table.Column<string>(type: "nvarchar(64)", maxLength: 64, nullable: false),
                    RetailAdvancedChargesDeliveryProrate = table.Column<int>(type: "int", nullable: false),
                    RetailChannelCode = table.Column<int>(type: "int", nullable: false),
                    RetailChannelRelation = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    Description = table.Column<string>(type: "nvarchar(150)", maxLength: 150, nullable: false),
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
                    table.PrimaryKey("PK_MarkupAutoTable", x => x.RECID);
                });

            migrationBuilder.CreateIndex(
                name: "IX_InventTransOriginSalesLine_DataAreaId_InventTransOrigin",
                table: "InventTransOriginSalesLine",
                columns: new[] { "DataAreaId", "InventTransOrigin" });

            migrationBuilder.CreateIndex(
                name: "IX_InventTransOriginSalesLine_SalesLineDataAreaId_SalesLineInventTransId",
                table: "InventTransOriginSalesLine",
                columns: new[] { "SalesLineDataAreaId", "SalesLineInventTransId" });

            migrationBuilder.CreateIndex(
                name: "IX_MarkupAutoLine_DataAreaId_TableRecId_LineNum",
                table: "MarkupAutoLine",
                columns: new[] { "DataAreaId", "TableRecId", "LineNum" });

            migrationBuilder.CreateIndex(
                name: "IX_MarkupAutoTable_DataAreaId_ModuleType_AccountCode_AccountRelation_ItemCode_ItemRelation",
                table: "MarkupAutoTable",
                columns: new[] { "DataAreaId", "ModuleType", "AccountCode", "AccountRelation", "ItemCode", "ItemRelation" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "InventTransOriginSalesLine");

            migrationBuilder.DropTable(
                name: "MarkupAutoLine");

            migrationBuilder.DropTable(
                name: "MarkupAutoTable");
        }
    }
}
