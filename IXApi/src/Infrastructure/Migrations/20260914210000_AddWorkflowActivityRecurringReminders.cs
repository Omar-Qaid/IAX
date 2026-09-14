using Microsoft.EntityFrameworkCore.Migrations;

namespace IAX.IXApi.Infrastructure.Migrations;

public partial class AddWorkflowActivityRecurringReminders : Migration
{
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.AddColumn<bool>(
            name: "IsRecurringReminderEnabled", table: "WfActivities",
            type: "bit", nullable: false, defaultValue: false);
        migrationBuilder.AddColumn<byte>(
            name: "MaxReminderOccurrences", table: "WfActivities",
            type: "tinyint", nullable: false, defaultValue: (byte)0);
        migrationBuilder.AddColumn<byte>(
            name: "RecurringReminderIntervalHours", table: "WfActivities",
            type: "tinyint", nullable: false, defaultValue: (byte)0);
    }

    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropColumn(name: "IsRecurringReminderEnabled", table: "WfActivities");
        migrationBuilder.DropColumn(name: "MaxReminderOccurrences", table: "WfActivities");
        migrationBuilder.DropColumn(name: "RecurringReminderIntervalHours", table: "WfActivities");
    }
}
