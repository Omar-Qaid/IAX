using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace IAX.IXApi.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddWorkflowMasterNameAlias : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "NameAlias",
                table: "WfVariables",
                type: "nvarchar(255)",
                maxLength: 255,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "NameAlias",
                table: "WfSteps",
                type: "nvarchar(255)",
                maxLength: 255,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "NameAlias",
                table: "WfRequestControls",
                type: "nvarchar(255)",
                maxLength: 255,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "NameAlias",
                table: "WfProcessTypes",
                type: "nvarchar(255)",
                maxLength: 255,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "NameAlias",
                table: "WfProcesses",
                type: "nvarchar(255)",
                maxLength: 255,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "NameAlias",
                table: "WfPriorities",
                type: "nvarchar(255)",
                maxLength: 255,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "NameAlias",
                table: "WfPrintTemplates",
                type: "nvarchar(255)",
                maxLength: 255,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "NameAlias",
                table: "WfOperators",
                type: "nvarchar(255)",
                maxLength: 255,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "NameAlias",
                table: "WfDataTypes",
                type: "nvarchar(255)",
                maxLength: 255,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "NameAlias",
                table: "WfControls",
                type: "nvarchar(255)",
                maxLength: 255,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "NameAlias",
                table: "WfCategories",
                type: "nvarchar(255)",
                maxLength: 255,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "NameAlias",
                table: "WfActivityTypes",
                type: "nvarchar(255)",
                maxLength: 255,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "NameAlias",
                table: "WfActivityControls",
                type: "nvarchar(255)",
                maxLength: 255,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "NameAlias",
                table: "WfActivities",
                type: "nvarchar(255)",
                maxLength: 255,
                nullable: true);

            migrationBuilder.UpdateData(
                table: "WfDataTypes",
                keyColumn: "RECID",
                keyValue: (byte)1,
                column: "NameAlias",
                value: null);

            migrationBuilder.UpdateData(
                table: "WfDataTypes",
                keyColumn: "RECID",
                keyValue: (byte)2,
                column: "NameAlias",
                value: null);

            migrationBuilder.UpdateData(
                table: "WfDataTypes",
                keyColumn: "RECID",
                keyValue: (byte)3,
                column: "NameAlias",
                value: null);

            migrationBuilder.UpdateData(
                table: "WfDataTypes",
                keyColumn: "RECID",
                keyValue: (byte)4,
                column: "NameAlias",
                value: null);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "NameAlias",
                table: "WfVariables");

            migrationBuilder.DropColumn(
                name: "NameAlias",
                table: "WfSteps");

            migrationBuilder.DropColumn(
                name: "NameAlias",
                table: "WfRequestControls");

            migrationBuilder.DropColumn(
                name: "NameAlias",
                table: "WfProcessTypes");

            migrationBuilder.DropColumn(
                name: "NameAlias",
                table: "WfProcesses");

            migrationBuilder.DropColumn(
                name: "NameAlias",
                table: "WfPriorities");

            migrationBuilder.DropColumn(
                name: "NameAlias",
                table: "WfPrintTemplates");

            migrationBuilder.DropColumn(
                name: "NameAlias",
                table: "WfOperators");

            migrationBuilder.DropColumn(
                name: "NameAlias",
                table: "WfDataTypes");

            migrationBuilder.DropColumn(
                name: "NameAlias",
                table: "WfControls");

            migrationBuilder.DropColumn(
                name: "NameAlias",
                table: "WfCategories");

            migrationBuilder.DropColumn(
                name: "NameAlias",
                table: "WfActivityTypes");

            migrationBuilder.DropColumn(
                name: "NameAlias",
                table: "WfActivityControls");

            migrationBuilder.DropColumn(
                name: "NameAlias",
                table: "WfActivities");
        }
    }
}
