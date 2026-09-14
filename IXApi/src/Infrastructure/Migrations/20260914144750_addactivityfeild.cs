using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace IAX.IXApi.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class addactivityfeild : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "UsedAsCriteria",
                table: "WfRequestDetails");

            migrationBuilder.AlterColumn<string>(
                name: "ControlValue",
                table: "WfRequestDetails",
                type: "nvarchar(255)",
                maxLength: 255,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(255)",
                oldMaxLength: 255);

            migrationBuilder.AddColumn<decimal>(
                name: "EarnedScore",
                table: "WfRequestDetails",
                type: "decimal(18,4)",
                precision: 18,
                scale: 4,
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<string>(
                name: "Value",
                table: "WfRequestDetails",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ValueAlias",
                table: "WfRequestDetails",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AlterColumn<bool>(
                name: "UsedAsCriteria",
                table: "WfActivityDetails",
                type: "bit",
                nullable: false,
                defaultValue: false,
                oldClrType: typeof(bool),
                oldType: "bit");

            migrationBuilder.AlterColumn<string>(
                name: "ControlValue",
                table: "WfActivityDetails",
                type: "nvarchar(255)",
                maxLength: 255,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(255)",
                oldMaxLength: 255);

            migrationBuilder.AddColumn<string>(
                name: "ControlLabel",
                table: "WfActivityDetails",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ControlLabelAR",
                table: "WfActivityDetails",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ControlValueAR",
                table: "WfActivityDetails",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ControlValueEN",
                table: "WfActivityDetails",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "EarnedScore",
                table: "WfActivityDetails",
                type: "decimal(18,4)",
                precision: 18,
                scale: 4,
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<long>(
                name: "RelatedObjectId",
                table: "WfActivityDetails",
                type: "bigint",
                nullable: false,
                defaultValue: 0L);

            migrationBuilder.AddColumn<decimal>(
                name: "Score",
                table: "WfActivityDetails",
                type: "decimal(18,4)",
                precision: 18,
                scale: 4,
                nullable: false,
                defaultValue: 0m);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "EarnedScore",
                table: "WfRequestDetails");

            migrationBuilder.DropColumn(
                name: "Value",
                table: "WfRequestDetails");

            migrationBuilder.DropColumn(
                name: "ValueAlias",
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

            migrationBuilder.DropColumn(
                name: "EarnedScore",
                table: "WfActivityDetails");

            migrationBuilder.DropColumn(
                name: "RelatedObjectId",
                table: "WfActivityDetails");

            migrationBuilder.DropColumn(
                name: "Score",
                table: "WfActivityDetails");

            migrationBuilder.AlterColumn<string>(
                name: "ControlValue",
                table: "WfRequestDetails",
                type: "nvarchar(255)",
                maxLength: 255,
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "nvarchar(255)",
                oldMaxLength: 255,
                oldNullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "UsedAsCriteria",
                table: "WfRequestDetails",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AlterColumn<bool>(
                name: "UsedAsCriteria",
                table: "WfActivityDetails",
                type: "bit",
                nullable: false,
                oldClrType: typeof(bool),
                oldType: "bit",
                oldDefaultValue: false);

            migrationBuilder.AlterColumn<string>(
                name: "ControlValue",
                table: "WfActivityDetails",
                type: "nvarchar(255)",
                maxLength: 255,
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "nvarchar(255)",
                oldMaxLength: 255,
                oldNullable: true);
        }
    }
}
