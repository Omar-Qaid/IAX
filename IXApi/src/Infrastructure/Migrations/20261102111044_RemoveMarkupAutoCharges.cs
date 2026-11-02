using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace IAX.IXApi.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class RemoveMarkupAutoCharges : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "MarkupAutoLine");

            migrationBuilder.DropTable(
                name: "MarkupAutoTable");

            migrationBuilder.DropColumn(
                name: "ItmCostTypeGroupId",
                table: "VendTable");

            migrationBuilder.DropColumn(
                name: "ItmImportCostingVendor",
                table: "VendTable");

            migrationBuilder.DropColumn(
                name: "ItmOverUnderToleranceGroupId",
                table: "VendTable");

            migrationBuilder.DropColumn(
                name: "ItmServicesProvider",
                table: "VendTable");

            migrationBuilder.DropColumn(
                name: "VendorPortalAdministratorRecId",
                table: "VendTable");

            migrationBuilder.DropColumn(
                name: "EmptyTaxBaseForOutgoingTax_W",
                table: "TaxTrans");

            migrationBuilder.DropColumn(
                name: "CalculatedAmountMst_W",
                table: "MarkupTrans");

            migrationBuilder.DropColumn(
                name: "MarkupAutoLineRecId",
                table: "MarkupTrans");

            migrationBuilder.DropColumn(
                name: "MarkupAutoTableRecId",
                table: "MarkupTrans");

            migrationBuilder.DropColumn(
                name: "McrCouponMarkup",
                table: "MarkupTrans");

            migrationBuilder.DropColumn(
                name: "McrInstallmentEligible",
                table: "MarkupTrans");

            migrationBuilder.DropColumn(
                name: "McrMarkupTransCreatedBy",
                table: "MarkupTrans");

            migrationBuilder.DropColumn(
                name: "McrMiscChargeOverride",
                table: "MarkupTrans");

            migrationBuilder.DropColumn(
                name: "McrOriginalMiscChargeValue",
                table: "MarkupTrans");

            migrationBuilder.DropColumn(
                name: "McrSavedRecId",
                table: "MarkupTrans");

            migrationBuilder.DropColumn(
                name: "McrSavedTableId",
                table: "MarkupTrans");

            migrationBuilder.DropColumn(
                name: "RetailShippingPromotionDiscount",
                table: "MarkupTrans");

            migrationBuilder.DropColumn(
                name: "TaxAmountMst_W",
                table: "MarkupTrans");

            migrationBuilder.DropColumn(
                name: "RetailMarketingOptIn",
                table: "LogisticsElectronicAddress");

            migrationBuilder.DropColumn(
                name: "Resident_W",
                table: "DirPartyTable");

            migrationBuilder.DropColumn(
                name: "RetailChannel",
                table: "DimensionAttributeValueCombination");

            migrationBuilder.DropColumn(
                name: "RetailChannelValue",
                table: "DimensionAttributeValueCombination");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "ItmCostTypeGroupId",
                table: "VendTable",
                type: "nvarchar(100)",
                maxLength: 100,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<int>(
                name: "ItmImportCostingVendor",
                table: "VendTable",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<string>(
                name: "ItmOverUnderToleranceGroupId",
                table: "VendTable",
                type: "nvarchar(10)",
                maxLength: 10,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<int>(
                name: "ItmServicesProvider",
                table: "VendTable",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<long>(
                name: "VendorPortalAdministratorRecId",
                table: "VendTable",
                type: "bigint",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "EmptyTaxBaseForOutgoingTax_W",
                table: "TaxTrans",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<decimal>(
                name: "CalculatedAmountMst_W",
                table: "MarkupTrans",
                type: "decimal(18,4)",
                precision: 18,
                scale: 4,
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<long>(
                name: "MarkupAutoLineRecId",
                table: "MarkupTrans",
                type: "bigint",
                nullable: false,
                defaultValue: 0L);

            migrationBuilder.AddColumn<long>(
                name: "MarkupAutoTableRecId",
                table: "MarkupTrans",
                type: "bigint",
                nullable: false,
                defaultValue: 0L);

            migrationBuilder.AddColumn<int>(
                name: "McrCouponMarkup",
                table: "MarkupTrans",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "McrInstallmentEligible",
                table: "MarkupTrans",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "McrMarkupTransCreatedBy",
                table: "MarkupTrans",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "McrMiscChargeOverride",
                table: "MarkupTrans",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<decimal>(
                name: "McrOriginalMiscChargeValue",
                table: "MarkupTrans",
                type: "decimal(18,4)",
                precision: 18,
                scale: 4,
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<long>(
                name: "McrSavedRecId",
                table: "MarkupTrans",
                type: "bigint",
                nullable: false,
                defaultValue: 0L);

            migrationBuilder.AddColumn<int>(
                name: "McrSavedTableId",
                table: "MarkupTrans",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<decimal>(
                name: "RetailShippingPromotionDiscount",
                table: "MarkupTrans",
                type: "decimal(18,4)",
                precision: 18,
                scale: 4,
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<decimal>(
                name: "TaxAmountMst_W",
                table: "MarkupTrans",
                type: "decimal(18,4)",
                precision: 18,
                scale: 4,
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<int>(
                name: "RetailMarketingOptIn",
                table: "LogisticsElectronicAddress",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "Resident_W",
                table: "DirPartyTable",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<long>(
                name: "RetailChannel",
                table: "DimensionAttributeValueCombination",
                type: "bigint",
                nullable: false,
                defaultValue: 0L);

            migrationBuilder.AddColumn<string>(
                name: "RetailChannelValue",
                table: "DimensionAttributeValueCombination",
                type: "nvarchar(30)",
                maxLength: 30,
                nullable: false,
                defaultValue: "");

            migrationBuilder.CreateTable(
                name: "MarkupAutoLine",
                columns: table => new
                {
                    RECID = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    CreatedBy = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    CurrencyCode = table.Column<string>(type: "nvarchar(3)", maxLength: 3, nullable: false),
                    CustomsAssessableValue_IN = table.Column<int>(type: "int", nullable: false),
                    DataAreaId = table.Column<string>(type: "nvarchar(4)", maxLength: 4, nullable: false),
                    FromAmount = table.Column<decimal>(type: "decimal(18,4)", precision: 18, scale: 4, nullable: false),
                    InventLocationId = table.Column<string>(type: "nvarchar(10)", maxLength: 10, nullable: false),
                    InventSiteId = table.Column<string>(type: "nvarchar(10)", maxLength: 10, nullable: false),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false),
                    Keep = table.Column<int>(type: "int", nullable: false),
                    LastModifiedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    LastModifiedBy = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    LineNum = table.Column<decimal>(type: "decimal(18,4)", precision: 18, scale: 4, nullable: false),
                    MCRReturnMarkup = table.Column<int>(type: "int", nullable: false),
                    MarkupCategory = table.Column<int>(type: "int", nullable: false),
                    MarkupCode = table.Column<string>(type: "nvarchar(10)", maxLength: 10, nullable: false),
                    MarkupCurrencyCode = table.Column<string>(type: "nvarchar(3)", maxLength: 3, nullable: false),
                    ModuleCategory = table.Column<int>(type: "int", nullable: false),
                    ModuleType = table.Column<int>(type: "int", nullable: false),
                    NotionalCharges_IN = table.Column<int>(type: "int", nullable: false),
                    NotionalPct_IN = table.Column<decimal>(type: "decimal(18,4)", precision: 18, scale: 4, nullable: false),
                    OwnerAccountId = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    RecVersion = table.Column<int>(type: "int", nullable: false),
                    RowVersion = table.Column<byte[]>(type: "rowversion", rowVersion: true, nullable: false),
                    TableRecId = table.Column<long>(type: "bigint", nullable: false),
                    TableTableId = table.Column<int>(type: "int", nullable: false),
                    TaxGroup = table.Column<string>(type: "nvarchar(10)", maxLength: 10, nullable: false),
                    TaxItemGroup = table.Column<string>(type: "nvarchar(10)", maxLength: 10, nullable: false),
                    ToAmount = table.Column<decimal>(type: "decimal(18,4)", precision: 18, scale: 4, nullable: false),
                    Txt = table.Column<string>(type: "nvarchar(150)", maxLength: 150, nullable: false),
                    Value = table.Column<decimal>(type: "decimal(18,4)", precision: 18, scale: 4, nullable: false)
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
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    CreatedBy = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    DataAreaId = table.Column<string>(type: "nvarchar(4)", maxLength: 4, nullable: false),
                    Description = table.Column<string>(type: "nvarchar(150)", maxLength: 150, nullable: false),
                    DlvModeCode = table.Column<int>(type: "int", nullable: false),
                    DlvModeRelation = table.Column<string>(type: "nvarchar(10)", maxLength: 10, nullable: false),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false),
                    ItemCode = table.Column<int>(type: "int", nullable: false),
                    ItemRelation = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    LastModifiedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    LastModifiedBy = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    MarkupReturn = table.Column<int>(type: "int", nullable: false),
                    ModuleCategory = table.Column<int>(type: "int", nullable: false),
                    ModuleType = table.Column<int>(type: "int", nullable: false),
                    OwnerAccountId = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    RecVersion = table.Column<int>(type: "int", nullable: false),
                    RetailAdvancedChargesDeliveryProrate = table.Column<int>(type: "int", nullable: false),
                    RetailChannelCode = table.Column<int>(type: "int", nullable: false),
                    RetailChannelRelation = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    RetailConcessionFee = table.Column<int>(type: "int", nullable: false),
                    RetailConcessionFeeLegacy = table.Column<int>(type: "int", nullable: false),
                    ReturnRelation = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    RowVersion = table.Column<byte[]>(type: "rowversion", rowVersion: true, nullable: false),
                    SHA256Hash = table.Column<string>(type: "nvarchar(64)", maxLength: 64, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_MarkupAutoTable", x => x.RECID);
                });

            migrationBuilder.CreateIndex(
                name: "IX_MarkupAutoLine_DataAreaId_TableRecId_LineNum",
                table: "MarkupAutoLine",
                columns: new[] { "DataAreaId", "TableRecId", "LineNum" });

            migrationBuilder.CreateIndex(
                name: "IX_MarkupAutoTable_DataAreaId_ModuleType_AccountCode_AccountRelation_ItemCode_ItemRelation",
                table: "MarkupAutoTable",
                columns: new[] { "DataAreaId", "ModuleType", "AccountCode", "AccountRelation", "ItemCode", "ItemRelation" });
        }
    }
}
