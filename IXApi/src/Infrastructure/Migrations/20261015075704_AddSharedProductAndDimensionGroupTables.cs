using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace IAX.IXApi.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddSharedProductAndDimensionGroupTables : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "EcoResProduct",
                columns: table => new
                {
                    RECID = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    DisplayProductNumber = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    ProductType = table.Column<int>(type: "int", nullable: false),
                    ProductSubtype = table.Column<int>(type: "int", nullable: false),
                    SearchName = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
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
                    table.PrimaryKey("PK_EcoResProduct", x => x.RECID);
                });

            migrationBuilder.CreateTable(
                name: "EcoResStorageDimensionGroup",
                columns: table => new
                {
                    RECID = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    StorageDimensionGroupId = table.Column<string>(type: "nvarchar(10)", maxLength: 10, nullable: false),
                    Name = table.Column<string>(type: "nvarchar(60)", maxLength: 60, nullable: false),
                    UseSite = table.Column<bool>(type: "bit", nullable: false),
                    UseWarehouse = table.Column<bool>(type: "bit", nullable: false),
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
                    table.PrimaryKey("PK_EcoResStorageDimensionGroup", x => x.RECID);
                });

            migrationBuilder.CreateTable(
                name: "EcoResStorageDimensionGroupItem",
                columns: table => new
                {
                    RECID = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ItemId = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    ItemDataAreaId = table.Column<string>(type: "nvarchar(4)", maxLength: 4, nullable: false),
                    StorageDimensionGroupId = table.Column<string>(type: "nvarchar(10)", maxLength: 10, nullable: false),
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
                    table.PrimaryKey("PK_EcoResStorageDimensionGroupItem", x => x.RECID);
                });

            migrationBuilder.CreateTable(
                name: "EcoResTrackingDimensionGroup",
                columns: table => new
                {
                    RECID = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    TrackingDimensionGroupId = table.Column<string>(type: "nvarchar(10)", maxLength: 10, nullable: false),
                    Name = table.Column<string>(type: "nvarchar(60)", maxLength: 60, nullable: false),
                    UseBatchNumber = table.Column<bool>(type: "bit", nullable: false),
                    UseSerialNumber = table.Column<bool>(type: "bit", nullable: false),
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
                    table.PrimaryKey("PK_EcoResTrackingDimensionGroup", x => x.RECID);
                });

            migrationBuilder.CreateTable(
                name: "EcoResTrackingDimensionGroupItem",
                columns: table => new
                {
                    RECID = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ItemId = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    ItemDataAreaId = table.Column<string>(type: "nvarchar(4)", maxLength: 4, nullable: false),
                    TrackingDimensionGroupId = table.Column<string>(type: "nvarchar(10)", maxLength: 10, nullable: false),
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
                    table.PrimaryKey("PK_EcoResTrackingDimensionGroupItem", x => x.RECID);
                });

            migrationBuilder.CreateTable(
                name: "EcoResProductTranslation",
                columns: table => new
                {
                    RECID = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Product = table.Column<long>(type: "bigint", nullable: false),
                    LanguageId = table.Column<string>(type: "nvarchar(7)", maxLength: 7, nullable: false),
                    Name = table.Column<string>(type: "nvarchar(60)", maxLength: 60, nullable: false),
                    Description = table.Column<string>(type: "nvarchar(max)", maxLength: 2147483647, nullable: false),
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
                    table.PrimaryKey("PK_EcoResProductTranslation", x => x.RECID);
                    table.ForeignKey(
                        name: "FK_EcoResProductTranslation_EcoResProduct_Product",
                        column: x => x.Product,
                        principalTable: "EcoResProduct",
                        principalColumn: "RECID",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_EcoResProduct_DisplayProductNumber",
                table: "EcoResProduct",
                column: "DisplayProductNumber",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_EcoResProductTranslation_Product_LanguageId",
                table: "EcoResProductTranslation",
                columns: new[] { "Product", "LanguageId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_EcoResStorageDimensionGroup_StorageDimensionGroupId",
                table: "EcoResStorageDimensionGroup",
                column: "StorageDimensionGroupId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_EcoResStorageDimensionGroupItem_ItemId_ItemDataAreaId",
                table: "EcoResStorageDimensionGroupItem",
                columns: new[] { "ItemId", "ItemDataAreaId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_EcoResStorageDimensionGroupItem_StorageDimensionGroupId",
                table: "EcoResStorageDimensionGroupItem",
                column: "StorageDimensionGroupId");

            migrationBuilder.CreateIndex(
                name: "IX_EcoResTrackingDimensionGroup_TrackingDimensionGroupId",
                table: "EcoResTrackingDimensionGroup",
                column: "TrackingDimensionGroupId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_EcoResTrackingDimensionGroupItem_ItemId_ItemDataAreaId",
                table: "EcoResTrackingDimensionGroupItem",
                columns: new[] { "ItemId", "ItemDataAreaId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_EcoResTrackingDimensionGroupItem_TrackingDimensionGroupId",
                table: "EcoResTrackingDimensionGroupItem",
                column: "TrackingDimensionGroupId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "EcoResProductTranslation");

            migrationBuilder.DropTable(
                name: "EcoResStorageDimensionGroup");

            migrationBuilder.DropTable(
                name: "EcoResStorageDimensionGroupItem");

            migrationBuilder.DropTable(
                name: "EcoResTrackingDimensionGroup");

            migrationBuilder.DropTable(
                name: "EcoResTrackingDimensionGroupItem");

            migrationBuilder.DropTable(
                name: "EcoResProduct");
        }
    }
}
