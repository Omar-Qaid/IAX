using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace IAX.IXApi.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AlignStandardDimensionGroupTables : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("""
                SELECT RECID, StorageDimensionGroupId, Name, UseSite, UseWarehouse, UseLocation
                INTO #StorageDimensionGroupBackup FROM EcoResStorageDimensionGroup;
                SELECT RECID, ItemId, ItemDataAreaId, StorageDimensionGroupId
                INTO #StorageDimensionGroupItemBackup FROM EcoResStorageDimensionGroupItem;
                SELECT RECID, TrackingDimensionGroupId, Name, UseBatchNumber, UseSerialNumber
                INTO #TrackingDimensionGroupBackup FROM EcoResTrackingDimensionGroup;
                SELECT RECID, ItemId, ItemDataAreaId, TrackingDimensionGroupId
                INTO #TrackingDimensionGroupItemBackup FROM EcoResTrackingDimensionGroupItem;
                """);
            migrationBuilder.DropIndex(
                name: "IX_EcoResTrackingDimensionGroupItem_TrackingDimensionGroupId",
                table: "EcoResTrackingDimensionGroupItem");

            migrationBuilder.DropIndex(
                name: "IX_EcoResTrackingDimensionGroup_TrackingDimensionGroupId",
                table: "EcoResTrackingDimensionGroup");

            migrationBuilder.DropIndex(
                name: "IX_EcoResStorageDimensionGroupItem_StorageDimensionGroupId",
                table: "EcoResStorageDimensionGroupItem");

            migrationBuilder.DropIndex(
                name: "IX_EcoResStorageDimensionGroup_StorageDimensionGroupId",
                table: "EcoResStorageDimensionGroup");

            migrationBuilder.DropColumn(
                name: "TrackingDimensionGroupId",
                table: "EcoResTrackingDimensionGroupItem");

            migrationBuilder.DropColumn(
                name: "TrackingDimensionGroupId",
                table: "EcoResTrackingDimensionGroup");

            migrationBuilder.DropColumn(
                name: "UseBatchNumber",
                table: "EcoResTrackingDimensionGroup");

            migrationBuilder.DropColumn(
                name: "UseSerialNumber",
                table: "EcoResTrackingDimensionGroup");

            migrationBuilder.DropColumn(
                name: "StorageDimensionGroupId",
                table: "EcoResStorageDimensionGroupItem");

            migrationBuilder.DropColumn(
                name: "StorageDimensionGroupId",
                table: "EcoResStorageDimensionGroup");

            migrationBuilder.DropColumn(
                name: "UseLocation",
                table: "EcoResStorageDimensionGroup");

            migrationBuilder.DropColumn(
                name: "UseSite",
                table: "EcoResStorageDimensionGroup");

            migrationBuilder.DropColumn(
                name: "UseWarehouse",
                table: "EcoResStorageDimensionGroup");

            migrationBuilder.AddColumn<long>(
                name: "TrackingDimensionGroup",
                table: "EcoResTrackingDimensionGroupItem",
                type: "bigint",
                nullable: false,
                defaultValue: 0L);

            migrationBuilder.AddColumn<string>(
                name: "Description",
                table: "EcoResTrackingDimensionGroup",
                type: "nvarchar(60)",
                maxLength: 60,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<long>(
                name: "StorageDimensionGroup",
                table: "EcoResStorageDimensionGroupItem",
                type: "bigint",
                nullable: false,
                defaultValue: 0L);

            migrationBuilder.AddColumn<string>(
                name: "Description",
                table: "EcoResStorageDimensionGroup",
                type: "nvarchar(60)",
                maxLength: 60,
                nullable: false,
                defaultValue: "");

            migrationBuilder.CreateTable(
                name: "EcoResStorageDimensionGroupFldSetup",
                columns: table => new
                {
                    RECID = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    StorageDimensionGroup = table.Column<long>(type: "bigint", nullable: false),
                    DimensionFieldId = table.Column<int>(type: "int", nullable: false),
                    LineNumber = table.Column<decimal>(type: "decimal(18,4)", precision: 18, scale: 4, nullable: false),
                    IsAllowBlankReceiptEnabled = table.Column<bool>(type: "bit", nullable: false),
                    IsAllowBlankIssueEnabled = table.Column<bool>(type: "bit", nullable: false),
                    IsPhysicalInventoryEnabled = table.Column<bool>(type: "bit", nullable: false),
                    IsFinancialInventoryEnabled = table.Column<bool>(type: "bit", nullable: false),
                    IsCoveragePlanByDimensionEnabled = table.Column<bool>(type: "bit", nullable: false),
                    IsPurchPriceSearchEnabled = table.Column<bool>(type: "bit", nullable: false),
                    IsSalesPriceSearchEnabled = table.Column<bool>(type: "bit", nullable: false),
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
                    table.PrimaryKey("PK_EcoResStorageDimensionGroupFldSetup", x => x.RECID);
                });

            migrationBuilder.CreateTable(
                name: "EcoResTrackingDimensionGroupFldSetup",
                columns: table => new
                {
                    RECID = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    TrackingDimensionGroup = table.Column<long>(type: "bigint", nullable: false),
                    DimensionFieldId = table.Column<int>(type: "int", nullable: false),
                    LineNumber = table.Column<decimal>(type: "decimal(18,4)", precision: 18, scale: 4, nullable: false),
                    IsAllowBlankReceiptEnabled = table.Column<bool>(type: "bit", nullable: false),
                    IsAllowBlankIssueEnabled = table.Column<bool>(type: "bit", nullable: false),
                    IsPhysicalInventoryEnabled = table.Column<bool>(type: "bit", nullable: false),
                    IsFinancialInventoryEnabled = table.Column<bool>(type: "bit", nullable: false),
                    IsCoveragePlanByDimensionEnabled = table.Column<bool>(type: "bit", nullable: false),
                    IsPrimaryStockingEnabled = table.Column<bool>(type: "bit", nullable: false),
                    IsPurchPriceSearchEnabled = table.Column<bool>(type: "bit", nullable: false),
                    IsSalesPriceSearchEnabled = table.Column<bool>(type: "bit", nullable: false),
                    IsSalesProcessActivated = table.Column<bool>(type: "bit", nullable: false),
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
                    table.PrimaryKey("PK_EcoResTrackingDimensionGroupFldSetup", x => x.RECID);
                });

            migrationBuilder.CreateIndex(
                name: "IX_EcoResTrackingDimensionGroupItem_TrackingDimensionGroup",
                table: "EcoResTrackingDimensionGroupItem",
                column: "TrackingDimensionGroup");

            migrationBuilder.CreateIndex(
                name: "IX_EcoResTrackingDimensionGroup_Name",
                table: "EcoResTrackingDimensionGroup",
                column: "Name",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_EcoResStorageDimensionGroupItem_StorageDimensionGroup",
                table: "EcoResStorageDimensionGroupItem",
                column: "StorageDimensionGroup");

            migrationBuilder.CreateIndex(
                name: "IX_EcoResStorageDimensionGroup_Name",
                table: "EcoResStorageDimensionGroup",
                column: "Name",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_EcoResStorageDimensionGroupFldSetup_StorageDimensionGroup_DimensionFieldId",
                table: "EcoResStorageDimensionGroupFldSetup",
                columns: new[] { "StorageDimensionGroup", "DimensionFieldId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_EcoResStorageDimensionGroupFldSetup_StorageDimensionGroup_LineNumber",
                table: "EcoResStorageDimensionGroupFldSetup",
                columns: new[] { "StorageDimensionGroup", "LineNumber" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_EcoResTrackingDimensionGroupFldSetup_TrackingDimensionGroup_DimensionFieldId",
                table: "EcoResTrackingDimensionGroupFldSetup",
                columns: new[] { "TrackingDimensionGroup", "DimensionFieldId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_EcoResTrackingDimensionGroupFldSetup_TrackingDimensionGroup_LineNumber",
                table: "EcoResTrackingDimensionGroupFldSetup",
                columns: new[] { "TrackingDimensionGroup", "LineNumber" },
                unique: true);

            migrationBuilder.Sql("""
                UPDATE target SET target.Name = source.StorageDimensionGroupId, target.Description = source.Name
                FROM EcoResStorageDimensionGroup target
                INNER JOIN #StorageDimensionGroupBackup source ON source.RECID = target.RECID;

                UPDATE target SET target.StorageDimensionGroup = header.RECID
                FROM EcoResStorageDimensionGroupItem target
                INNER JOIN #StorageDimensionGroupItemBackup source ON source.RECID = target.RECID
                INNER JOIN #StorageDimensionGroupBackup oldHeader ON oldHeader.StorageDimensionGroupId = source.StorageDimensionGroupId
                INNER JOIN EcoResStorageDimensionGroup header ON header.RECID = oldHeader.RECID;

                INSERT INTO EcoResStorageDimensionGroupFldSetup
                    (StorageDimensionGroup, DimensionFieldId, LineNumber,
                     IsAllowBlankReceiptEnabled, IsAllowBlankIssueEnabled,
                     IsPhysicalInventoryEnabled, IsFinancialInventoryEnabled,
                     IsCoveragePlanByDimensionEnabled, IsPurchPriceSearchEnabled,
                     IsSalesPriceSearchEnabled, IsActive, IsDeleted, RecVersion, DataAreaId)
                SELECT RECID, fieldId, lineNumber,
                       0, 0, 0, 0, 0, 0, 0,
                       isActive, 0, 1, ''
                FROM #StorageDimensionGroupBackup
                CROSS APPLY (VALUES (1, CAST(1 AS decimal(18,4)), UseSite),
                                    (2, CAST(2 AS decimal(18,4)), UseWarehouse),
                                    (3, CAST(3 AS decimal(18,4)), UseLocation)) setup(fieldId, lineNumber, isActive);

                UPDATE target SET target.Name = source.TrackingDimensionGroupId, target.Description = source.Name
                FROM EcoResTrackingDimensionGroup target
                INNER JOIN #TrackingDimensionGroupBackup source ON source.RECID = target.RECID;

                UPDATE target SET target.TrackingDimensionGroup = header.RECID
                FROM EcoResTrackingDimensionGroupItem target
                INNER JOIN #TrackingDimensionGroupItemBackup source ON source.RECID = target.RECID
                INNER JOIN #TrackingDimensionGroupBackup oldHeader ON oldHeader.TrackingDimensionGroupId = source.TrackingDimensionGroupId
                INNER JOIN EcoResTrackingDimensionGroup header ON header.RECID = oldHeader.RECID;

                INSERT INTO EcoResTrackingDimensionGroupFldSetup
                    (TrackingDimensionGroup, DimensionFieldId, LineNumber,
                     IsAllowBlankReceiptEnabled, IsAllowBlankIssueEnabled,
                     IsPhysicalInventoryEnabled, IsFinancialInventoryEnabled,
                     IsCoveragePlanByDimensionEnabled, IsPrimaryStockingEnabled,
                     IsPurchPriceSearchEnabled, IsSalesPriceSearchEnabled,
                     IsSalesProcessActivated, IsActive, IsDeleted, RecVersion, DataAreaId)
                SELECT RECID, fieldId, lineNumber,
                       0, 0, 0, 0, 0, 0, 0, 0, 0,
                       isActive, 0, 1, ''
                FROM #TrackingDimensionGroupBackup
                CROSS APPLY (VALUES (4, CAST(1 AS decimal(18,4)), UseBatchNumber),
                                    (5, CAST(2 AS decimal(18,4)), UseSerialNumber)) setup(fieldId, lineNumber, isActive);
                """);

            migrationBuilder.DropColumn(name: "ProductSubtype", table: "EcoResProduct");
            migrationBuilder.DropColumn(name: "Sku", table: "InventTable");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(name: "ProductSubtype", table: "EcoResProduct", type: "int", nullable: false, defaultValue: 0);
            migrationBuilder.AddColumn<string>(name: "Sku", table: "InventTable", type: "nvarchar(40)", maxLength: 40, nullable: false, defaultValue: "");
            migrationBuilder.DropTable(
                name: "EcoResStorageDimensionGroupFldSetup");

            migrationBuilder.DropTable(
                name: "EcoResTrackingDimensionGroupFldSetup");

            migrationBuilder.DropIndex(
                name: "IX_EcoResTrackingDimensionGroupItem_TrackingDimensionGroup",
                table: "EcoResTrackingDimensionGroupItem");

            migrationBuilder.DropIndex(
                name: "IX_EcoResTrackingDimensionGroup_Name",
                table: "EcoResTrackingDimensionGroup");

            migrationBuilder.DropIndex(
                name: "IX_EcoResStorageDimensionGroupItem_StorageDimensionGroup",
                table: "EcoResStorageDimensionGroupItem");

            migrationBuilder.DropIndex(
                name: "IX_EcoResStorageDimensionGroup_Name",
                table: "EcoResStorageDimensionGroup");

            migrationBuilder.DropColumn(
                name: "TrackingDimensionGroup",
                table: "EcoResTrackingDimensionGroupItem");

            migrationBuilder.DropColumn(
                name: "Description",
                table: "EcoResTrackingDimensionGroup");

            migrationBuilder.DropColumn(
                name: "StorageDimensionGroup",
                table: "EcoResStorageDimensionGroupItem");

            migrationBuilder.DropColumn(
                name: "Description",
                table: "EcoResStorageDimensionGroup");

            migrationBuilder.AddColumn<string>(
                name: "TrackingDimensionGroupId",
                table: "EcoResTrackingDimensionGroupItem",
                type: "nvarchar(10)",
                maxLength: 10,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "TrackingDimensionGroupId",
                table: "EcoResTrackingDimensionGroup",
                type: "nvarchar(10)",
                maxLength: 10,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<bool>(
                name: "UseBatchNumber",
                table: "EcoResTrackingDimensionGroup",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "UseSerialNumber",
                table: "EcoResTrackingDimensionGroup",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<string>(
                name: "StorageDimensionGroupId",
                table: "EcoResStorageDimensionGroupItem",
                type: "nvarchar(10)",
                maxLength: 10,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "StorageDimensionGroupId",
                table: "EcoResStorageDimensionGroup",
                type: "nvarchar(10)",
                maxLength: 10,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<bool>(
                name: "UseLocation",
                table: "EcoResStorageDimensionGroup",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "UseSite",
                table: "EcoResStorageDimensionGroup",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "UseWarehouse",
                table: "EcoResStorageDimensionGroup",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.CreateIndex(
                name: "IX_EcoResTrackingDimensionGroupItem_TrackingDimensionGroupId",
                table: "EcoResTrackingDimensionGroupItem",
                column: "TrackingDimensionGroupId");

            migrationBuilder.CreateIndex(
                name: "IX_EcoResTrackingDimensionGroup_TrackingDimensionGroupId",
                table: "EcoResTrackingDimensionGroup",
                column: "TrackingDimensionGroupId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_EcoResStorageDimensionGroupItem_StorageDimensionGroupId",
                table: "EcoResStorageDimensionGroupItem",
                column: "StorageDimensionGroupId");

            migrationBuilder.CreateIndex(
                name: "IX_EcoResStorageDimensionGroup_StorageDimensionGroupId",
                table: "EcoResStorageDimensionGroup",
                column: "StorageDimensionGroupId",
                unique: true);
        }
    }
}
