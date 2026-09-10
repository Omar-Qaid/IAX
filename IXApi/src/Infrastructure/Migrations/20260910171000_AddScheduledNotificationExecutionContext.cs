using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace IAX.IXApi.Infrastructure.Migrations;

public partial class AddScheduledNotificationExecutionContext : Migration
{
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.AddColumn<string>("DataAreaId", "SysScheduledNotifications", "nvarchar(10)", maxLength: 10, nullable: true);
        migrationBuilder.AddColumn<string>("ExecutionUserId", "SysScheduledNotifications", "nvarchar(256)", maxLength: 256, nullable: true);
        migrationBuilder.AddColumn<string>("OwnerAccountId", "SysScheduledNotifications", "nvarchar(256)", maxLength: 256, nullable: true);
        migrationBuilder.AddColumn<bool>("PreserveChannel", "SysScheduledNotifications", "bit", nullable: false, defaultValue: false);
        migrationBuilder.AddColumn<Guid>("ClaimToken", "SysScheduledNotifications", "uniqueidentifier", nullable: false, defaultValue: Guid.Empty);
    }

    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropColumn("DataAreaId", "SysScheduledNotifications");
        migrationBuilder.DropColumn("ExecutionUserId", "SysScheduledNotifications");
        migrationBuilder.DropColumn("OwnerAccountId", "SysScheduledNotifications");
        migrationBuilder.DropColumn("PreserveChannel", "SysScheduledNotifications");
        migrationBuilder.DropColumn("ClaimToken", "SysScheduledNotifications");
    }
}
