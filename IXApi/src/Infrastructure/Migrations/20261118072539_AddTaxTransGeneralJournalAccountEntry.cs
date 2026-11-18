using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace IAX.IXApi.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddTaxTransGeneralJournalAccountEntry : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "TaxTransGeneralJournalAccountEntry",
                columns: table => new
                {
                    RECID = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    GeneralJournalAccountEntry = table.Column<long>(type: "bigint", nullable: true),
                    LedgerDimension = table.Column<long>(type: "bigint", nullable: true),
                    TaxTrans = table.Column<long>(type: "bigint", nullable: true),
                    TaxTransRelationship = table.Column<int>(type: "int", nullable: true),
                    TaxUncommitted = table.Column<long>(type: "bigint", nullable: true),
                    AccountingCurrencyAmount = table.Column<decimal>(type: "decimal(32,6)", precision: 32, scale: 6, nullable: true),
                    MonetaryAmountType = table.Column<int>(type: "int", nullable: true),
                    SubledgerJournalAccountEntry = table.Column<long>(type: "bigint", nullable: true),
                    TransactionCurrency = table.Column<string>(type: "nvarchar(3)", maxLength: 3, nullable: true),
                    TransactionCurrencyAmount = table.Column<decimal>(type: "decimal(32,6)", precision: 32, scale: 6, nullable: true),
                    AllocationFactor = table.Column<decimal>(type: "decimal(32,6)", precision: 32, scale: 6, nullable: true),
                    Partition = table.Column<long>(type: "bigint", nullable: false),
                    SysDataStateCode = table.Column<int>(type: "int", nullable: false),
                    CreatedBy = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    LastModifiedBy = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    LastModifiedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    OwnerAccountId = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    SysRowVersion = table.Column<byte[]>(type: "rowversion", rowVersion: true, nullable: false),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    RecVersion = table.Column<int>(type: "int", nullable: false),
                    DataAreaId = table.Column<string>(type: "nvarchar(4)", maxLength: 4, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TaxTransGeneralJournalAccountEntry", x => x.RECID);
                });

            migrationBuilder.CreateIndex(
                name: "IX_TaxTransGeneralJournalAccountEntry_DataAreaId_GeneralJournalAccountEntry",
                table: "TaxTransGeneralJournalAccountEntry",
                columns: new[] { "DataAreaId", "GeneralJournalAccountEntry" });

            migrationBuilder.CreateIndex(
                name: "IX_TaxTransGeneralJournalAccountEntry_DataAreaId_SubledgerJournalAccountEntry",
                table: "TaxTransGeneralJournalAccountEntry",
                columns: new[] { "DataAreaId", "SubledgerJournalAccountEntry" });

            migrationBuilder.CreateIndex(
                name: "IX_TaxTransGeneralJournalAccountEntry_DataAreaId_TaxTrans",
                table: "TaxTransGeneralJournalAccountEntry",
                columns: new[] { "DataAreaId", "TaxTrans" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "TaxTransGeneralJournalAccountEntry");
        }
    }
}
