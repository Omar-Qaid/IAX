using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace IAX.IXApi.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class UpdateWorkflowControlNamesAndDetails : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ControlLabel",
                table: "WfRequestDetails");

            migrationBuilder.DropColumn(
                name: "ControlLabelAR",
                table: "WfRequestDetails");

            migrationBuilder.DropColumn(
                name: "ControlValueAR",
                table: "WfRequestDetails");

            migrationBuilder.DropColumn(
                name: "ControlValueEN",
                table: "WfRequestDetails");

            migrationBuilder.DropColumn(
                name: "ControlLabel",
                table: "WfActivityDetails");

            migrationBuilder.DropColumn(
                name: "ControlLabelAR",
                table: "WfActivityDetails");

            migrationBuilder.DropColumn(
                name: "ControlValueAR",
                table: "WfActivityDetails");

            migrationBuilder.DropColumn(
                name: "ControlValueEN",
                table: "WfActivityDetails");

            migrationBuilder.AddColumn<string>(
                name: "Name",
                table: "WfRequestPrintVersions",
                type: "nvarchar(255)",
                maxLength: 255,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "NameAlias",
                table: "WfRequestPrintVersions",
                type: "nvarchar(255)",
                maxLength: 255,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ErrorMessageAlias",
                table: "WfRequestControlsValidations",
                type: "nvarchar(1000)",
                maxLength: 1000,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "NameAlias",
                table: "WfRequestControlsOptions",
                type: "nvarchar(255)",
                maxLength: 255,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "NameAlias",
                table: "WfPerformerType",
                type: "nvarchar(255)",
                maxLength: 255,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "NameAlias",
                table: "WfActivityControlsValidations",
                type: "nvarchar(255)",
                maxLength: 255,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "NameAlias",
                table: "WfActivityControlsOptions",
                type: "nvarchar(255)",
                maxLength: 255,
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Name",
                table: "WfRequestPrintVersions");

            migrationBuilder.DropColumn(
                name: "NameAlias",
                table: "WfRequestPrintVersions");

            migrationBuilder.DropColumn(
                name: "ErrorMessageAlias",
                table: "WfRequestControlsValidations");

            migrationBuilder.DropColumn(
                name: "NameAlias",
                table: "WfRequestControlsOptions");

            migrationBuilder.DropColumn(
                name: "NameAlias",
                table: "WfPerformerType");

            migrationBuilder.DropColumn(
                name: "NameAlias",
                table: "WfActivityControlsValidations");

            migrationBuilder.DropColumn(
                name: "NameAlias",
                table: "WfActivityControlsOptions");

            migrationBuilder.AddColumn<string>(
                name: "ControlLabel",
                table: "WfRequestDetails",
                type: "nvarchar(255)",
                maxLength: 255,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "ControlLabelAR",
                table: "WfRequestDetails",
                type: "nvarchar(255)",
                maxLength: 255,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "ControlValueAR",
                table: "WfRequestDetails",
                type: "nvarchar(255)",
                maxLength: 255,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "ControlValueEN",
                table: "WfRequestDetails",
                type: "nvarchar(255)",
                maxLength: 255,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "ControlLabel",
                table: "WfActivityDetails",
                type: "nvarchar(255)",
                maxLength: 255,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "ControlLabelAR",
                table: "WfActivityDetails",
                type: "nvarchar(255)",
                maxLength: 255,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "ControlValueAR",
                table: "WfActivityDetails",
                type: "nvarchar(255)",
                maxLength: 255,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "ControlValueEN",
                table: "WfActivityDetails",
                type: "nvarchar(255)",
                maxLength: 255,
                nullable: false,
                defaultValue: "");
        }
    }
}
