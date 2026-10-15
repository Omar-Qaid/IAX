using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace IAX.IXApi.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddItemOrderSetupTables : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "InventItemInventSetup",
                columns: table => new
                {
                    RECID = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ItemId = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    ItemDataAreaId = table.Column<string>(type: "nvarchar(4)", maxLength: 4, nullable: false),
                    InventDimId = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    InventDimIdDefault = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    ATPApplyDemandTimeFence = table.Column<int>(type: "int", nullable: true),
                    ATPApplySupplyTimeFence = table.Column<int>(type: "int", nullable: true),
                    ATPBackwardDemandTimeFence = table.Column<int>(type: "int", nullable: true),
                    ATPBackwardSupplyTimeFence = table.Column<int>(type: "int", nullable: true),
                    ATPInclPlannedOrders = table.Column<int>(type: "int", nullable: true),
                    ATPTimeFence = table.Column<int>(type: "int", nullable: true),
                    CalendarDays = table.Column<int>(type: "int", nullable: true),
                    DeliveryDateControlType = table.Column<int>(type: "int", nullable: true),
                    HighestQty = table.Column<decimal>(type: "decimal(18,4)", precision: 18, scale: 4, nullable: true),
                    LeadTime = table.Column<int>(type: "int", nullable: true),
                    LowestQty = table.Column<decimal>(type: "decimal(18,4)", precision: 18, scale: 4, nullable: true),
                    MandatoryInventLocation = table.Column<int>(type: "int", nullable: true),
                    MandatoryInventSite = table.Column<int>(type: "int", nullable: true),
                    MultipleQty = table.Column<decimal>(type: "decimal(18,4)", precision: 18, scale: 4, nullable: true),
                    Override = table.Column<int>(type: "int", nullable: true),
                    StandardQty = table.Column<decimal>(type: "decimal(18,4)", precision: 18, scale: 4, nullable: true),
                    Stopped = table.Column<int>(type: "int", nullable: true),
                    Sequence = table.Column<long>(type: "bigint", nullable: true),
                    OverrideDefaultStorageDimensions = table.Column<int>(type: "int", nullable: true),
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
                    table.PrimaryKey("PK_InventItemInventSetup", x => x.RECID);
                });

            migrationBuilder.CreateTable(
                name: "InventItemPurchSetup",
                columns: table => new
                {
                    RECID = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ItemId = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    ItemDataAreaId = table.Column<string>(type: "nvarchar(4)", maxLength: 4, nullable: false),
                    InventDimId = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    InventDimIdDefault = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    CalendarDays = table.Column<int>(type: "int", nullable: true),
                    HighestQty = table.Column<decimal>(type: "decimal(18,4)", precision: 18, scale: 4, nullable: true),
                    LeadTime = table.Column<int>(type: "int", nullable: true),
                    LowestQty = table.Column<decimal>(type: "decimal(18,4)", precision: 18, scale: 4, nullable: true),
                    MandatoryInventLocation = table.Column<int>(type: "int", nullable: true),
                    MandatoryInventSite = table.Column<int>(type: "int", nullable: true),
                    MultipleQty = table.Column<decimal>(type: "decimal(18,4)", precision: 18, scale: 4, nullable: true),
                    Override = table.Column<int>(type: "int", nullable: true),
                    StandardQty = table.Column<decimal>(type: "decimal(18,4)", precision: 18, scale: 4, nullable: true),
                    Stopped = table.Column<int>(type: "int", nullable: true),
                    Sequence = table.Column<long>(type: "bigint", nullable: true),
                    OverrideDefaultStorageDimensions = table.Column<int>(type: "int", nullable: true),
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
                    table.PrimaryKey("PK_InventItemPurchSetup", x => x.RECID);
                });

            migrationBuilder.CreateTable(
                name: "InventItemSalesSetup",
                columns: table => new
                {
                    RECID = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ItemId = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    ItemDataAreaId = table.Column<string>(type: "nvarchar(4)", maxLength: 4, nullable: false),
                    InventDimId = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    InventDimIdDefault = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    ATPApplyDemandTimeFence = table.Column<int>(type: "int", nullable: true),
                    ATPApplySupplyTimeFence = table.Column<int>(type: "int", nullable: true),
                    ATPBackwardDemandTimeFence = table.Column<int>(type: "int", nullable: true),
                    ATPBackwardSupplyTimeFence = table.Column<int>(type: "int", nullable: true),
                    ATPInclPlannedOrders = table.Column<int>(type: "int", nullable: true),
                    ATPTimeFence = table.Column<int>(type: "int", nullable: true),
                    CalendarDays = table.Column<int>(type: "int", nullable: true),
                    DeliveryDateControlType = table.Column<int>(type: "int", nullable: true),
                    HighestQty = table.Column<decimal>(type: "decimal(18,4)", precision: 18, scale: 4, nullable: true),
                    LeadTime = table.Column<int>(type: "int", nullable: true),
                    LowestQty = table.Column<decimal>(type: "decimal(18,4)", precision: 18, scale: 4, nullable: true),
                    MandatoryInventLocation = table.Column<int>(type: "int", nullable: true),
                    MandatoryInventSite = table.Column<int>(type: "int", nullable: true),
                    MultipleQty = table.Column<decimal>(type: "decimal(18,4)", precision: 18, scale: 4, nullable: true),
                    Override = table.Column<int>(type: "int", nullable: true),
                    OverrideSalesLeadTime = table.Column<int>(type: "int", nullable: true),
                    StandardQty = table.Column<decimal>(type: "decimal(18,4)", precision: 18, scale: 4, nullable: true),
                    Stopped = table.Column<int>(type: "int", nullable: true),
                    Sequence = table.Column<long>(type: "bigint", nullable: true),
                    OverrideDefaultStorageDimensions = table.Column<int>(type: "int", nullable: true),
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
                    table.PrimaryKey("PK_InventItemSalesSetup", x => x.RECID);
                });

            migrationBuilder.CreateIndex(
                name: "IX_InventItemInventSetup_ItemId_ItemDataAreaId_InventDimId",
                table: "InventItemInventSetup",
                columns: new[] { "ItemId", "ItemDataAreaId", "InventDimId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_InventItemPurchSetup_ItemId_ItemDataAreaId_InventDimId",
                table: "InventItemPurchSetup",
                columns: new[] { "ItemId", "ItemDataAreaId", "InventDimId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_InventItemSalesSetup_ItemId_ItemDataAreaId_InventDimId",
                table: "InventItemSalesSetup",
                columns: new[] { "ItemId", "ItemDataAreaId", "InventDimId" },
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "InventItemInventSetup");

            migrationBuilder.DropTable(
                name: "InventItemPurchSetup");

            migrationBuilder.DropTable(
                name: "InventItemSalesSetup");
        }
    }
}
