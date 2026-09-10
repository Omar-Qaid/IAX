using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace IAX.IXApi.Infrastructure.Migrations;

public partial class AddWFProcessScheduled : Migration
{
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.CreateTable(
            name: "WFProcessScheduled",
            columns: table => new
            {
                RecId = table.Column<long>(type: "bigint", nullable: false).Annotation("SqlServer:Identity", "1, 1"),
                ProcessId = table.Column<long>(type: "bigint", nullable: false),
                DataAreaId = table.Column<string>(type: "nvarchar(10)", maxLength: 10, nullable: false),
                ExecutionUserId = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: false),
                OwnerAccountId = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: false),
                Enabled = table.Column<bool>(type: "bit", nullable: false),
                ConfigurationJson = table.Column<string>(type: "nvarchar(max)", nullable: false),
                NextRunAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                LastRunAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                LastRequestId = table.Column<long>(type: "bigint", nullable: true),
                BackgroundJobId = table.Column<long>(type: "bigint", nullable: false),
                RowVersion = table.Column<byte[]>(type: "rowversion", rowVersion: true, nullable: false)
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_WFProcessScheduled", x => x.RecId);
                table.ForeignKey("FK_WFProcessScheduled_SysBackgroundJobs_BackgroundJobId", x => x.BackgroundJobId,
                    "SysBackgroundJobs", "RECID", onDelete: ReferentialAction.Restrict);
                table.ForeignKey("FK_WFProcessScheduled_WfProcesses_ProcessId", x => x.ProcessId,
                    "WfProcesses", "RECID", onDelete: ReferentialAction.Restrict);
            });
        migrationBuilder.CreateIndex("IX_WFProcessScheduled_BackgroundJobId", "WFProcessScheduled", "BackgroundJobId", unique: true);
        migrationBuilder.CreateIndex("IX_WFProcessScheduled_DataAreaId_ProcessId", "WFProcessScheduled", new[] { "DataAreaId", "ProcessId" }, unique: true);
        migrationBuilder.CreateIndex("IX_WFProcessScheduled_ProcessId", "WFProcessScheduled", "ProcessId");
    }

    protected override void Down(MigrationBuilder migrationBuilder) => migrationBuilder.DropTable("WFProcessScheduled");
}
