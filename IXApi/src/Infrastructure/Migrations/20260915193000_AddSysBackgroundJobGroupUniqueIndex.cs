using IAX.IXApi.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace IAX.IXApi.Infrastructure.Migrations;

[DbContext(typeof(ApplicationDbContext))]
[Migration("20260915193000_AddSysBackgroundJobGroupUniqueIndex")]
public partial class AddSysBackgroundJobGroupUniqueIndex : Migration
{
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.CreateIndex(
            name: "IX_SysBackgroundJobGroup_DataAreaId_GroupCode",
            table: "SysBackgroundJobGroup", columns: new[] { "DataAreaId", "GroupCode" }, unique: true);
    }

    protected override void Down(MigrationBuilder migrationBuilder) =>
        migrationBuilder.DropIndex(name: "IX_SysBackgroundJobGroup_DataAreaId_GroupCode", table: "SysBackgroundJobGroup");
}
