using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace IAX.IXApi.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddBatchJobTasks : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {










            migrationBuilder.CreateTable(
                name: "SysBackgroundJobTaskExecutions",
                columns: table => new
                {
                    RecId = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ExecutionId = table.Column<long>(type: "bigint", nullable: false),
                    TaskId = table.Column<long>(type: "bigint", nullable: false),
                    TaskName = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    Status = table.Column<int>(type: "int", nullable: false),
                    StartedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CompletedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    Output = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ErrorMessage = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SysBackgroundJobTaskExecutions", x => x.RecId);
                    table.ForeignKey(
                        name: "FK_SysBackgroundJobTaskExecutions_SysBackgroundJobExecutions_ExecutionId",
                        column: x => x.ExecutionId,
                        principalTable: "SysBackgroundJobExecutions",
                        principalColumn: "RecId",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "SysBackgroundJobTasks",
                columns: table => new
                {
                    RecId = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    JobId = table.Column<long>(type: "bigint", nullable: false),
                    Name = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    JobKey = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    PayloadJson = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ExecutionOrder = table.Column<int>(type: "int", nullable: false),
                    DependsOnTaskId = table.Column<long>(type: "bigint", nullable: true),
                    IsEnabled = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SysBackgroundJobTasks", x => x.RecId);
                    table.ForeignKey(
                        name: "FK_SysBackgroundJobTasks_SysBackgroundJobs_JobId",
                        column: x => x.JobId,
                        principalTable: "SysBackgroundJobs",
                        principalColumn: "RECID",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_SysBackgroundJobTaskExecutions_ExecutionId",
                table: "SysBackgroundJobTaskExecutions",
                column: "ExecutionId");

            migrationBuilder.CreateIndex(
                name: "IX_SysBackgroundJobTasks_JobId",
                table: "SysBackgroundJobTasks",
                column: "JobId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "SysBackgroundJobTaskExecutions");

            migrationBuilder.DropTable(
                name: "SysBackgroundJobTasks");










        }
    }
}
