using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace IAX.IXApi.Infrastructure.Migrations;

public partial class AddProductIntegrityConstraints : Migration
{
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.CreateIndex(
            name: "IX_InventTable_DataAreaId_ItemId",
            table: "InventTable",
            columns: new[] { "DataAreaId", "ItemId" },
            unique: true);

        migrationBuilder.CreateIndex(
            name: "IX_InventTable_DataAreaId_Product",
            table: "InventTable",
            columns: new[] { "DataAreaId", "Product" },
            unique: true);

        migrationBuilder.CreateIndex(
            name: "IX_InventTableModule_DataAreaId_ItemId_ModuleType",
            table: "InventTableModule",
            columns: new[] { "DataAreaId", "ItemId", "ModuleType" },
            unique: true);

        migrationBuilder.AddForeignKey(
            name: "FK_InventTable_EcoResProduct_Product",
            table: "InventTable",
            column: "Product",
            principalTable: "EcoResProduct",
            principalColumn: "RECID",
            onDelete: ReferentialAction.Restrict);
    }

    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropForeignKey(
            name: "FK_InventTable_EcoResProduct_Product",
            table: "InventTable");

        migrationBuilder.DropIndex(name: "IX_InventTable_DataAreaId_ItemId", table: "InventTable");
        migrationBuilder.DropIndex(name: "IX_InventTable_DataAreaId_Product", table: "InventTable");
        migrationBuilder.DropIndex(name: "IX_InventTableModule_DataAreaId_ItemId_ModuleType", table: "InventTableModule");
    }
}
