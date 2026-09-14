using Microsoft.EntityFrameworkCore.Migrations;

namespace IAX.IXApi.Infrastructure.Migrations;

public partial class AddActivityControlMetadata : Migration
{
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        foreach (var name in new[] { "CanFilter", "CanGroup", "CanSort" })
            migrationBuilder.AddColumn<bool>(name: name, table: "WfActivityControls",
                type: "bit", nullable: false, defaultValue: true);

        migrationBuilder.AddColumn<string>(name: "ReferenceType", table: "WfActivityControls",
            type: "nvarchar(max)", nullable: true);
        migrationBuilder.AddColumn<string>(name: "FieldRole", table: "WfActivityControls",
            type: "nvarchar(max)", nullable: false, defaultValue: "Dimension");
        migrationBuilder.AddColumn<string>(name: "DataType", table: "WfActivityControls",
            type: "nvarchar(max)", nullable: false, defaultValue: "String");
        migrationBuilder.AddColumn<string>(name: "DefaultAggregation", table: "WfActivityControls",
            type: "nvarchar(max)", nullable: false, defaultValue: "NONE");
    }

    protected override void Down(MigrationBuilder migrationBuilder)
    {
        foreach (var name in new[] { "CanFilter", "CanGroup", "CanSort", "ReferenceType",
                     "FieldRole", "DataType", "DefaultAggregation" })
            migrationBuilder.DropColumn(name: name, table: "WfActivityControls");
    }
}
