using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace IAX.IXApi.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddVendExtendedPriceMatching : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<decimal>(
                name: "ExtendedPriceToleranceAmount",
                table: "VendParameters",
                type: "decimal(32,6)",
                precision: 18,
                scale: 4,
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<decimal>(
                name: "ExtendedPriceTolerancePercentage",
                table: "VendParameters",
                type: "decimal(32,6)",
                precision: 18,
                scale: 4,
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<int>(
                name: "UseExtendedPriceMatching",
                table: "VendParameters",
                type: "int",
                nullable: false,
                defaultValue: 0);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ExtendedPriceToleranceAmount",
                table: "VendParameters");

            migrationBuilder.DropColumn(
                name: "ExtendedPriceTolerancePercentage",
                table: "VendParameters");

            migrationBuilder.DropColumn(
                name: "UseExtendedPriceMatching",
                table: "VendParameters");
        }
    }
}
