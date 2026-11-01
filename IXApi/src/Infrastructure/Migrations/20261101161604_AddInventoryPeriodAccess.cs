using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace IAX.IXApi.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddInventoryPeriodAccess : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "InventAccessLevel",
                table: "LedgerPeriodModuleAccessControl",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<string>(
                name: "InventUserGroupInfo",
                table: "LedgerPeriodModuleAccessControl",
                type: "nvarchar(60)",
                maxLength: 60,
                nullable: false,
                defaultValue: "");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "InventAccessLevel",
                table: "LedgerPeriodModuleAccessControl");

            migrationBuilder.DropColumn(
                name: "InventUserGroupInfo",
                table: "LedgerPeriodModuleAccessControl");
        }
    }
}
