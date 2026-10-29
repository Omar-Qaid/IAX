using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace IAX.IXApi.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class InventoryCountingTransitAndScopedFifo : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<long>(
                name: "ReceiptTransRecId",
                table: "InventSettlement",
                type: "bigint",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "CountingConfirmed",
                table: "InventJournalTable",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "CountingFrozen",
                table: "InventJournalTable",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<string>(
                name: "ScopeItemId",
                table: "InventClosing",
                type: "nvarchar(20)",
                maxLength: 20,
                nullable: false,
                defaultValue: "");

            migrationBuilder.CreateTable(
                name: "InventTransferOrder",
                columns: table => new
                {
                    RECID = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    TransferId = table.Column<string>(type: "nvarchar(40)", maxLength: 40, nullable: false),
                    ItemId = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    Unit = table.Column<string>(type: "nvarchar(10)", maxLength: 10, nullable: false),
                    SiteId = table.Column<string>(type: "nvarchar(10)", maxLength: 10, nullable: false),
                    FromWarehouse = table.Column<string>(type: "nvarchar(10)", maxLength: 10, nullable: false),
                    TransitWarehouse = table.Column<string>(type: "nvarchar(10)", maxLength: 10, nullable: false),
                    ToWarehouse = table.Column<string>(type: "nvarchar(10)", maxLength: 10, nullable: false),
                    JournalNameId = table.Column<string>(type: "nvarchar(10)", maxLength: 10, nullable: false),
                    Quantity = table.Column<decimal>(type: "decimal(18,4)", precision: 18, scale: 4, nullable: false),
                    Shipped = table.Column<decimal>(type: "decimal(18,4)", precision: 18, scale: 4, nullable: false),
                    Received = table.Column<decimal>(type: "decimal(18,4)", precision: 18, scale: 4, nullable: false),
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
                    table.PrimaryKey("PK_InventTransferOrder", x => x.RECID);
                });

            migrationBuilder.CreateTable(
                name: "InventTransferPosting",
                columns: table => new
                {
                    RECID = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    TransferOrderRecId = table.Column<long>(type: "bigint", nullable: false),
                    RequestId = table.Column<string>(type: "nvarchar(40)", maxLength: 40, nullable: false),
                    JournalId = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    IsReceipt = table.Column<bool>(type: "bit", nullable: false),
                    Quantity = table.Column<decimal>(type: "decimal(18,4)", precision: 18, scale: 4, nullable: false),
                    PostingDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CostAmount = table.Column<decimal>(type: "decimal(18,4)", precision: 18, scale: 4, nullable: false),
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
                    table.PrimaryKey("PK_InventTransferPosting", x => x.RECID);
                    table.ForeignKey(
                        name: "FK_InventTransferPosting_InventTransferOrder_TransferOrderRecId",
                        column: x => x.TransferOrderRecId,
                        principalTable: "InventTransferOrder",
                        principalColumn: "RECID",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_InventTransferOrder_DataAreaId_TransferId",
                table: "InventTransferOrder",
                columns: new[] { "DataAreaId", "TransferId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_InventTransferPosting_DataAreaId_TransferOrderRecId_RequestId",
                table: "InventTransferPosting",
                columns: new[] { "DataAreaId", "TransferOrderRecId", "RequestId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_InventTransferPosting_TransferOrderRecId",
                table: "InventTransferPosting",
                column: "TransferOrderRecId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "InventTransferPosting");

            migrationBuilder.DropTable(
                name: "InventTransferOrder");

            migrationBuilder.DropColumn(
                name: "ReceiptTransRecId",
                table: "InventSettlement");

            migrationBuilder.DropColumn(
                name: "CountingConfirmed",
                table: "InventJournalTable");

            migrationBuilder.DropColumn(
                name: "CountingFrozen",
                table: "InventJournalTable");

            migrationBuilder.DropColumn(
                name: "ScopeItemId",
                table: "InventClosing");
        }
    }
}
