using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace IAX.IXApi.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class LedgerParametersAndMainAccountLegalEntity : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "LedgerParameters",
                columns: table => new
                {
                    RECID = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Key_ = table.Column<int>(type: "int", nullable: false),
                    AbortErrorAccount = table.Column<long>(type: "bigint", nullable: false),
                    CheckContinuous = table.Column<int>(type: "int", nullable: false),
                    CheckVoucher = table.Column<int>(type: "int", nullable: false),
                    CheckJournalizing = table.Column<int>(type: "int", nullable: false),
                    AllowMultipleTransactionsWithinOneVoucher = table.Column<int>(type: "int", nullable: false),
                    DimensionValuesUsedForSummaryAccount = table.Column<int>(type: "int", nullable: false),
                    ReverseOppositeSignAmounts = table.Column<int>(type: "int", nullable: false),
                    TransactionReversalCorrection = table.Column<int>(type: "int", nullable: false),
                    MaxRoundingDifferenceMst = table.Column<decimal>(type: "decimal(28,6)", precision: 28, scale: 6, nullable: false),
                    MaxRoundingDifferenceMstSecond = table.Column<decimal>(type: "decimal(28,6)", precision: 28, scale: 6, nullable: false),
                    LimitedOpenFiscalPeriods = table.Column<int>(type: "int", nullable: false),
                    PeriodClosingPosting = table.Column<int>(type: "int", nullable: false),
                    DeleteOpening = table.Column<int>(type: "int", nullable: false),
                    OpeningVoucher = table.Column<int>(type: "int", nullable: false),
                    OpeningWithoutBudget = table.Column<int>(type: "int", nullable: false),
                    YearClosed = table.Column<int>(type: "int", nullable: false),
                    EnableLedgerSettlement = table.Column<int>(type: "int", nullable: false),
                    AdvancedLedgerSettlement = table.Column<int>(type: "int", nullable: false),
                    EnableLedgerSettlementPartialSettlement = table.Column<int>(type: "int", nullable: false),
                    EnableLedgerSettlementPostRealizedGainLoss = table.Column<int>(type: "int", nullable: false),
                    EnableLedgerSettlementSubledgerSettlement = table.Column<int>(type: "int", nullable: false),
                    EnableLedgerSettlementAdvancedAwarenessOptions = table.Column<int>(type: "int", nullable: false),
                    EnableLedgerSettlementProcessAutomation = table.Column<int>(type: "int", nullable: false),
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
                    table.PrimaryKey("PK_LedgerParameters", x => x.RECID);
                });

            migrationBuilder.CreateTable(
                name: "MainAccountLegalEntity",
                columns: table => new
                {
                    RECID = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    MainAccount = table.Column<long>(type: "bigint", nullable: false),
                    LegalEntity = table.Column<long>(type: "bigint", nullable: false),
                    DefaultDimension = table.Column<long>(type: "bigint", nullable: false),
                    FixedDimensions = table.Column<long>(type: "bigint", nullable: false),
                    DoNotAllowManualEntry = table.Column<int>(type: "int", nullable: false),
                    AutoAllocate = table.Column<int>(type: "int", nullable: false),
                    TaxCode = table.Column<string>(type: "nvarchar(10)", maxLength: 10, nullable: false),
                    TaxGroup = table.Column<string>(type: "nvarchar(10)", maxLength: 10, nullable: false),
                    TaxItemGroup = table.Column<string>(type: "nvarchar(10)", maxLength: 10, nullable: false),
                    TaxTableDataAreaId = table.Column<string>(type: "nvarchar(4)", maxLength: 4, nullable: false),
                    TaxItemGroupHeadingDataAreaId = table.Column<string>(type: "nvarchar(4)", maxLength: 4, nullable: false),
                    TaxDirection = table.Column<int>(type: "int", nullable: false),
                    TaxFree = table.Column<int>(type: "int", nullable: false),
                    ValidateTaxCode = table.Column<int>(type: "int", nullable: false),
                    ExchangeAdjustmentRateType = table.Column<long>(type: "bigint", nullable: false),
                    ReportingExchangeAdjustmentRateType = table.Column<long>(type: "bigint", nullable: false),
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
                    table.PrimaryKey("PK_MainAccountLegalEntity", x => x.RECID);
                    table.ForeignKey(
                        name: "FK_MainAccountLegalEntity_MainAccount_MainAccount",
                        column: x => x.MainAccount,
                        principalTable: "MainAccount",
                        principalColumn: "RECID",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_LedgerParameters_DataAreaId_Key_",
                table: "LedgerParameters",
                columns: new[] { "DataAreaId", "Key_" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_MainAccountLegalEntity_MainAccount_LegalEntity",
                table: "MainAccountLegalEntity",
                columns: new[] { "MainAccount", "LegalEntity" },
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "LedgerParameters");

            migrationBuilder.DropTable(
                name: "MainAccountLegalEntity");
        }
    }
}
