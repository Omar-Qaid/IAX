using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace IAX.IXApi.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class VendorInstallmentOpenRows : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_VendTransOpen_DataAreaId_RefRecId",
                table: "VendTransOpen");

            migrationBuilder.AddColumn<DateTime>(
                name: "DueDate",
                table: "VendSettlement",
                type: "datetime2",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.AddColumn<long>(
                name: "TransOpen",
                table: "VendSettlement",
                type: "bigint",
                nullable: false,
                defaultValue: 0L);

            migrationBuilder.CreateIndex(
                name: "IX_VendTransOpen_DataAreaId_RefRecId",
                table: "VendTransOpen",
                columns: new[] { "DataAreaId", "RefRecId" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_VendTransOpen_DataAreaId_RefRecId",
                table: "VendTransOpen");

            migrationBuilder.DropColumn(
                name: "DueDate",
                table: "VendSettlement");

            migrationBuilder.DropColumn(
                name: "TransOpen",
                table: "VendSettlement");

            migrationBuilder.CreateIndex(
                name: "IX_VendTransOpen_DataAreaId_RefRecId",
                table: "VendTransOpen",
                columns: new[] { "DataAreaId", "RefRecId" },
                unique: true);
        }
    }
}
