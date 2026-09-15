using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace IAX.IXApi.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class createnewfeilds : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_BatchJobs_Status_IsEnabled_NextRunAt",
                table: "BatchJobs");

            migrationBuilder.DropColumn(
                name: "CronExpression",
                table: "BatchJobs");

            migrationBuilder.RenameColumn(
                name: "RunAt",
                table: "BatchJobs",
                newName: "StartDateTime");

            migrationBuilder.RenameColumn(
                name: "Priority",
                table: "BatchJobs",
                newName: "SchedulingPriorityIsOverridden");

            migrationBuilder.RenameColumn(
                name: "NextRunAt",
                table: "BatchJobs",
                newName: "StartDate");

            migrationBuilder.RenameColumn(
                name: "Name",
                table: "BatchJobs",
                newName: "Caption");

            migrationBuilder.RenameColumn(
                name: "LastRunAt",
                table: "BatchJobs",
                newName: "OrigStartDateTime");

            migrationBuilder.RenameColumn(
                name: "IntervalSeconds",
                table: "BatchJobs",
                newName: "StartTime");

            migrationBuilder.RenameIndex(
                name: "IX_BatchJobs_Name",
                table: "BatchJobs",
                newName: "IX_BatchJobs_Caption");

            migrationBuilder.RenameColumn(
                name: "RecId",
                table: "BatchJobHistory",
                newName: "RECID");

            migrationBuilder.AddColumn<string>(
                name: "ActivePeriod",
                table: "BatchJobs",
                type: "nvarchar(10)",
                maxLength: 10,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "BatchGroup",
                table: "BatchJobs",
                type: "nvarchar(10)",
                maxLength: 10,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "CanceledBy",
                table: "BatchJobs",
                type: "nvarchar(20)",
                maxLength: 20,
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "Critical",
                table: "BatchJobs",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<string>(
                name: "DataPartition",
                table: "BatchJobs",
                type: "nvarchar(8)",
                maxLength: 8,
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "EmitBusinessEvent",
                table: "BatchJobs",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<DateTime>(
                name: "EndDateTime",
                table: "BatchJobs",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "EndDateTimeTzId",
                table: "BatchJobs",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ExecutingBy",
                table: "BatchJobs",
                type: "nvarchar(20)",
                maxLength: 20,
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "Finishing",
                table: "BatchJobs",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "LogLevel",
                table: "BatchJobs",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "Managed",
                table: "BatchJobs",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "MonitoringCategory",
                table: "BatchJobs",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "OrigStartDateTimeTzId",
                table: "BatchJobs",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<byte[]>(
                name: "RecurrenceData",
                table: "BatchJobs",
                type: "varbinary(max)",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "RuntimeJob",
                table: "BatchJobs",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "SchedulingPriority",
                table: "BatchJobs",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "StartDateTimeTzId",
                table: "BatchJobs",
                type: "int",
                nullable: true);

            migrationBuilder.AlterColumn<DateTime>(
                name: "CreatedAt",
                table: "BatchJobHistory",
                type: "datetime2",
                nullable: true,
                oldClrType: typeof(DateTime),
                oldType: "datetime2");

            migrationBuilder.AddColumn<int>(
                name: "AlertsProcessed",
                table: "BatchJobHistory",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<string>(
                name: "BatchCreatedBy",
                table: "BatchJobHistory",
                type: "nvarchar(20)",
                maxLength: 20,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "BatchGroup",
                table: "BatchJobHistory",
                type: "nvarchar(10)",
                maxLength: 10,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "CanceledBy",
                table: "BatchJobHistory",
                type: "nvarchar(20)",
                maxLength: 20,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Caption",
                table: "BatchJobHistory",
                type: "nvarchar(200)",
                maxLength: 200,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "CreatedBy",
                table: "BatchJobHistory",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "DataAreaId",
                table: "BatchJobHistory",
                type: "nvarchar(4)",
                maxLength: 4,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "DataPartition",
                table: "BatchJobHistory",
                type: "nvarchar(8)",
                maxLength: 8,
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "EndDateTimeTzId",
                table: "BatchJobHistory",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ExecutedBy",
                table: "BatchJobHistory",
                type: "nvarchar(20)",
                maxLength: 20,
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "Finishing",
                table: "BatchJobHistory",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "GroupSchedulingPriority",
                table: "BatchJobHistory",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<bool>(
                name: "IsActive",
                table: "BatchJobHistory",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "IsDeleted",
                table: "BatchJobHistory",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<int>(
                name: "JobSchedulingPriority",
                table: "BatchJobHistory",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "JobSchedulingPriorityIsOverridden",
                table: "BatchJobHistory",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<DateTime>(
                name: "LastModifiedAt",
                table: "BatchJobHistory",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "LastModifiedBy",
                table: "BatchJobHistory",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "OrigStartDateTime",
                table: "BatchJobHistory",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "OrigStartDateTimeTzId",
                table: "BatchJobHistory",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "OwnerAccountId",
                table: "BatchJobHistory",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "RecVersion",
                table: "BatchJobHistory",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<byte[]>(
                name: "RowVersion",
                table: "BatchJobHistory",
                type: "rowversion",
                rowVersion: true,
                nullable: false,
                defaultValue: new byte[0]);

            migrationBuilder.AddColumn<int>(
                name: "RuntimeJob",
                table: "BatchJobHistory",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "StartDateTimeTzId",
                table: "BatchJobHistory",
                type: "int",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "SysBackgroundJobActivePeriod",
                columns: table => new
                {
                    RECID = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Code = table.Column<string>(type: "nvarchar(10)", maxLength: 10, nullable: false),
                    Name = table.Column<string>(type: "nvarchar(150)", maxLength: 150, nullable: true),
                    FromTimeUtc = table.Column<int>(type: "int", nullable: false),
                    ToTimeUtc = table.Column<int>(type: "int", nullable: false),
                    FromTimeLocal = table.Column<int>(type: "int", nullable: false),
                    ToTimeLocal = table.Column<int>(type: "int", nullable: false),
                    TimeZoneFollowed = table.Column<int>(type: "int", nullable: false),
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
                constraints: table =>
                {
                    table.PrimaryKey("PK_SysBackgroundJobActivePeriod", x => x.RECID);
                });

            migrationBuilder.CreateTable(
                name: "SysBackgroundJobGroup",
                columns: table => new
                {
                    RECID = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    GroupCode = table.Column<string>(type: "nvarchar(10)", maxLength: 10, nullable: false),
                    Description = table.Column<string>(type: "nvarchar(60)", maxLength: 60, nullable: true),
                    SchedulingPriority = table.Column<int>(type: "int", nullable: false),
                    MaxConcurrency = table.Column<int>(type: "int", nullable: false),
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
                constraints: table =>
                {
                    table.PrimaryKey("PK_SysBackgroundJobGroup", x => x.RECID);
                });

            migrationBuilder.CreateTable(
                name: "SysBackgroundJobRecurrenceCount",
                columns: table => new
                {
                    RECID = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    BatchJobId = table.Column<long>(type: "bigint", nullable: false),
                    RecurrenceCount = table.Column<int>(type: "int", nullable: false),
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
                constraints: table =>
                {
                    table.PrimaryKey("PK_SysBackgroundJobRecurrenceCount", x => x.RECID);
                    table.ForeignKey(
                        name: "FK_SysBackgroundJobRecurrenceCount_BatchJobs_BatchJobId",
                        column: x => x.BatchJobId,
                        principalTable: "BatchJobs",
                        principalColumn: "RECID",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_BatchJobs_Status_IsEnabled_StartDateTime",
                table: "BatchJobs",
                columns: new[] { "Status", "IsEnabled", "StartDateTime" });

            migrationBuilder.CreateIndex(
                name: "IX_SysBackgroundJobRecurrenceCount_BatchJobId",
                table: "SysBackgroundJobRecurrenceCount",
                column: "BatchJobId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "SysBackgroundJobActivePeriod");

            migrationBuilder.DropTable(
                name: "SysBackgroundJobGroup");

            migrationBuilder.DropTable(
                name: "SysBackgroundJobRecurrenceCount");

            migrationBuilder.DropIndex(
                name: "IX_BatchJobs_Status_IsEnabled_StartDateTime",
                table: "BatchJobs");

            migrationBuilder.DropColumn(
                name: "ActivePeriod",
                table: "BatchJobs");

            migrationBuilder.DropColumn(
                name: "BatchGroup",
                table: "BatchJobs");

            migrationBuilder.DropColumn(
                name: "CanceledBy",
                table: "BatchJobs");

            migrationBuilder.DropColumn(
                name: "Critical",
                table: "BatchJobs");

            migrationBuilder.DropColumn(
                name: "DataPartition",
                table: "BatchJobs");

            migrationBuilder.DropColumn(
                name: "EmitBusinessEvent",
                table: "BatchJobs");

            migrationBuilder.DropColumn(
                name: "EndDateTime",
                table: "BatchJobs");

            migrationBuilder.DropColumn(
                name: "EndDateTimeTzId",
                table: "BatchJobs");

            migrationBuilder.DropColumn(
                name: "ExecutingBy",
                table: "BatchJobs");

            migrationBuilder.DropColumn(
                name: "Finishing",
                table: "BatchJobs");

            migrationBuilder.DropColumn(
                name: "LogLevel",
                table: "BatchJobs");

            migrationBuilder.DropColumn(
                name: "Managed",
                table: "BatchJobs");

            migrationBuilder.DropColumn(
                name: "MonitoringCategory",
                table: "BatchJobs");

            migrationBuilder.DropColumn(
                name: "OrigStartDateTimeTzId",
                table: "BatchJobs");

            migrationBuilder.DropColumn(
                name: "RecurrenceData",
                table: "BatchJobs");

            migrationBuilder.DropColumn(
                name: "RuntimeJob",
                table: "BatchJobs");

            migrationBuilder.DropColumn(
                name: "SchedulingPriority",
                table: "BatchJobs");

            migrationBuilder.DropColumn(
                name: "StartDateTimeTzId",
                table: "BatchJobs");

            migrationBuilder.DropColumn(
                name: "AlertsProcessed",
                table: "BatchJobHistory");

            migrationBuilder.DropColumn(
                name: "BatchCreatedBy",
                table: "BatchJobHistory");

            migrationBuilder.DropColumn(
                name: "BatchGroup",
                table: "BatchJobHistory");

            migrationBuilder.DropColumn(
                name: "CanceledBy",
                table: "BatchJobHistory");

            migrationBuilder.DropColumn(
                name: "Caption",
                table: "BatchJobHistory");

            migrationBuilder.DropColumn(
                name: "CreatedBy",
                table: "BatchJobHistory");

            migrationBuilder.DropColumn(
                name: "DataAreaId",
                table: "BatchJobHistory");

            migrationBuilder.DropColumn(
                name: "DataPartition",
                table: "BatchJobHistory");

            migrationBuilder.DropColumn(
                name: "EndDateTimeTzId",
                table: "BatchJobHistory");

            migrationBuilder.DropColumn(
                name: "ExecutedBy",
                table: "BatchJobHistory");

            migrationBuilder.DropColumn(
                name: "Finishing",
                table: "BatchJobHistory");

            migrationBuilder.DropColumn(
                name: "GroupSchedulingPriority",
                table: "BatchJobHistory");

            migrationBuilder.DropColumn(
                name: "IsActive",
                table: "BatchJobHistory");

            migrationBuilder.DropColumn(
                name: "IsDeleted",
                table: "BatchJobHistory");

            migrationBuilder.DropColumn(
                name: "JobSchedulingPriority",
                table: "BatchJobHistory");

            migrationBuilder.DropColumn(
                name: "JobSchedulingPriorityIsOverridden",
                table: "BatchJobHistory");

            migrationBuilder.DropColumn(
                name: "LastModifiedAt",
                table: "BatchJobHistory");

            migrationBuilder.DropColumn(
                name: "LastModifiedBy",
                table: "BatchJobHistory");

            migrationBuilder.DropColumn(
                name: "OrigStartDateTime",
                table: "BatchJobHistory");

            migrationBuilder.DropColumn(
                name: "OrigStartDateTimeTzId",
                table: "BatchJobHistory");

            migrationBuilder.DropColumn(
                name: "OwnerAccountId",
                table: "BatchJobHistory");

            migrationBuilder.DropColumn(
                name: "RecVersion",
                table: "BatchJobHistory");

            migrationBuilder.DropColumn(
                name: "RowVersion",
                table: "BatchJobHistory");

            migrationBuilder.DropColumn(
                name: "RuntimeJob",
                table: "BatchJobHistory");

            migrationBuilder.DropColumn(
                name: "StartDateTimeTzId",
                table: "BatchJobHistory");

            migrationBuilder.RenameColumn(
                name: "StartTime",
                table: "BatchJobs",
                newName: "IntervalSeconds");

            migrationBuilder.RenameColumn(
                name: "StartDateTime",
                table: "BatchJobs",
                newName: "RunAt");

            migrationBuilder.RenameColumn(
                name: "StartDate",
                table: "BatchJobs",
                newName: "NextRunAt");

            migrationBuilder.RenameColumn(
                name: "SchedulingPriorityIsOverridden",
                table: "BatchJobs",
                newName: "Priority");

            migrationBuilder.RenameColumn(
                name: "OrigStartDateTime",
                table: "BatchJobs",
                newName: "LastRunAt");

            migrationBuilder.RenameColumn(
                name: "Caption",
                table: "BatchJobs",
                newName: "Name");

            migrationBuilder.RenameIndex(
                name: "IX_BatchJobs_Caption",
                table: "BatchJobs",
                newName: "IX_BatchJobs_Name");

            migrationBuilder.RenameColumn(
                name: "RECID",
                table: "BatchJobHistory",
                newName: "RecId");

            migrationBuilder.AddColumn<string>(
                name: "CronExpression",
                table: "BatchJobs",
                type: "nvarchar(120)",
                maxLength: 120,
                nullable: true);

            migrationBuilder.AlterColumn<DateTime>(
                name: "CreatedAt",
                table: "BatchJobHistory",
                type: "datetime2",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified),
                oldClrType: typeof(DateTime),
                oldType: "datetime2",
                oldNullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_BatchJobs_Status_IsEnabled_NextRunAt",
                table: "BatchJobs",
                columns: new[] { "Status", "IsEnabled", "NextRunAt" });
        }
    }
}
