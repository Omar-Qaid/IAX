using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace IAX.IXApi.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddMainAccountAvailabilityAndUserValidation : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<DateTime>(
                name: "ActiveFrom",
                table: "MainAccount",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "ActiveTo",
                table: "MainAccount",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "Suspended",
                table: "MainAccount",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<string>(
                name: "UserId",
                table: "MainAccount",
                type: "nvarchar(8)",
                maxLength: 8,
                nullable: false,
                defaultValue: "");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ActiveFrom",
                table: "MainAccount");

            migrationBuilder.DropColumn(
                name: "ActiveTo",
                table: "MainAccount");

            migrationBuilder.DropColumn(
                name: "Suspended",
                table: "MainAccount");

            migrationBuilder.DropColumn(
                name: "UserId",
                table: "MainAccount");
        }
    }
}
