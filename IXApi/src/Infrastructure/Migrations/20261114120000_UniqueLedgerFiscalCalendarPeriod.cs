using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace IAX.IXApi.Infrastructure.Migrations;

public sealed partial class UniqueLedgerFiscalCalendarPeriod : Migration
{
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.Sql("""
            IF EXISTS (SELECT 1 FROM LedgerFiscalCalendarPeriod GROUP BY Ledger, FiscalCalendarPeriod HAVING COUNT(*) > 1)
                THROW 51000, 'Duplicate ledger fiscal calendar periods exist. Resolve duplicate status rows before applying this migration.', 1;
            """);
        migrationBuilder.DropIndex(
            name: "IX_LedgerFiscalCalendarPeriod_Ledger",
            table: "LedgerFiscalCalendarPeriod");
        migrationBuilder.CreateIndex(
            name: "IX_LedgerFiscalCalendarPeriod_Ledger_FiscalCalendarPeriod",
            table: "LedgerFiscalCalendarPeriod",
            columns: new[] { "Ledger", "FiscalCalendarPeriod" },
            unique: true);
    }

    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropIndex(name: "IX_LedgerFiscalCalendarPeriod_Ledger_FiscalCalendarPeriod", table: "LedgerFiscalCalendarPeriod");
        migrationBuilder.CreateIndex(
            name: "IX_LedgerFiscalCalendarPeriod_Ledger",
            table: "LedgerFiscalCalendarPeriod",
            column: "Ledger");
    }
}
