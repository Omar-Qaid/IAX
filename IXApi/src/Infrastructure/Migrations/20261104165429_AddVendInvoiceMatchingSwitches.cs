using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace IAX.IXApi.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddVendInvoiceMatchingSwitches : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "MatchingPolicy",
                table: "VendParameters",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "MatchingPolicyAllowOverride",
                table: "VendParameters",
                type: "int",
                nullable: false,
                defaultValue: 1);

            migrationBuilder.AddColumn<int>(
                name: "UseInvoiceMatching",
                table: "VendParameters",
                type: "int",
                nullable: false,
                defaultValue: 1);

            migrationBuilder.AddColumn<int>(
                name: "UseTotalPriceMatching",
                table: "VendParameters",
                type: "int",
                nullable: false,
                defaultValue: 1);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "MatchingPolicy",
                table: "VendParameters");

            migrationBuilder.DropColumn(
                name: "MatchingPolicyAllowOverride",
                table: "VendParameters");

            migrationBuilder.DropColumn(
                name: "UseInvoiceMatching",
                table: "VendParameters");

            migrationBuilder.DropColumn(
                name: "UseTotalPriceMatching",
                table: "VendParameters");
        }
    }
}
