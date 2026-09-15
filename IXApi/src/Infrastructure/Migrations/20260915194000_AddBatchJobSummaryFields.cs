using IAX.IXApi.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;

namespace IAX.IXApi.Infrastructure.Migrations;

[DbContext(typeof(ApplicationDbContext))]
[Migration("20260915194000_AddBatchJobSummaryFields")]
public sealed class AddBatchJobSummaryFields : Migration
{
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.AddColumn<bool>(name: "HasAlert", table: "BatchJobs", type: "bit", nullable: false, defaultValue: false);
        migrationBuilder.AddColumn<decimal>(name: "Progress", table: "BatchJobs", type: "decimal(18,4)", precision: 18, scale: 4, nullable: false, defaultValue: 0m);
    }

    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropColumn(name: "HasAlert", table: "BatchJobs");
        migrationBuilder.DropColumn(name: "Progress", table: "BatchJobs");
    }
}
