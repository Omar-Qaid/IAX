using IAX.IXApi.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace IAX.IXApi.Infrastructure.Migrations;

[DbContext(typeof(ApplicationDbContext))]
[Migration("20261015161000_AddInventItemGroupForm")]
public partial class AddInventItemGroupForm : Migration
{
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.CreateTable(
            name: "InventItemGroupForm",
            columns: table => new
            {
                RECID = table.Column<long>(type: "bigint", nullable: false).Annotation("SqlServer:Identity", "1, 1"),
                ItemGroupId = table.Column<string>(type: "nvarchar(10)", maxLength: 10, nullable: false),
                InventAccountType = table.Column<int>(type: "int", nullable: false),
                LedgerDimension = table.Column<long>(type: "bigint", nullable: false),
                LedgerDimensionGroup = table.Column<int>(type: "int", nullable: false),
                RowVersion = table.Column<byte[]>(type: "rowversion", rowVersion: true, nullable: false),
                IsDeleted = table.Column<bool>(type: "bit", nullable: false),
                IsActive = table.Column<bool>(type: "bit", nullable: false),
                RecVersion = table.Column<int>(type: "int", nullable: false),
                DataAreaId = table.Column<string>(type: "nvarchar(4)", maxLength: 4, nullable: false),
                CreatedBy = table.Column<string>(type: "nvarchar(max)", nullable: true),
                CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                LastModifiedBy = table.Column<string>(type: "nvarchar(max)", nullable: true),
                LastModifiedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                OwnerAccountId = table.Column<string>(type: "nvarchar(max)", nullable: true)
            },
            constraints: table => table.PrimaryKey("PK_InventItemGroupForm", x => x.RECID));
        migrationBuilder.CreateIndex(name: "IX_InventItemGroupForm_ItemGroupId_DataAreaId_InventAccountType", table: "InventItemGroupForm", columns: new[] { "ItemGroupId", "DataAreaId", "InventAccountType" }, unique: true);
    }

    protected override void Down(MigrationBuilder migrationBuilder) => migrationBuilder.DropTable(name: "InventItemGroupForm");
}
