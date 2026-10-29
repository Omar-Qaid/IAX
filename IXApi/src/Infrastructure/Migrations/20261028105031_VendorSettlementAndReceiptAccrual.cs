using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace IAX.IXApi.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class VendorSettlementAndReceiptAccrual : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<decimal>(
                name: "AccrualReversalAmountMst",
                table: "VendInvoicePackingSlipQuantityMatch",
                type: "decimal(18,4)",
                precision: 18,
                scale: 4,
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<long>(
                name: "ReceiptAccrualRecId",
                table: "VendInvoicePackingSlipQuantityMatch",
                type: "bigint",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "PurchReceiptAccrual",
                columns: table => new
                {
                    RECID = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    PackingSlipTransRecId = table.Column<long>(type: "bigint", nullable: false),
                    GeneralJournalEntryRecId = table.Column<long>(type: "bigint", nullable: false),
                    InventoryLedgerDimension = table.Column<long>(type: "bigint", nullable: false),
                    AccrualLedgerDimension = table.Column<long>(type: "bigint", nullable: false),
                    InventQuantity = table.Column<decimal>(type: "decimal(18,6)", precision: 18, scale: 6, nullable: false),
                    AmountMst = table.Column<decimal>(type: "decimal(18,4)", precision: 18, scale: 4, nullable: false),
                    ReversedInventQuantity = table.Column<decimal>(type: "decimal(18,6)", precision: 18, scale: 6, nullable: false),
                    ReversedAmountMst = table.Column<decimal>(type: "decimal(18,4)", precision: 18, scale: 4, nullable: false),
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
                    table.PrimaryKey("PK_PurchReceiptAccrual", x => x.RECID);
                    table.ForeignKey(
                        name: "FK_PurchReceiptAccrual_DimensionAttributeValueCombination_AccrualLedgerDimension",
                        column: x => x.AccrualLedgerDimension,
                        principalTable: "DimensionAttributeValueCombination",
                        principalColumn: "RECID",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_PurchReceiptAccrual_DimensionAttributeValueCombination_InventoryLedgerDimension",
                        column: x => x.InventoryLedgerDimension,
                        principalTable: "DimensionAttributeValueCombination",
                        principalColumn: "RECID",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_PurchReceiptAccrual_GeneralJournalEntry_GeneralJournalEntryRecId",
                        column: x => x.GeneralJournalEntryRecId,
                        principalTable: "GeneralJournalEntry",
                        principalColumn: "RECID",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_PurchReceiptAccrual_VendPackingSlipTrans_PackingSlipTransRecId",
                        column: x => x.PackingSlipTransRecId,
                        principalTable: "VendPackingSlipTrans",
                        principalColumn: "RECID",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "VendSettlement",
                columns: table => new
                {
                    RECID = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    AccountNum = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    InvoiceTransRecId = table.Column<long>(type: "bigint", nullable: false),
                    PaymentTransRecId = table.Column<long>(type: "bigint", nullable: false),
                    TransDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    Voucher = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    AmountCur = table.Column<decimal>(type: "decimal(18,4)", precision: 18, scale: 4, nullable: false),
                    AmountMst = table.Column<decimal>(type: "decimal(18,4)", precision: 18, scale: 4, nullable: false),
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
                    table.PrimaryKey("PK_VendSettlement", x => x.RECID);
                    table.ForeignKey(
                        name: "FK_VendSettlement_VendTrans_InvoiceTransRecId",
                        column: x => x.InvoiceTransRecId,
                        principalTable: "VendTrans",
                        principalColumn: "RECID",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_VendSettlement_VendTrans_PaymentTransRecId",
                        column: x => x.PaymentTransRecId,
                        principalTable: "VendTrans",
                        principalColumn: "RECID",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "VendTransOpen",
                columns: table => new
                {
                    RECID = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    AccountNum = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    RefRecId = table.Column<long>(type: "bigint", nullable: false),
                    TransDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    DueDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    AmountCur = table.Column<decimal>(type: "decimal(18,4)", precision: 18, scale: 4, nullable: false),
                    AmountMst = table.Column<decimal>(type: "decimal(18,4)", precision: 18, scale: 4, nullable: false),
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
                    table.PrimaryKey("PK_VendTransOpen", x => x.RECID);
                    table.ForeignKey(
                        name: "FK_VendTransOpen_VendTrans_RefRecId",
                        column: x => x.RefRecId,
                        principalTable: "VendTrans",
                        principalColumn: "RECID",
                        onDelete: ReferentialAction.Restrict);
                });

            // Preserve historic transactions and derive only their outstanding balances.
            // No historic settlement links or receipt accruals can safely be inferred.
            migrationBuilder.Sql("""
                INSERT INTO [VendTransOpen]
                    ([AccountNum], [RefRecId], [TransDate], [DueDate], [AmountCur], [AmountMst],
                     [DataAreaId], [IsDeleted], [IsActive], [RecVersion], [OwnerAccountId], [CreatedAt], [CreatedBy])
                SELECT [AccountNum], [RECID], [TransDate], [DueDate],
                       [AmountCur] - [SettleAmountCur], [AmountMst] - [SettleAmountMst],
                       [DataAreaId], 0, [IsActive], 1, [OwnerAccountId], SYSUTCDATETIME(), N'Migration'
                FROM [VendTrans]
                WHERE [IsDeleted] = 0 AND [Closed] < '19000101'
                  AND ([AmountCur] <> [SettleAmountCur] OR [AmountMst] <> [SettleAmountMst]);
                """);

            migrationBuilder.CreateIndex(
                name: "IX_VendInvoicePackingSlipQuantityMatch_ReceiptAccrualRecId",
                table: "VendInvoicePackingSlipQuantityMatch",
                column: "ReceiptAccrualRecId");

            migrationBuilder.CreateIndex(
                name: "IX_PurchReceiptAccrual_AccrualLedgerDimension",
                table: "PurchReceiptAccrual",
                column: "AccrualLedgerDimension");

            migrationBuilder.CreateIndex(
                name: "IX_PurchReceiptAccrual_DataAreaId_PackingSlipTransRecId",
                table: "PurchReceiptAccrual",
                columns: new[] { "DataAreaId", "PackingSlipTransRecId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_PurchReceiptAccrual_GeneralJournalEntryRecId",
                table: "PurchReceiptAccrual",
                column: "GeneralJournalEntryRecId");

            migrationBuilder.CreateIndex(
                name: "IX_PurchReceiptAccrual_InventoryLedgerDimension",
                table: "PurchReceiptAccrual",
                column: "InventoryLedgerDimension");

            migrationBuilder.CreateIndex(
                name: "IX_PurchReceiptAccrual_PackingSlipTransRecId",
                table: "PurchReceiptAccrual",
                column: "PackingSlipTransRecId");

            migrationBuilder.CreateIndex(
                name: "IX_VendSettlement_DataAreaId_InvoiceTransRecId_PaymentTransRecId",
                table: "VendSettlement",
                columns: new[] { "DataAreaId", "InvoiceTransRecId", "PaymentTransRecId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_VendSettlement_InvoiceTransRecId",
                table: "VendSettlement",
                column: "InvoiceTransRecId");

            migrationBuilder.CreateIndex(
                name: "IX_VendSettlement_PaymentTransRecId",
                table: "VendSettlement",
                column: "PaymentTransRecId");

            migrationBuilder.CreateIndex(
                name: "IX_VendTransOpen_DataAreaId_RefRecId",
                table: "VendTransOpen",
                columns: new[] { "DataAreaId", "RefRecId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_VendTransOpen_RefRecId",
                table: "VendTransOpen",
                column: "RefRecId");

            migrationBuilder.AddForeignKey(
                name: "FK_VendInvoicePackingSlipQuantityMatch_PurchReceiptAccrual_ReceiptAccrualRecId",
                table: "VendInvoicePackingSlipQuantityMatch",
                column: "ReceiptAccrualRecId",
                principalTable: "PurchReceiptAccrual",
                principalColumn: "RECID",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_VendInvoicePackingSlipQuantityMatch_PurchReceiptAccrual_ReceiptAccrualRecId",
                table: "VendInvoicePackingSlipQuantityMatch");

            migrationBuilder.DropTable(
                name: "PurchReceiptAccrual");

            migrationBuilder.DropTable(
                name: "VendSettlement");

            migrationBuilder.DropTable(
                name: "VendTransOpen");

            migrationBuilder.DropIndex(
                name: "IX_VendInvoicePackingSlipQuantityMatch_ReceiptAccrualRecId",
                table: "VendInvoicePackingSlipQuantityMatch");

            migrationBuilder.DropColumn(
                name: "AccrualReversalAmountMst",
                table: "VendInvoicePackingSlipQuantityMatch");

            migrationBuilder.DropColumn(
                name: "ReceiptAccrualRecId",
                table: "VendInvoicePackingSlipQuantityMatch");
        }
    }
}
