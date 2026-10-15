using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace IAX.IXApi.Infrastructure.Migrations;

public partial class AddEcoResCategory : Migration
{
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.CreateTable(
            name: "EcoResCategory",
            columns: table => new
            {
                RECID = table.Column<long>(type: "bigint", nullable: false)
                    .Annotation("SqlServer:Identity", "1, 1"),
                Name = table.Column<string>(type: "nvarchar(60)", maxLength: 60, nullable: false),
                CategoryHierarchy = table.Column<long>(type: "bigint", nullable: false),
                ParentCategory = table.Column<long>(type: "bigint", nullable: false),
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
            constraints: table => table.PrimaryKey("PK_EcoResCategory", x => x.RECID));

        migrationBuilder.CreateIndex(
            name: "IX_EcoResCategory_CategoryHierarchy_Name",
            table: "EcoResCategory",
            columns: new[] { "CategoryHierarchy", "Name" });
    }

    protected override void Down(MigrationBuilder migrationBuilder) =>
        migrationBuilder.DropTable(name: "EcoResCategory");
}
