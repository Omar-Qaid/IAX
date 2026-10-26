using IAX.IXApi.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace IAX.IXApi.Infrastructure.Migrations;

[DbContext(typeof(ApplicationDbContext))]
[Migration("20261026190000_SeedExecutionHistory")]
public sealed class SeedExecutionHistory : Migration
{
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.AddColumn<string>("SeederId", "SysDataSeedLogs", type: "nvarchar(200)", maxLength: 200, nullable: true);
        migrationBuilder.AddColumn<int>("Version", "SysDataSeedLogs", type: "int", nullable: false, defaultValue: 1);
        migrationBuilder.AddColumn<string>("Scope", "SysDataSeedLogs", type: "nvarchar(100)", maxLength: 100, nullable: false, defaultValue: "global");
        migrationBuilder.AddColumn<string>("Outcome", "SysDataSeedLogs", type: "nvarchar(16)", maxLength: 16, nullable: false, defaultValue: "Success");
        migrationBuilder.AddColumn<string>("Policy", "SysDataSeedLogs", type: "nvarchar(16)", maxLength: 16, nullable: false, defaultValue: "Legacy");
        migrationBuilder.AddColumn<long>("DurationMs", "SysDataSeedLogs", type: "bigint", nullable: false, defaultValue: 0L);
        migrationBuilder.AddColumn<string>("Error", "SysDataSeedLogs", type: "nvarchar(max)", nullable: true);
        migrationBuilder.CreateIndex(
            name: "IX_SysDataSeedLogs_SeederId_Version_Scope",
            table: "SysDataSeedLogs",
            columns: new[] { "SeederId", "Version", "Scope" },
            unique: true,
            filter: "[SeederId] IS NOT NULL AND [Policy] = 'RunOnce' AND [Outcome] = 'Success'");
    }

    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropIndex("IX_SysDataSeedLogs_SeederId_Version_Scope", "SysDataSeedLogs");
        foreach (var column in new[] { "SeederId", "Version", "Scope", "Outcome", "Policy", "DurationMs", "Error" })
            migrationBuilder.DropColumn(column, "SysDataSeedLogs");
    }
}
