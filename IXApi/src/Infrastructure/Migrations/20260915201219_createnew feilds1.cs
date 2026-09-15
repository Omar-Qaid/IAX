using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace IAX.IXApi.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class createnewfeilds1 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_SysBackgroundJobRecurrenceCount_BatchJobs_BatchJobId",
                table: "SysBackgroundJobRecurrenceCount");

            migrationBuilder.DropPrimaryKey(
                name: "PK_SysBackgroundJobRecurrenceCount",
                table: "SysBackgroundJobRecurrenceCount");

            migrationBuilder.RenameTable(
                name: "SysBackgroundJobRecurrenceCount",
                newName: "SysBackgroundJobRecurrenceCounts");

            migrationBuilder.RenameIndex(
                name: "IX_SysBackgroundJobRecurrenceCount_BatchJobId",
                table: "SysBackgroundJobRecurrenceCounts",
                newName: "IX_SysBackgroundJobRecurrenceCounts_BatchJobId");

            migrationBuilder.AddPrimaryKey(
                name: "PK_SysBackgroundJobRecurrenceCounts",
                table: "SysBackgroundJobRecurrenceCounts",
                column: "RECID");

            migrationBuilder.AddForeignKey(
                name: "FK_SysBackgroundJobRecurrenceCounts_BatchJobs_BatchJobId",
                table: "SysBackgroundJobRecurrenceCounts",
                column: "BatchJobId",
                principalTable: "BatchJobs",
                principalColumn: "RECID",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_SysBackgroundJobRecurrenceCounts_BatchJobs_BatchJobId",
                table: "SysBackgroundJobRecurrenceCounts");

            migrationBuilder.DropPrimaryKey(
                name: "PK_SysBackgroundJobRecurrenceCounts",
                table: "SysBackgroundJobRecurrenceCounts");

            migrationBuilder.RenameTable(
                name: "SysBackgroundJobRecurrenceCounts",
                newName: "SysBackgroundJobRecurrenceCount");

            migrationBuilder.RenameIndex(
                name: "IX_SysBackgroundJobRecurrenceCounts_BatchJobId",
                table: "SysBackgroundJobRecurrenceCount",
                newName: "IX_SysBackgroundJobRecurrenceCount_BatchJobId");

            migrationBuilder.AddPrimaryKey(
                name: "PK_SysBackgroundJobRecurrenceCount",
                table: "SysBackgroundJobRecurrenceCount",
                column: "RECID");

            migrationBuilder.AddForeignKey(
                name: "FK_SysBackgroundJobRecurrenceCount_BatchJobs_BatchJobId",
                table: "SysBackgroundJobRecurrenceCount",
                column: "BatchJobId",
                principalTable: "BatchJobs",
                principalColumn: "RECID",
                onDelete: ReferentialAction.Restrict);
        }
    }
}
