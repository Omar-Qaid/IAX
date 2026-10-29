using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace IAX.IXApi.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class LedgerTransSettlement : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "LedgerTransSettlement",
                columns: table => new
                {
                    RECID = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    SettleId = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    TransRecId = table.Column<long>(type: "bigint", nullable: false),
                    RefTransRecId = table.Column<long>(type: "bigint", nullable: false),
                    SettlementGroup = table.Column<long>(type: "bigint", nullable: false),
                    SettlementTableId = table.Column<int>(type: "int", nullable: false),
                    OriginalSettleId = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    SettleDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    DateProcessed = table.Column<DateTime>(type: "datetime2", nullable: false),
                    Voucher = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    UserId = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    AutomationRule = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    SettleTransactionCurrencyAmount = table.Column<decimal>(type: "decimal(28,6)", precision: 28, scale: 6, nullable: false),
                    SettleAccountingCurrencyAmount = table.Column<decimal>(type: "decimal(28,6)", precision: 28, scale: 6, nullable: false),
                    SettleReportingCurrencyAmount = table.Column<decimal>(type: "decimal(28,6)", precision: 28, scale: 6, nullable: false),
                    ExchAdjAccountingCurrencyAmount = table.Column<decimal>(type: "decimal(28,6)", precision: 28, scale: 6, nullable: false),
                    ExchAdjReportingCurrencyAmount = table.Column<decimal>(type: "decimal(28,6)", precision: 28, scale: 6, nullable: false),
                    Reason = table.Column<long>(type: "bigint", nullable: false),
                    SysDataStateCode = table.Column<int>(type: "int", nullable: false),
                    IsReversed = table.Column<int>(type: "int", nullable: false),
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
                    table.PrimaryKey("PK_LedgerTransSettlement", x => x.RECID);
                    table.ForeignKey(
                        name: "FK_LedgerTransSettlement_GeneralJournalAccountEntry_TransRecId",
                        column: x => x.TransRecId,
                        principalTable: "GeneralJournalAccountEntry",
                        principalColumn: "RECID",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_LedgerTransSettlement_DataAreaId_SettleId",
                table: "LedgerTransSettlement",
                columns: new[] { "DataAreaId", "SettleId" });

            migrationBuilder.CreateIndex(
                name: "IX_LedgerTransSettlement_DataAreaId_TransRecId",
                table: "LedgerTransSettlement",
                columns: new[] { "DataAreaId", "TransRecId" });

            migrationBuilder.CreateIndex(
                name: "IX_LedgerTransSettlement_TransRecId",
                table: "LedgerTransSettlement",
                column: "TransRecId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "LedgerTransSettlement");
        }
    }
}
