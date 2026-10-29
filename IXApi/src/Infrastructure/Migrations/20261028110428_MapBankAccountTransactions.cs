using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace IAX.IXApi.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class MapBankAccountTransactions : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "BankAccountTrans",
                columns: table => new
                {
                    RECID = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    AccountId = table.Column<string>(type: "nvarchar(10)", maxLength: 10, nullable: false),
                    TransDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    Voucher = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    Txt = table.Column<string>(type: "nvarchar(150)", maxLength: 150, nullable: false),
                    BankTransType = table.Column<string>(type: "nvarchar(10)", maxLength: 10, nullable: false),
                    LedgerTransType = table.Column<int>(type: "int", nullable: false),
                    CurrencyCode = table.Column<string>(type: "nvarchar(3)", maxLength: 3, nullable: false),
                    AmountCur = table.Column<decimal>(type: "decimal(18,4)", precision: 18, scale: 4, nullable: false),
                    AmountMst = table.Column<decimal>(type: "decimal(18,4)", precision: 18, scale: 4, nullable: false),
                    AmountReportingCurrency = table.Column<decimal>(type: "decimal(18,4)", precision: 18, scale: 4, nullable: false),
                    ExchRateMst = table.Column<decimal>(type: "decimal(28,12)", precision: 28, scale: 12, nullable: false),
                    ExchRateRep = table.Column<decimal>(type: "decimal(28,12)", precision: 28, scale: 12, nullable: false),
                    BankTransCurrencyCode = table.Column<string>(type: "nvarchar(3)", maxLength: 3, nullable: false),
                    BankTransAmountCur = table.Column<decimal>(type: "decimal(18,4)", precision: 18, scale: 4, nullable: false),
                    AmountCorrect = table.Column<decimal>(type: "decimal(18,4)", precision: 18, scale: 4, nullable: false),
                    PaymentMode = table.Column<string>(type: "nvarchar(10)", maxLength: 10, nullable: false),
                    PaymReference = table.Column<string>(type: "nvarchar(35)", maxLength: 35, nullable: false),
                    ChequeNum = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    DepositNum = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    AccountStatement = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    AccountStatementDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    ClearedDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    AcknowledgementDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    Reconciled = table.Column<int>(type: "int", nullable: false),
                    Included = table.Column<int>(type: "int", nullable: false),
                    LedgerDimension = table.Column<long>(type: "bigint", nullable: false),
                    DefaultDimension = table.Column<long>(type: "bigint", nullable: false),
                    SourceRecId = table.Column<long>(type: "bigint", nullable: false),
                    SourceTableId = table.Column<int>(type: "int", nullable: false),
                    ReasonRefRecId = table.Column<long>(type: "bigint", nullable: false),
                    Cancel = table.Column<int>(type: "int", nullable: false),
                    CancelPending = table.Column<int>(type: "int", nullable: false),
                    Manual = table.Column<int>(type: "int", nullable: false),
                    IsSummarization = table.Column<int>(type: "int", nullable: false),
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
                    table.PrimaryKey("PK_BankAccountTrans", x => x.RECID);
                });

            migrationBuilder.CreateIndex(
                name: "IX_BankAccountTrans_DataAreaId_AccountId_TransDate",
                table: "BankAccountTrans",
                columns: new[] { "DataAreaId", "AccountId", "TransDate" });

            migrationBuilder.CreateIndex(
                name: "IX_BankAccountTrans_DataAreaId_SourceTableId_SourceRecId",
                table: "BankAccountTrans",
                columns: new[] { "DataAreaId", "SourceTableId", "SourceRecId" },
                unique: true,
                filter: "[SourceTableId] > 0 AND [SourceRecId] > 0");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "BankAccountTrans");
        }
    }
}
