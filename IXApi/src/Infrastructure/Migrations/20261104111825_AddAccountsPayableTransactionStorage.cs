using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace IAX.IXApi.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddAccountsPayableTransactionStorage : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "AccountingDistribution",
                columns: table => new
                {
                    RECID = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    AccountingDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    AccountingEvent = table.Column<long>(type: "bigint", nullable: true),
                    AccountingLegalEntity = table.Column<long>(type: "bigint", nullable: true),
                    AllocationFactor = table.Column<decimal>(type: "decimal(28,12)", precision: 28, scale: 12, nullable: true),
                    AmountSource = table.Column<int>(type: "int", nullable: true),
                    FinalizeAccountingEvent = table.Column<long>(type: "bigint", nullable: true),
                    LedgerDimension = table.Column<long>(type: "bigint", nullable: true),
                    MonetaryAmount = table.Column<int>(type: "int", nullable: true),
                    Number = table.Column<long>(type: "bigint", nullable: true),
                    ParentDistribution = table.Column<long>(type: "bigint", nullable: true),
                    ReferenceDistribution = table.Column<long>(type: "bigint", nullable: true),
                    ReferenceRole = table.Column<int>(type: "int", nullable: true),
                    SourceDocumentHeader = table.Column<long>(type: "bigint", nullable: true),
                    SourceDocumentLine = table.Column<long>(type: "bigint", nullable: true),
                    TransactionCurrency = table.Column<string>(type: "nvarchar(3)", maxLength: 3, nullable: true),
                    TransactionCurrencyAmount = table.Column<decimal>(type: "decimal(28,12)", precision: 28, scale: 12, nullable: true),
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
                    table.PrimaryKey("PK_AccountingDistribution", x => x.RECID);
                    table.ForeignKey(
                        name: "FK_AccountingDistribution_AccountingDistribution_ParentDistribution",
                        column: x => x.ParentDistribution,
                        principalTable: "AccountingDistribution",
                        principalColumn: "RECID",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_AccountingDistribution_AccountingDistribution_ReferenceDistribution",
                        column: x => x.ReferenceDistribution,
                        principalTable: "AccountingDistribution",
                        principalColumn: "RECID",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_AccountingDistribution_CompanyInfo_AccountingLegalEntity",
                        column: x => x.AccountingLegalEntity,
                        principalTable: "CompanyInfo",
                        principalColumn: "RECID",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_AccountingDistribution_Currency_TransactionCurrency",
                        column: x => x.TransactionCurrency,
                        principalTable: "Currency",
                        principalColumn: "CurrencyCode",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_AccountingDistribution_DimensionAttributeValueCombination_LedgerDimension",
                        column: x => x.LedgerDimension,
                        principalTable: "DimensionAttributeValueCombination",
                        principalColumn: "RECID",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "SubledgerJournalEntry",
                columns: table => new
                {
                    RECID = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    AccountingEvent = table.Column<long>(type: "bigint", nullable: true),
                    FiscalCalendarPeriod = table.Column<long>(type: "bigint", nullable: true),
                    JournalNumber = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    Ledger = table.Column<long>(type: "bigint", nullable: true),
                    PostingLayer = table.Column<int>(type: "int", nullable: false),
                    ReferenceIdentityTransferStatus = table.Column<int>(type: "int", nullable: true),
                    Status = table.Column<int>(type: "int", nullable: true),
                    TransferId = table.Column<long>(type: "bigint", nullable: true),
                    Type = table.Column<int>(type: "int", nullable: false),
                    Voucher = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    VoucherDataAreaId = table.Column<string>(type: "nvarchar(4)", maxLength: 4, nullable: true),
                    TransTxt = table.Column<string>(type: "nvarchar(150)", maxLength: 150, nullable: true),
                    DocumentDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    DocumentNumber = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
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
                    table.PrimaryKey("PK_SubledgerJournalEntry", x => x.RECID);
                    table.ForeignKey(
                        name: "FK_SubledgerJournalEntry_FiscalCalendarPeriod_FiscalCalendarPeriod",
                        column: x => x.FiscalCalendarPeriod,
                        principalTable: "FiscalCalendarPeriod",
                        principalColumn: "RECID",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_SubledgerJournalEntry_Ledger_Ledger",
                        column: x => x.Ledger,
                        principalTable: "Ledger",
                        principalColumn: "RECID",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "VendInvoiceInfoSubLine",
                columns: table => new
                {
                    RECID = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    DocumentId = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    ParmId = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    LineRefRecId = table.Column<long>(type: "bigint", nullable: true),
                    JournalRefRecId = table.Column<long>(type: "bigint", nullable: true),
                    JournalRefTableId = table.Column<int>(type: "int", nullable: true),
                    ReceiveNow = table.Column<decimal>(type: "decimal(28,12)", precision: 28, scale: 12, nullable: true),
                    InventNow = table.Column<decimal>(type: "decimal(28,12)", precision: 28, scale: 12, nullable: true),
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
                    table.PrimaryKey("PK_VendInvoiceInfoSubLine", x => x.RECID);
                    table.ForeignKey(
                        name: "FK_VendInvoiceInfoSubLine_VendInvoiceInfoLine_LineRefRecId",
                        column: x => x.LineRefRecId,
                        principalTable: "VendInvoiceInfoLine",
                        principalColumn: "RECID",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "SubledgerJournalAccountEntry",
                columns: table => new
                {
                    RECID = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    SubledgerJournalEntry = table.Column<long>(type: "bigint", nullable: true),
                    LedgerDimension = table.Column<long>(type: "bigint", nullable: false),
                    PostingType = table.Column<int>(type: "int", nullable: false),
                    Side = table.Column<int>(type: "int", nullable: true),
                    IsCorrection = table.Column<int>(type: "int", nullable: true),
                    IsTransferredInDetail = table.Column<int>(type: "int", nullable: true),
                    OriginalTransferPolicy = table.Column<int>(type: "int", nullable: true),
                    AccountEntryEventRole = table.Column<int>(type: "int", nullable: true),
                    TransactionCurrency = table.Column<string>(type: "nvarchar(3)", maxLength: 3, nullable: true),
                    TransactionCurrencyAmount = table.Column<decimal>(type: "decimal(28,12)", precision: 28, scale: 12, nullable: true),
                    AccountingCurrencyAmount = table.Column<decimal>(type: "decimal(28,12)", precision: 28, scale: 12, nullable: true),
                    ReportingCurrencyAmount = table.Column<decimal>(type: "decimal(28,12)", precision: 28, scale: 12, nullable: true),
                    ExchangeRate1 = table.Column<decimal>(type: "decimal(28,12)", precision: 28, scale: 12, nullable: true),
                    ExchangeRate2 = table.Column<decimal>(type: "decimal(28,12)", precision: 28, scale: 12, nullable: true),
                    ReportingExchangeRate1 = table.Column<decimal>(type: "decimal(28,12)", precision: 28, scale: 12, nullable: true),
                    ReportingExchangeRate2 = table.Column<decimal>(type: "decimal(28,12)", precision: 28, scale: 12, nullable: true),
                    ExchangeRateDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    HistoricalExchangeRateDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    GeneralJournalAccountEntry = table.Column<long>(type: "bigint", nullable: true),
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
                    table.PrimaryKey("PK_SubledgerJournalAccountEntry", x => x.RECID);
                    table.ForeignKey(
                        name: "FK_SubledgerJournalAccountEntry_Currency_TransactionCurrency",
                        column: x => x.TransactionCurrency,
                        principalTable: "Currency",
                        principalColumn: "CurrencyCode",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_SubledgerJournalAccountEntry_DimensionAttributeValueCombination_LedgerDimension",
                        column: x => x.LedgerDimension,
                        principalTable: "DimensionAttributeValueCombination",
                        principalColumn: "RECID",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_SubledgerJournalAccountEntry_GeneralJournalAccountEntry_GeneralJournalAccountEntry",
                        column: x => x.GeneralJournalAccountEntry,
                        principalTable: "GeneralJournalAccountEntry",
                        principalColumn: "RECID",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_SubledgerJournalAccountEntry_SubledgerJournalEntry_SubledgerJournalEntry",
                        column: x => x.SubledgerJournalEntry,
                        principalTable: "SubledgerJournalEntry",
                        principalColumn: "RECID",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_AccountingDistribution_AccountingLegalEntity",
                table: "AccountingDistribution",
                column: "AccountingLegalEntity");

            migrationBuilder.CreateIndex(
                name: "IX_AccountingDistribution_LedgerDimension",
                table: "AccountingDistribution",
                column: "LedgerDimension");

            migrationBuilder.CreateIndex(
                name: "IX_AccountingDistribution_ParentDistribution",
                table: "AccountingDistribution",
                column: "ParentDistribution");

            migrationBuilder.CreateIndex(
                name: "IX_AccountingDistribution_ReferenceDistribution",
                table: "AccountingDistribution",
                column: "ReferenceDistribution");

            migrationBuilder.CreateIndex(
                name: "IX_AccountingDistribution_SourceDocumentHeader",
                table: "AccountingDistribution",
                column: "SourceDocumentHeader");

            migrationBuilder.CreateIndex(
                name: "IX_AccountingDistribution_SourceDocumentLine",
                table: "AccountingDistribution",
                column: "SourceDocumentLine");

            migrationBuilder.CreateIndex(
                name: "IX_AccountingDistribution_TransactionCurrency",
                table: "AccountingDistribution",
                column: "TransactionCurrency");

            migrationBuilder.CreateIndex(
                name: "IX_SubledgerJournalAccountEntry_GeneralJournalAccountEntry",
                table: "SubledgerJournalAccountEntry",
                column: "GeneralJournalAccountEntry");

            migrationBuilder.CreateIndex(
                name: "IX_SubledgerJournalAccountEntry_LedgerDimension",
                table: "SubledgerJournalAccountEntry",
                column: "LedgerDimension");

            migrationBuilder.CreateIndex(
                name: "IX_SubledgerJournalAccountEntry_SubledgerJournalEntry",
                table: "SubledgerJournalAccountEntry",
                column: "SubledgerJournalEntry");

            migrationBuilder.CreateIndex(
                name: "IX_SubledgerJournalAccountEntry_TransactionCurrency",
                table: "SubledgerJournalAccountEntry",
                column: "TransactionCurrency");

            migrationBuilder.CreateIndex(
                name: "IX_SubledgerJournalEntry_AccountingEvent",
                table: "SubledgerJournalEntry",
                column: "AccountingEvent");

            migrationBuilder.CreateIndex(
                name: "IX_SubledgerJournalEntry_FiscalCalendarPeriod",
                table: "SubledgerJournalEntry",
                column: "FiscalCalendarPeriod");

            migrationBuilder.CreateIndex(
                name: "IX_SubledgerJournalEntry_Ledger",
                table: "SubledgerJournalEntry",
                column: "Ledger");

            migrationBuilder.CreateIndex(
                name: "IX_SubledgerJournalEntry_VoucherDataAreaId_Voucher",
                table: "SubledgerJournalEntry",
                columns: new[] { "VoucherDataAreaId", "Voucher" });

            migrationBuilder.CreateIndex(
                name: "IX_VendInvoiceInfoSubLine_DataAreaId_ParmId_LineRefRecId",
                table: "VendInvoiceInfoSubLine",
                columns: new[] { "DataAreaId", "ParmId", "LineRefRecId" });

            migrationBuilder.CreateIndex(
                name: "IX_VendInvoiceInfoSubLine_JournalRefTableId_JournalRefRecId",
                table: "VendInvoiceInfoSubLine",
                columns: new[] { "JournalRefTableId", "JournalRefRecId" });

            migrationBuilder.CreateIndex(
                name: "IX_VendInvoiceInfoSubLine_LineRefRecId",
                table: "VendInvoiceInfoSubLine",
                column: "LineRefRecId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "AccountingDistribution");

            migrationBuilder.DropTable(
                name: "SubledgerJournalAccountEntry");

            migrationBuilder.DropTable(
                name: "VendInvoiceInfoSubLine");

            migrationBuilder.DropTable(
                name: "SubledgerJournalEntry");
        }
    }
}
