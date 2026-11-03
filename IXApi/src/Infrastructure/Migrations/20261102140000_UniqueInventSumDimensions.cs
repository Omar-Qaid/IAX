using IAX.IXApi.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace IAX.IXApi.Infrastructure.Migrations;

[DbContext(typeof(ApplicationDbContext))]
[Migration("20261102140000_UniqueInventSumDimensions")]
public partial class UniqueInventSumDimensions : Migration
{
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.CreateIndex(
            name: "IX_InventSum_DataAreaId_ItemId_InventDimId",
            table: "InventSum",
            columns: new[] { "DataAreaId", "ItemId", "InventDimId" },
            unique: true);
    }

    protected override void Down(MigrationBuilder migrationBuilder) =>
        migrationBuilder.DropIndex(name: "IX_InventSum_DataAreaId_ItemId_InventDimId", table: "InventSum");
}
