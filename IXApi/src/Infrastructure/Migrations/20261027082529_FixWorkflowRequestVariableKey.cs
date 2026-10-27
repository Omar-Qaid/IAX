using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace IAX.IXApi.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class FixWorkflowRequestVariableKey : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropPrimaryKey(
                name: "PK_WfRequestVariables",
                table: "WfRequestVariables");

            migrationBuilder.AddPrimaryKey(
                name: "PK_WfRequestVariables",
                table: "WfRequestVariables",
                column: "RECID");

            migrationBuilder.CreateIndex(
                name: "IX_WfRequestVariables_RequestId_VariableId",
                table: "WfRequestVariables",
                columns: new[] { "RequestId", "VariableId" },
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropPrimaryKey(
                name: "PK_WfRequestVariables",
                table: "WfRequestVariables");

            migrationBuilder.DropIndex(
                name: "IX_WfRequestVariables_RequestId_VariableId",
                table: "WfRequestVariables");

            migrationBuilder.AddPrimaryKey(
                name: "PK_WfRequestVariables",
                table: "WfRequestVariables",
                column: "RequestId");
        }
    }
}
