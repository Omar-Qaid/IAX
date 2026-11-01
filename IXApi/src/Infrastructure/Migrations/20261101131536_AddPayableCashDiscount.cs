using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace IAX.IXApi.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddPayableCashDiscount : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<DateTime>(
                name: "CashDiscBaseDate",
                table: "VendTrans",
                type: "datetime2",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.AddColumn<string>(
                name: "CashDiscCode",
                table: "VendTrans",
                type: "nvarchar(10)",
                maxLength: 10,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<decimal>(
                name: "UtilizedCashDisc",
                table: "VendSettlement",
                type: "decimal(18,4)",
                precision: 18,
                scale: 4,
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<string>(
                name: "CashDisc",
                table: "PurchTable",
                type: "nvarchar(10)",
                maxLength: 10,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<DateTime>(
                name: "CashDiscBaseDate",
                table: "PurchTable",
                type: "datetime2",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "CashDiscBaseDate",
                table: "VendTrans");

            migrationBuilder.DropColumn(
                name: "CashDiscCode",
                table: "VendTrans");

            migrationBuilder.DropColumn(
                name: "UtilizedCashDisc",
                table: "VendSettlement");

            migrationBuilder.DropColumn(
                name: "CashDisc",
                table: "PurchTable");

            migrationBuilder.DropColumn(
                name: "CashDiscBaseDate",
                table: "PurchTable");
        }
    }
}
