using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace IAX.IXApi.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class GenericBatchFramework : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_SysBackgroundJobExecutions_SysBackgroundJobs_JobId",
                table: "SysBackgroundJobExecutions");

            migrationBuilder.DropForeignKey(
                name: "FK_SysBackgroundJobTaskExecutions_SysBackgroundJobExecutions_ExecutionId",
                table: "SysBackgroundJobTaskExecutions");

            migrationBuilder.DropForeignKey(
                name: "FK_SysBackgroundJobTasks_SysBackgroundJobs_JobId",
                table: "SysBackgroundJobTasks");

            migrationBuilder.DropForeignKey(
                name: "FK_WFProcessScheduled_SysBackgroundJobs_BackgroundJobId",
                table: "WFProcessScheduled");

            migrationBuilder.DropPrimaryKey(
                name: "PK_SysBackgroundJobTasks",
                table: "SysBackgroundJobTasks");

            migrationBuilder.DropPrimaryKey(
                name: "PK_SysBackgroundJobTaskExecutions",
                table: "SysBackgroundJobTaskExecutions");

            migrationBuilder.DropPrimaryKey(
                name: "PK_SysBackgroundJobs",
                table: "SysBackgroundJobs");

            migrationBuilder.DropPrimaryKey(
                name: "PK_SysBackgroundJobExecutions",
                table: "SysBackgroundJobExecutions");

            migrationBuilder.RenameTable(
                name: "SysBackgroundJobTasks",
                newName: "BatchJobTasks");

            migrationBuilder.RenameTable(
                name: "SysBackgroundJobTaskExecutions",
                newName: "BatchJobTaskHistory");

            migrationBuilder.RenameTable(
                name: "SysBackgroundJobs",
                newName: "BatchJobs");

            migrationBuilder.RenameTable(
                name: "SysBackgroundJobExecutions",
                newName: "BatchJobHistory");

            migrationBuilder.RenameColumn(
                name: "JobKey",
                table: "BatchJobTasks",
                newName: "ServiceKey");

            migrationBuilder.RenameIndex(
                name: "IX_SysBackgroundJobTasks_JobId",
                table: "BatchJobTasks",
                newName: "IX_BatchJobTasks_JobId");

            migrationBuilder.RenameIndex(
                name: "IX_SysBackgroundJobTaskExecutions_ExecutionId",
                table: "BatchJobTaskHistory",
                newName: "IX_BatchJobTaskHistory_ExecutionId");

            migrationBuilder.RenameIndex(
                name: "IX_SysBackgroundJobs_TenantId",
                table: "BatchJobs",
                newName: "IX_BatchJobs_TenantId");

            migrationBuilder.RenameIndex(
                name: "IX_SysBackgroundJobs_Status_IsEnabled_NextRunAt",
                table: "BatchJobs",
                newName: "IX_BatchJobs_Status_IsEnabled_NextRunAt");

            migrationBuilder.RenameIndex(
                name: "IX_SysBackgroundJobs_Status",
                table: "BatchJobs",
                newName: "IX_BatchJobs_Status");

            migrationBuilder.RenameIndex(
                name: "IX_SysBackgroundJobs_Name",
                table: "BatchJobs",
                newName: "IX_BatchJobs_Name");

            migrationBuilder.RenameIndex(
                name: "IX_SysBackgroundJobs_JobKey",
                table: "BatchJobs",
                newName: "IX_BatchJobs_JobKey");

            migrationBuilder.RenameIndex(
                name: "IX_SysBackgroundJobExecutions_Status",
                table: "BatchJobHistory",
                newName: "IX_BatchJobHistory_Status");

            migrationBuilder.RenameIndex(
                name: "IX_SysBackgroundJobExecutions_JobId_CreatedAt",
                table: "BatchJobHistory",
                newName: "IX_BatchJobHistory_JobId_CreatedAt");

            migrationBuilder.RenameIndex(
                name: "IX_SysBackgroundJobExecutions_JobId",
                table: "BatchJobHistory",
                newName: "IX_BatchJobHistory_JobId");











            migrationBuilder.AddColumn<int>(
                name: "MaxRetryCount",
                table: "BatchJobTasks",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "RetryDelaySeconds",
                table: "BatchJobTasks",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "Attempt",
                table: "BatchJobTaskHistory",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<Guid>(
                name: "CorrelationId",
                table: "BatchJobTaskHistory",
                type: "uniqueidentifier",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.AddPrimaryKey(
                name: "PK_BatchJobTasks",
                table: "BatchJobTasks",
                column: "RecId");

            migrationBuilder.AddPrimaryKey(
                name: "PK_BatchJobTaskHistory",
                table: "BatchJobTaskHistory",
                column: "RecId");

            migrationBuilder.AddPrimaryKey(
                name: "PK_BatchJobs",
                table: "BatchJobs",
                column: "RECID");

            migrationBuilder.AddPrimaryKey(
                name: "PK_BatchJobHistory",
                table: "BatchJobHistory",
                column: "RecId");

            migrationBuilder.CreateTable(
                name: "BatchSettings",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false),
                    Enabled = table.Column<bool>(type: "bit", nullable: false),
                    PollIntervalSeconds = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_BatchSettings", x => x.Id);
                });

            migrationBuilder.AddForeignKey(
                name: "FK_BatchJobHistory_BatchJobs_JobId",
                table: "BatchJobHistory",
                column: "JobId",
                principalTable: "BatchJobs",
                principalColumn: "RECID",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_BatchJobTaskHistory_BatchJobHistory_ExecutionId",
                table: "BatchJobTaskHistory",
                column: "ExecutionId",
                principalTable: "BatchJobHistory",
                principalColumn: "RecId",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_BatchJobTasks_BatchJobs_JobId",
                table: "BatchJobTasks",
                column: "JobId",
                principalTable: "BatchJobs",
                principalColumn: "RECID",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_WFProcessScheduled_BatchJobs_BackgroundJobId",
                table: "WFProcessScheduled",
                column: "BackgroundJobId",
                principalTable: "BatchJobs",
                principalColumn: "RECID",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_BatchJobHistory_BatchJobs_JobId",
                table: "BatchJobHistory");

            migrationBuilder.DropForeignKey(
                name: "FK_BatchJobTaskHistory_BatchJobHistory_ExecutionId",
                table: "BatchJobTaskHistory");

            migrationBuilder.DropForeignKey(
                name: "FK_BatchJobTasks_BatchJobs_JobId",
                table: "BatchJobTasks");

            migrationBuilder.DropForeignKey(
                name: "FK_WFProcessScheduled_BatchJobs_BackgroundJobId",
                table: "WFProcessScheduled");

            migrationBuilder.DropTable(
                name: "BatchSettings");

            migrationBuilder.DropPrimaryKey(
                name: "PK_BatchJobTasks",
                table: "BatchJobTasks");

            migrationBuilder.DropPrimaryKey(
                name: "PK_BatchJobTaskHistory",
                table: "BatchJobTaskHistory");

            migrationBuilder.DropPrimaryKey(
                name: "PK_BatchJobs",
                table: "BatchJobs");

            migrationBuilder.DropPrimaryKey(
                name: "PK_BatchJobHistory",
                table: "BatchJobHistory");











            migrationBuilder.DropColumn(
                name: "MaxRetryCount",
                table: "BatchJobTasks");

            migrationBuilder.DropColumn(
                name: "RetryDelaySeconds",
                table: "BatchJobTasks");

            migrationBuilder.DropColumn(
                name: "Attempt",
                table: "BatchJobTaskHistory");

            migrationBuilder.DropColumn(
                name: "CorrelationId",
                table: "BatchJobTaskHistory");

            migrationBuilder.RenameTable(
                name: "BatchJobTasks",
                newName: "SysBackgroundJobTasks");

            migrationBuilder.RenameTable(
                name: "BatchJobTaskHistory",
                newName: "SysBackgroundJobTaskExecutions");

            migrationBuilder.RenameTable(
                name: "BatchJobs",
                newName: "SysBackgroundJobs");

            migrationBuilder.RenameTable(
                name: "BatchJobHistory",
                newName: "SysBackgroundJobExecutions");

            migrationBuilder.RenameColumn(
                name: "ServiceKey",
                table: "SysBackgroundJobTasks",
                newName: "JobKey");

            migrationBuilder.RenameIndex(
                name: "IX_BatchJobTasks_JobId",
                table: "SysBackgroundJobTasks",
                newName: "IX_SysBackgroundJobTasks_JobId");

            migrationBuilder.RenameIndex(
                name: "IX_BatchJobTaskHistory_ExecutionId",
                table: "SysBackgroundJobTaskExecutions",
                newName: "IX_SysBackgroundJobTaskExecutions_ExecutionId");

            migrationBuilder.RenameIndex(
                name: "IX_BatchJobs_TenantId",
                table: "SysBackgroundJobs",
                newName: "IX_SysBackgroundJobs_TenantId");

            migrationBuilder.RenameIndex(
                name: "IX_BatchJobs_Status_IsEnabled_NextRunAt",
                table: "SysBackgroundJobs",
                newName: "IX_SysBackgroundJobs_Status_IsEnabled_NextRunAt");

            migrationBuilder.RenameIndex(
                name: "IX_BatchJobs_Status",
                table: "SysBackgroundJobs",
                newName: "IX_SysBackgroundJobs_Status");

            migrationBuilder.RenameIndex(
                name: "IX_BatchJobs_Name",
                table: "SysBackgroundJobs",
                newName: "IX_SysBackgroundJobs_Name");

            migrationBuilder.RenameIndex(
                name: "IX_BatchJobs_JobKey",
                table: "SysBackgroundJobs",
                newName: "IX_SysBackgroundJobs_JobKey");

            migrationBuilder.RenameIndex(
                name: "IX_BatchJobHistory_Status",
                table: "SysBackgroundJobExecutions",
                newName: "IX_SysBackgroundJobExecutions_Status");

            migrationBuilder.RenameIndex(
                name: "IX_BatchJobHistory_JobId_CreatedAt",
                table: "SysBackgroundJobExecutions",
                newName: "IX_SysBackgroundJobExecutions_JobId_CreatedAt");

            migrationBuilder.RenameIndex(
                name: "IX_BatchJobHistory_JobId",
                table: "SysBackgroundJobExecutions",
                newName: "IX_SysBackgroundJobExecutions_JobId");

            migrationBuilder.AddPrimaryKey(
                name: "PK_SysBackgroundJobTasks",
                table: "SysBackgroundJobTasks",
                column: "RecId");

            migrationBuilder.AddPrimaryKey(
                name: "PK_SysBackgroundJobTaskExecutions",
                table: "SysBackgroundJobTaskExecutions",
                column: "RecId");

            migrationBuilder.AddPrimaryKey(
                name: "PK_SysBackgroundJobs",
                table: "SysBackgroundJobs",
                column: "RECID");

            migrationBuilder.AddPrimaryKey(
                name: "PK_SysBackgroundJobExecutions",
                table: "SysBackgroundJobExecutions",
                column: "RecId");

            migrationBuilder.AddForeignKey(
                name: "FK_SysBackgroundJobExecutions_SysBackgroundJobs_JobId",
                table: "SysBackgroundJobExecutions",
                column: "JobId",
                principalTable: "SysBackgroundJobs",
                principalColumn: "RECID",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_SysBackgroundJobTaskExecutions_SysBackgroundJobExecutions_ExecutionId",
                table: "SysBackgroundJobTaskExecutions",
                column: "ExecutionId",
                principalTable: "SysBackgroundJobExecutions",
                principalColumn: "RecId",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_SysBackgroundJobTasks_SysBackgroundJobs_JobId",
                table: "SysBackgroundJobTasks",
                column: "JobId",
                principalTable: "SysBackgroundJobs",
                principalColumn: "RECID",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_WFProcessScheduled_SysBackgroundJobs_BackgroundJobId",
                table: "WFProcessScheduled",
                column: "BackgroundJobId",
                principalTable: "SysBackgroundJobs",
                principalColumn: "RECID",
                onDelete: ReferentialAction.Restrict);
        }
    }
}
