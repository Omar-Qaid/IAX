using IAX.IXApi.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace IAX.IXApi.Infrastructure.Migrations;

[DbContext(typeof(ApplicationDbContext))]
[Migration("20261013130000_RemoveInvalidPriceDiscForeignKeys")]
public sealed class RemoveInvalidPriceDiscForeignKeys : Migration
{
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropForeignKey("FK_PriceDiscTable_CustTable_AccountRelation", "PriceDiscTable");
        migrationBuilder.DropForeignKey("FK_PriceDiscTable_InventTable_ItemRelation", "PriceDiscTable");
        migrationBuilder.DropForeignKey("FK_PriceDiscTable_InventDim_InventDimId", "PriceDiscTable");
    }

    // Restoring unconditional constraints would reject valid group, all, and
    // dimension-independent agreements that may have been entered after Up.
    protected override void Down(MigrationBuilder migrationBuilder) { }
}
