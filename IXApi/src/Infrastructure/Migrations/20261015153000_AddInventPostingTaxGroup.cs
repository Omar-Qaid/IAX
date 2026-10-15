using IAX.IXApi.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace IAX.IXApi.Infrastructure.Migrations;

[DbContext(typeof(ApplicationDbContext))]
[Migration("20261015153000_AddInventPostingTaxGroup")]
public partial class AddInventPostingTaxGroup : Migration
{
    protected override void Up(MigrationBuilder migrationBuilder) => migrationBuilder.AddColumn<string>(
        name: "TaxGroupId", table: "InventPosting", type: "nvarchar(10)", maxLength: 10, nullable: false, defaultValue: "");

    protected override void Down(MigrationBuilder migrationBuilder) => migrationBuilder.DropColumn(name: "TaxGroupId", table: "InventPosting");
}
