using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace IAX.IXApi.Infrastructure.Migrations;

public partial class AddRequestDetailControlLabels : Migration
{
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.AddColumn<string>(name: "ControlLabel", table: "WfRequestDetails",
            type: "nvarchar(max)", nullable: false, defaultValue: "");
        migrationBuilder.AddColumn<string>(name: "ControlLabelAlias", table: "WfRequestDetails",
            type: "nvarchar(max)", nullable: false, defaultValue: "");
    }

    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropColumn(name: "ControlLabel", table: "WfRequestDetails");
        migrationBuilder.DropColumn(name: "ControlLabelAlias", table: "WfRequestDetails");
    }
}
