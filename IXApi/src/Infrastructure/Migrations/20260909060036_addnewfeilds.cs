using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace IAX.IXApi.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class addnewfeilds : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "AutoPassingHrs",
                table: "WfSteps");

            migrationBuilder.RenameColumn(
                name: "AllMandatory",
                table: "WfSteps",
                newName: "MustCompleteAll");

            migrationBuilder.RenameColumn(
                name: "MandatoryDocs",
                table: "WfProcesses",
                newName: "MandatoryDocuments");

            migrationBuilder.RenameColumn(
                name: "CanRepeat",
                table: "WfProcesses",
                newName: "IsRepeatable");

            migrationBuilder.RenameColumn(
                name: "ShowPreviousSteps",
                table: "WfActivities",
                newName: "MandatoryDocuments");

            migrationBuilder.RenameColumn(
                name: "ShowPreviousDocs",
                table: "WfActivities",
                newName: "IsWhatsAppNotificationEnabled");

            migrationBuilder.RenameColumn(
                name: "MandatoryDocs",
                table: "WfActivities",
                newName: "IsSystemNotificationEnabled");

            migrationBuilder.RenameColumn(
                name: "AutoPassingHrs",
                table: "WfActivities",
                newName: "AutoPassAfterHours");

            migrationBuilder.RenameColumn(
                name: "AutoPassEnabled",
                table: "WfActivities",
                newName: "IsSmsNotificationEnabled");

            migrationBuilder.RenameColumn(
                name: "AlertingByWhatsApp",
                table: "WfActivities",
                newName: "IsEmailNotificationEnabled");

            migrationBuilder.RenameColumn(
                name: "AlertingBySystem",
                table: "WfActivities",
                newName: "IsAutoPassEnabled");

            migrationBuilder.RenameColumn(
                name: "AlertingBySms",
                table: "WfActivities",
                newName: "CanViewPreviousSteps");

            migrationBuilder.RenameColumn(
                name: "AlertingByEmail",
                table: "WfActivities",
                newName: "CanViewPreviousDocuments");

            migrationBuilder.AddColumn<byte>(
                name: "RepeatIntervalHours",
                table: "WfProcesses",
                type: "tinyint",
                nullable: false,
                defaultValue: (byte)0);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "RepeatIntervalHours",
                table: "WfProcesses");

            migrationBuilder.RenameColumn(
                name: "MustCompleteAll",
                table: "WfSteps",
                newName: "AllMandatory");

            migrationBuilder.RenameColumn(
                name: "MandatoryDocuments",
                table: "WfProcesses",
                newName: "MandatoryDocs");

            migrationBuilder.RenameColumn(
                name: "IsRepeatable",
                table: "WfProcesses",
                newName: "CanRepeat");

            migrationBuilder.RenameColumn(
                name: "MandatoryDocuments",
                table: "WfActivities",
                newName: "ShowPreviousSteps");

            migrationBuilder.RenameColumn(
                name: "IsWhatsAppNotificationEnabled",
                table: "WfActivities",
                newName: "ShowPreviousDocs");

            migrationBuilder.RenameColumn(
                name: "IsSystemNotificationEnabled",
                table: "WfActivities",
                newName: "MandatoryDocs");

            migrationBuilder.RenameColumn(
                name: "IsSmsNotificationEnabled",
                table: "WfActivities",
                newName: "AutoPassEnabled");

            migrationBuilder.RenameColumn(
                name: "IsEmailNotificationEnabled",
                table: "WfActivities",
                newName: "AlertingByWhatsApp");

            migrationBuilder.RenameColumn(
                name: "IsAutoPassEnabled",
                table: "WfActivities",
                newName: "AlertingBySystem");

            migrationBuilder.RenameColumn(
                name: "CanViewPreviousSteps",
                table: "WfActivities",
                newName: "AlertingBySms");

            migrationBuilder.RenameColumn(
                name: "CanViewPreviousDocuments",
                table: "WfActivities",
                newName: "AlertingByEmail");

            migrationBuilder.RenameColumn(
                name: "AutoPassAfterHours",
                table: "WfActivities",
                newName: "AutoPassingHrs");

            migrationBuilder.AddColumn<byte>(
                name: "AutoPassingHrs",
                table: "WfSteps",
                type: "tinyint",
                nullable: false,
                defaultValue: (byte)0);
        }
    }
}
