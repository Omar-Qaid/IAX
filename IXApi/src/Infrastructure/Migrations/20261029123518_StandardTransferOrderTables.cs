using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace IAX.IXApi.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class StandardTransferOrderTables : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "InventTransferPosting");

            migrationBuilder.DropTable(
                name: "InventTransferOrder");

            migrationBuilder.CreateTable(
                name: "InventTransferLine",
                columns: table => new
                {
                    RECID = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    TransferId = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    LineNum = table.Column<decimal>(type: "decimal(28,16)", precision: 28, scale: 16, nullable: false),
                    ItemId = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    UnitId = table.Column<string>(type: "nvarchar(10)", maxLength: 10, nullable: false),
                    InventDimId = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    InventTransId = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    InventTransIdTransitTo = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    InventTransIdTransitFrom = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    InventTransIdReceive = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    InventTransIdScrap = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    QtyTransfer = table.Column<decimal>(type: "decimal(28,6)", precision: 28, scale: 6, nullable: false),
                    QtyShipNow = table.Column<decimal>(type: "decimal(28,6)", precision: 28, scale: 6, nullable: false),
                    QtyShipped = table.Column<decimal>(type: "decimal(28,6)", precision: 28, scale: 6, nullable: false),
                    QtyRemainShip = table.Column<decimal>(type: "decimal(28,6)", precision: 28, scale: 6, nullable: false),
                    QtyReceiveNow = table.Column<decimal>(type: "decimal(28,6)", precision: 28, scale: 6, nullable: false),
                    QtyReceived = table.Column<decimal>(type: "decimal(28,6)", precision: 28, scale: 6, nullable: false),
                    QtyRemainReceive = table.Column<decimal>(type: "decimal(28,6)", precision: 28, scale: 6, nullable: false),
                    QtyScrapped = table.Column<decimal>(type: "decimal(28,6)", precision: 28, scale: 6, nullable: false),
                    ShipDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    ReceiveDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    OverDeliveryPct = table.Column<decimal>(type: "decimal(28,6)", precision: 28, scale: 6, nullable: false),
                    UnderDeliveryPct = table.Column<decimal>(type: "decimal(28,6)", precision: 28, scale: 6, nullable: false),
                    RemainStatus = table.Column<int>(type: "int", nullable: false),
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
                    table.PrimaryKey("PK_InventTransferLine", x => x.RECID);
                });

            migrationBuilder.CreateTable(
                name: "InventTransferTable",
                columns: table => new
                {
                    RECID = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    TransferId = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    TransferStatus = table.Column<int>(type: "int", nullable: false),
                    InventLocationIdFrom = table.Column<string>(type: "nvarchar(10)", maxLength: 10, nullable: false),
                    InventLocationIdTransit = table.Column<string>(type: "nvarchar(10)", maxLength: 10, nullable: false),
                    InventLocationIdTo = table.Column<string>(type: "nvarchar(10)", maxLength: 10, nullable: false),
                    ShipDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    ReceiveDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    DlvModeId = table.Column<string>(type: "nvarchar(10)", maxLength: 10, nullable: false),
                    DlvTermId = table.Column<string>(type: "nvarchar(10)", maxLength: 10, nullable: false),
                    FromAddressName = table.Column<string>(type: "nvarchar(60)", maxLength: 60, nullable: false),
                    ToAddressName = table.Column<string>(type: "nvarchar(60)", maxLength: 60, nullable: false),
                    FromPostalAddress = table.Column<long>(type: "bigint", nullable: false),
                    ToPostalAddress = table.Column<long>(type: "bigint", nullable: false),
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
                    table.PrimaryKey("PK_InventTransferTable", x => x.RECID);
                });

            migrationBuilder.CreateIndex(
                name: "IX_InventTransferLine_DataAreaId_TransferId_LineNum",
                table: "InventTransferLine",
                columns: new[] { "DataAreaId", "TransferId", "LineNum" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_InventTransferTable_DataAreaId_TransferId",
                table: "InventTransferTable",
                columns: new[] { "DataAreaId", "TransferId" },
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "InventTransferLine");

            migrationBuilder.DropTable(
                name: "InventTransferTable");

            migrationBuilder.CreateTable(
                name: "InventTransferOrder",
                columns: table => new
                {
                    RECID = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    CreatedBy = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    DataAreaId = table.Column<string>(type: "nvarchar(4)", maxLength: 4, nullable: false),
                    FromWarehouse = table.Column<string>(type: "nvarchar(10)", maxLength: 10, nullable: false),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false),
                    ItemId = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    JournalNameId = table.Column<string>(type: "nvarchar(10)", maxLength: 10, nullable: false),
                    LastModifiedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    LastModifiedBy = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    OwnerAccountId = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Quantity = table.Column<decimal>(type: "decimal(18,4)", precision: 18, scale: 4, nullable: false),
                    RecVersion = table.Column<int>(type: "int", nullable: false),
                    Received = table.Column<decimal>(type: "decimal(18,4)", precision: 18, scale: 4, nullable: false),
                    RowVersion = table.Column<byte[]>(type: "rowversion", rowVersion: true, nullable: false),
                    Shipped = table.Column<decimal>(type: "decimal(18,4)", precision: 18, scale: 4, nullable: false),
                    SiteId = table.Column<string>(type: "nvarchar(10)", maxLength: 10, nullable: false),
                    ToWarehouse = table.Column<string>(type: "nvarchar(10)", maxLength: 10, nullable: false),
                    TransferId = table.Column<string>(type: "nvarchar(40)", maxLength: 40, nullable: false),
                    TransitWarehouse = table.Column<string>(type: "nvarchar(10)", maxLength: 10, nullable: false),
                    Unit = table.Column<string>(type: "nvarchar(10)", maxLength: 10, nullable: false)
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
                    CostAmount = table.Column<decimal>(type: "decimal(18,4)", precision: 18, scale: 4, nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    CreatedBy = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    DataAreaId = table.Column<string>(type: "nvarchar(4)", maxLength: 4, nullable: false),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false),
                    IsReceipt = table.Column<bool>(type: "bit", nullable: false),
                    JournalId = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    LastModifiedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    LastModifiedBy = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    OwnerAccountId = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    PostingDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    Quantity = table.Column<decimal>(type: "decimal(18,4)", precision: 18, scale: 4, nullable: false),
                    RecVersion = table.Column<int>(type: "int", nullable: false),
                    RequestId = table.Column<string>(type: "nvarchar(40)", maxLength: 40, nullable: false),
                    RowVersion = table.Column<byte[]>(type: "rowversion", rowVersion: true, nullable: false),
                    TransferOrderRecId = table.Column<long>(type: "bigint", nullable: false)
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
    }
}
