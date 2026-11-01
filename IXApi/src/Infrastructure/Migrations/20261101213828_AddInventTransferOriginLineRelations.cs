using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace IAX.IXApi.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddInventTransferOriginLineRelations : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "InventTransOriginTransferReceive",
                columns: table => new
                {
                    RECID = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    InventTransOrigin = table.Column<long>(type: "bigint", nullable: false),
                    TransferReceiveDataAreaId = table.Column<string>(type: "nvarchar(4)", maxLength: 4, nullable: false),
                    TransferReceiveId = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    TransferReceiveLineNum = table.Column<decimal>(type: "decimal(28,16)", precision: 28, scale: 16, nullable: false),
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
                    table.PrimaryKey("PK_InventTransOriginTransferReceive", x => x.RECID);
                });

            migrationBuilder.CreateTable(
                name: "InventTransOriginTransferTransitFrom",
                columns: table => new
                {
                    RECID = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    InventTransOrigin = table.Column<long>(type: "bigint", nullable: false),
                    TransferTransitFromDataAreaId = table.Column<string>(type: "nvarchar(4)", maxLength: 4, nullable: false),
                    TransferTransitFromId = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    TransferTransitFromLineNum = table.Column<decimal>(type: "decimal(28,16)", precision: 28, scale: 16, nullable: false),
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
                    table.PrimaryKey("PK_InventTransOriginTransferTransitFrom", x => x.RECID);
                });

            migrationBuilder.CreateTable(
                name: "InventTransOriginTransferTransitTo",
                columns: table => new
                {
                    RECID = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    InventTransOrigin = table.Column<long>(type: "bigint", nullable: false),
                    TransferTransitToDataAreaId = table.Column<string>(type: "nvarchar(4)", maxLength: 4, nullable: false),
                    TransferTransitToId = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    TransferTransitToLineNum = table.Column<decimal>(type: "decimal(28,16)", precision: 28, scale: 16, nullable: false),
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
                    table.PrimaryKey("PK_InventTransOriginTransferTransitTo", x => x.RECID);
                });

            migrationBuilder.CreateIndex(
                name: "IX_InventTransOriginTransferReceive_DataAreaId_InventTransOrigin",
                table: "InventTransOriginTransferReceive",
                columns: new[] { "DataAreaId", "InventTransOrigin" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_InventTransOriginTransferReceive_TransferReceiveDataAreaId_TransferReceiveId_TransferReceiveLineNum",
                table: "InventTransOriginTransferReceive",
                columns: new[] { "TransferReceiveDataAreaId", "TransferReceiveId", "TransferReceiveLineNum" });

            migrationBuilder.CreateIndex(
                name: "IX_InventTransOriginTransferTransitFrom_DataAreaId_InventTransOrigin",
                table: "InventTransOriginTransferTransitFrom",
                columns: new[] { "DataAreaId", "InventTransOrigin" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_InventTransOriginTransferTransitFrom_TransferTransitFromDataAreaId_TransferTransitFromId_TransferTransitFromLineNum",
                table: "InventTransOriginTransferTransitFrom",
                columns: new[] { "TransferTransitFromDataAreaId", "TransferTransitFromId", "TransferTransitFromLineNum" });

            migrationBuilder.CreateIndex(
                name: "IX_InventTransOriginTransferTransitTo_DataAreaId_InventTransOrigin",
                table: "InventTransOriginTransferTransitTo",
                columns: new[] { "DataAreaId", "InventTransOrigin" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_InventTransOriginTransferTransitTo_TransferTransitToDataAreaId_TransferTransitToId_TransferTransitToLineNum",
                table: "InventTransOriginTransferTransitTo",
                columns: new[] { "TransferTransitToDataAreaId", "TransferTransitToId", "TransferTransitToLineNum" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "InventTransOriginTransferReceive");

            migrationBuilder.DropTable(
                name: "InventTransOriginTransferTransitFrom");

            migrationBuilder.DropTable(
                name: "InventTransOriginTransferTransitTo");
        }
    }
}
