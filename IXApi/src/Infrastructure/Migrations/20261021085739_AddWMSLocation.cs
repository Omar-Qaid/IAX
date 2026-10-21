using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace IAX.IXApi.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddWMSLocation : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "WMSLocation",
                columns: table => new
                {
                    RECID = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    InventLocationId = table.Column<string>(type: "nvarchar(10)", maxLength: 10, nullable: false),
                    WMSLocationId = table.Column<string>(type: "nvarchar(10)", maxLength: 10, nullable: false),
                    InputLocation = table.Column<string>(type: "nvarchar(10)", maxLength: 10, nullable: false),
                    CheckText = table.Column<string>(type: "nvarchar(512)", maxLength: 512, nullable: false),
                    LocationType = table.Column<int>(type: "int", nullable: false),
                    SortCode = table.Column<int>(type: "int", nullable: false),
                    ManualName = table.Column<int>(type: "int", nullable: false),
                    CreatedBy = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    LastModifiedBy = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    LastModifiedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    OwnerAccountId = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    RowVersion = table.Column<byte[]>(type: "rowversion", rowVersion: true, nullable: false),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    RecVersion = table.Column<int>(type: "int", nullable: false),
                    DataAreaId = table.Column<string>(type: "nvarchar(4)", maxLength: 4, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_WMSLocation", x => x.RECID);
                });

            migrationBuilder.CreateIndex(
                name: "IX_WMSLocation_DataAreaId_InventLocationId_WMSLocationId",
                table: "WMSLocation",
                columns: new[] { "DataAreaId", "InventLocationId", "WMSLocationId" },
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "WMSLocation");
        }
    }
}
