using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace IAX.IXApi.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddLedgerGainLossAccounts : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "CurrencyLedgerGainLossAccount",
                columns: table => new
                {
                    RECID = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Ledger = table.Column<long>(type: "bigint", nullable: false),
                    CurrencyCode = table.Column<string>(type: "nvarchar(3)", maxLength: 3, nullable: false),
                    AccountType = table.Column<int>(type: "int", nullable: false),
                    LedgerDimension = table.Column<long>(type: "bigint", nullable: false),
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
                    table.PrimaryKey("PK_CurrencyLedgerGainLossAccount", x => x.RECID);
                });

            migrationBuilder.CreateTable(
                name: "LedgerGainLossAccount",
                columns: table => new
                {
                    RECID = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Ledger = table.Column<long>(type: "bigint", nullable: false),
                    AccountType = table.Column<int>(type: "int", nullable: false),
                    LedgerDimension = table.Column<long>(type: "bigint", nullable: false),
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
                    table.PrimaryKey("PK_LedgerGainLossAccount", x => x.RECID);
                });

            migrationBuilder.CreateIndex(
                name: "IX_CurrencyLedgerGainLossAccount_DataAreaId_Ledger_CurrencyCode_AccountType",
                table: "CurrencyLedgerGainLossAccount",
                columns: new[] { "DataAreaId", "Ledger", "CurrencyCode", "AccountType" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_LedgerGainLossAccount_DataAreaId_Ledger_AccountType",
                table: "LedgerGainLossAccount",
                columns: new[] { "DataAreaId", "Ledger", "AccountType" },
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "CurrencyLedgerGainLossAccount");

            migrationBuilder.DropTable(
                name: "LedgerGainLossAccount");
        }
    }
}
