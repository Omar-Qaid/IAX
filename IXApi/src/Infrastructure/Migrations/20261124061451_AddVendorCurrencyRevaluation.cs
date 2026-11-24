using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace IAX.IXApi.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddVendorCurrencyRevaluation : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<DateTime>(
                name: "LastExchAdj",
                table: "VendTrans",
                type: "datetime2",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.AddColumn<string>(
                name: "LastExchAdjVoucher",
                table: "VendTrans",
                type: "nvarchar(100)",
                maxLength: 100,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<decimal>(
                name: "ReportingExchAdjustmentUnrealized",
                table: "VendTrans",
                type: "decimal(18,4)",
                precision: 18,
                scale: 4,
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<decimal>(
                name: "VendExchAdjustmentUnrealized",
                table: "VendTrans",
                type: "decimal(18,4)",
                precision: 18,
                scale: 4,
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.CreateTable(
                name: "VendExchRateAdjustment",
                columns: table => new
                {
                    RECID = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ConsideredDate = table.Column<DateTime>(type: "date", nullable: true),
                    DateOfRate = table.Column<DateTime>(type: "date", nullable: true),
                    Dimensions = table.Column<int>(type: "int", nullable: true),
                    ExchPrinciple = table.Column<int>(type: "int", nullable: true),
                    ExecutedDate = table.Column<DateTime>(type: "date", nullable: true),
                    Notes = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    PostingProfile = table.Column<string>(type: "nvarchar(10)", maxLength: 10, nullable: true),
                    Reviewed = table.Column<int>(type: "int", nullable: true),
                    UseProfile = table.Column<int>(type: "int", nullable: true),
                    CreatedBy = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    LastModifiedBy = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    LastModifiedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    OwnerAccountId = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    RowVersion = table.Column<byte[]>(type: "rowversion", rowVersion: true, nullable: false),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    RecVersion = table.Column<int>(type: "int", nullable: false),
                    DataAreaId = table.Column<string>(type: "nvarchar(10)", maxLength: 10, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_VendExchRateAdjustment", x => x.RECID);
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "VendExchRateAdjustment");

            migrationBuilder.DropColumn(
                name: "LastExchAdj",
                table: "VendTrans");

            migrationBuilder.DropColumn(
                name: "LastExchAdjVoucher",
                table: "VendTrans");

            migrationBuilder.DropColumn(
                name: "ReportingExchAdjustmentUnrealized",
                table: "VendTrans");

            migrationBuilder.DropColumn(
                name: "VendExchAdjustmentUnrealized",
                table: "VendTrans");
        }
    }
}
