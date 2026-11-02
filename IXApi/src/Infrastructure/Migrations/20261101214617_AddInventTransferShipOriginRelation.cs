using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace IAX.IXApi.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddInventTransferShipOriginRelation : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "InventTransOriginTransferShip",
                columns: table => new
                {
                    RECID = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    InventTransOrigin = table.Column<long>(type: "bigint", nullable: false),
                    TransferShipDataAreaId = table.Column<string>(type: "nvarchar(4)", maxLength: 4, nullable: false),
                    TransferShipId = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    TransferShipLineNum = table.Column<decimal>(type: "decimal(28,16)", precision: 28, scale: 16, nullable: false),
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
                    table.PrimaryKey("PK_InventTransOriginTransferShip", x => x.RECID);
                });

            migrationBuilder.CreateIndex(
                name: "IX_InventTransOriginTransferShip_DataAreaId_InventTransOrigin",
                table: "InventTransOriginTransferShip",
                columns: new[] { "DataAreaId", "InventTransOrigin" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_InventTransOriginTransferShip_TransferShipDataAreaId_TransferShipId_TransferShipLineNum",
                table: "InventTransOriginTransferShip",
                columns: new[] { "TransferShipDataAreaId", "TransferShipId", "TransferShipLineNum" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "InventTransOriginTransferShip");
        }
    }
}
