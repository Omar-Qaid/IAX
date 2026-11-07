using System;
using IAX.IXApi.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;

namespace IAX.IXApi.Infrastructure.Migrations;

[DbContext(typeof(ApplicationDbContext))]
[Migration("20261107150000_AddBankAccountStatements")]
public sealed class AddBankAccountStatements : Migration
{
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.CreateTable(
            name: "BankAccountStatement",
            columns: table => new
            {
                RECID = table.Column<long>(type: "bigint", nullable: false)
                    .Annotation("SqlServer:Identity", "1, 1"),
                DataAreaId = table.Column<string>(type: "nvarchar(4)", maxLength: 4, nullable: false),
                AccountId = table.Column<string>(type: "nvarchar(10)", maxLength: 10, nullable: false),
                AccountStatementNum = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                AccountStatementDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                CurrencyCode = table.Column<string>(type: "nvarchar(3)", maxLength: 3, nullable: false),
                EndingBalance = table.Column<decimal>(type: "decimal(18,4)", precision: 18, scale: 4, nullable: false),
                ReconcileDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                CancelDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                CreatedBy = table.Column<string>(type: "nvarchar(max)", nullable: true),
                CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                LastModifiedBy = table.Column<string>(type: "nvarchar(max)", nullable: true),
                LastModifiedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                OwnerAccountId = table.Column<string>(type: "nvarchar(max)", nullable: true),
                RowVersion = table.Column<byte[]>(type: "rowversion", rowVersion: true, nullable: false),
                IsDeleted = table.Column<bool>(type: "bit", nullable: false),
                IsActive = table.Column<bool>(type: "bit", nullable: false),
                RecVersion = table.Column<int>(type: "int", nullable: false)
            },
            constraints: table => table.PrimaryKey("PK_BankAccountStatement", x => x.RECID));
        migrationBuilder.CreateIndex(
            name: "IX_BankAccountStatement_DataAreaId_AccountId_AccountStatementNum",
            table: "BankAccountStatement",
            columns: new[] { "DataAreaId", "AccountId", "AccountStatementNum" },
            unique: true);
    }

    protected override void Down(MigrationBuilder migrationBuilder) =>
        migrationBuilder.DropTable(name: "BankAccountStatement");
}
