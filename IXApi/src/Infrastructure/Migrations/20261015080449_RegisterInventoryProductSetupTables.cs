using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace IAX.IXApi.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class RegisterInventoryProductSetupTables : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "InventItemGroupItem",
                columns: table => new
                {
                    RECID = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ItemId = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    ItemGroupId = table.Column<string>(type: "nvarchar(10)", maxLength: 10, nullable: false),
                    ItemDataAreaId = table.Column<string>(type: "nvarchar(4)", maxLength: 4, nullable: false),
                    ItemGroupDataAreaId = table.Column<string>(type: "nvarchar(4)", maxLength: 4, nullable: false),
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
                    table.PrimaryKey("PK_InventItemGroupItem", x => x.RECID);
                });

            migrationBuilder.CreateTable(
                name: "InventModelGroup",
                columns: table => new
                {
                    RECID = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ModelGroupId = table.Column<string>(type: "nvarchar(10)", maxLength: 10, nullable: false),
                    Name = table.Column<string>(type: "nvarchar(60)", maxLength: 60, nullable: false),
                    InventModel = table.Column<int>(type: "int", nullable: false),
                    StockedProduct = table.Column<int>(type: "int", nullable: false),
                    PostOnHandPhysical = table.Column<int>(type: "int", nullable: false),
                    PostOnHandFinancial = table.Column<int>(type: "int", nullable: false),
                    PostPhysicalRevenue = table.Column<int>(type: "int", nullable: false),
                    PostPhysicalPurchase = table.Column<int>(type: "int", nullable: false),
                    StandardCost = table.Column<int>(type: "int", nullable: false),
                    MandatoryRegister = table.Column<int>(type: "int", nullable: false),
                    MandatoryReceive = table.Column<int>(type: "int", nullable: false),
                    MandatoryPick = table.Column<int>(type: "int", nullable: false),
                    MandatoryDeduct = table.Column<int>(type: "int", nullable: false),
                    MandatoryWmsOrder = table.Column<int>(type: "int", nullable: false),
                    NegativePhysical = table.Column<int>(type: "int", nullable: false),
                    NegativeFinancial = table.Column<int>(type: "int", nullable: false),
                    QuarantineControl = table.Column<int>(type: "int", nullable: false),
                    InclPhysicalValueInCost = table.Column<int>(type: "int", nullable: false),
                    InventCostRecalculationIncludePhysicalValueForAverageModel = table.Column<int>(type: "int", nullable: false),
                    ItemProdReservationActive = table.Column<int>(type: "int", nullable: false),
                    ItemProdReservation = table.Column<int>(type: "int", nullable: false),
                    ReserveByDate = table.Column<int>(type: "int", nullable: false),
                    ReserveReversed = table.Column<int>(type: "int", nullable: false),
                    McrReservation = table.Column<int>(type: "int", nullable: false),
                    PdsDispositionCode = table.Column<string>(type: "nvarchar(10)", maxLength: 10, nullable: false),
                    PdsCheck = table.Column<int>(type: "int", nullable: false),
                    PdsConsReq = table.Column<int>(type: "int", nullable: false),
                    PdsReqVendBatchDetail = table.Column<int>(type: "int", nullable: false),
                    PdsSameLot = table.Column<int>(type: "int", nullable: false),
                    PdsVendorCheckItem = table.Column<int>(type: "int", nullable: false),
                    PdsPickCriteria = table.Column<int>(type: "int", nullable: false),
                    PickingListBatchExpirationDateValidationRule = table.Column<int>(type: "int", nullable: false),
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
                    table.PrimaryKey("PK_InventModelGroup", x => x.RECID);
                });

            migrationBuilder.CreateTable(
                name: "InventModelGroupItem",
                columns: table => new
                {
                    RECID = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ItemId = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    ModelGroupId = table.Column<string>(type: "nvarchar(10)", maxLength: 10, nullable: false),
                    ItemDataAreaId = table.Column<string>(type: "nvarchar(4)", maxLength: 4, nullable: false),
                    ModelGroupDataAreaId = table.Column<string>(type: "nvarchar(4)", maxLength: 4, nullable: false),
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
                    table.PrimaryKey("PK_InventModelGroupItem", x => x.RECID);
                });

            migrationBuilder.CreateIndex(
                name: "IX_InventItemGroupItem_ItemGroupId_ItemGroupDataAreaId",
                table: "InventItemGroupItem",
                columns: new[] { "ItemGroupId", "ItemGroupDataAreaId" });

            migrationBuilder.CreateIndex(
                name: "IX_InventItemGroupItem_ItemId_ItemDataAreaId",
                table: "InventItemGroupItem",
                columns: new[] { "ItemId", "ItemDataAreaId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_InventModelGroup_ModelGroupId_DataAreaId",
                table: "InventModelGroup",
                columns: new[] { "ModelGroupId", "DataAreaId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_InventModelGroupItem_ItemId_ItemDataAreaId",
                table: "InventModelGroupItem",
                columns: new[] { "ItemId", "ItemDataAreaId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_InventModelGroupItem_ModelGroupId_ModelGroupDataAreaId",
                table: "InventModelGroupItem",
                columns: new[] { "ModelGroupId", "ModelGroupDataAreaId" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "InventItemGroupItem");

            migrationBuilder.DropTable(
                name: "InventModelGroup");

            migrationBuilder.DropTable(
                name: "InventModelGroupItem");
        }
    }
}
