using IAX.IXApi.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;

namespace IAX.IXApi.Infrastructure.Migrations;

[DbContext(typeof(ApplicationDbContext))]
[Migration("20260915195000_RenameBatchJobActivePeriod")]
public sealed class RenameBatchJobActivePeriod : Migration
{
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.RenameTable(name: "SysBackgroundJobActivePeriod", newName: "BatchJobActivePeriod");
        migrationBuilder.RenameColumn(name: "Code", table: "BatchJobActivePeriod", newName: "ID");
        migrationBuilder.CreateIndex(name: "IX_BatchJobActivePeriod_DataAreaId_ID", table: "BatchJobActivePeriod",
            columns: new[] { "DataAreaId", "ID" }, unique: true);
    }
    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropIndex(name: "IX_BatchJobActivePeriod_DataAreaId_ID", table: "BatchJobActivePeriod");
        migrationBuilder.RenameColumn(name: "ID", table: "BatchJobActivePeriod", newName: "Code");
        migrationBuilder.RenameTable(name: "BatchJobActivePeriod", newName: "SysBackgroundJobActivePeriod");
    }
}
