using IAX.IXApi.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace IAX.IXApi.Infrastructure.Migrations
{
    [DbContext(typeof(ApplicationDbContext))]
    [Migration("20260815180000_UnifyWorkflowOptionName")]
    public partial class UnifyWorkflowOptionName : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "NameAr",
                table: "WfRequestControlsOptions",
                newName: "Name");

            migrationBuilder.RenameColumn(
                name: "NameAr",
                table: "WfActivityControlsOptions",
                newName: "Name");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "Name",
                table: "WfRequestControlsOptions",
                newName: "NameAr");

            migrationBuilder.RenameColumn(
                name: "Name",
                table: "WfActivityControlsOptions",
                newName: "NameAr");
        }
    }
}
