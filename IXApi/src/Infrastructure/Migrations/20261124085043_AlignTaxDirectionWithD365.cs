using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace IAX.IXApi.Infrastructure.Migrations
{
    /// <summary>
    /// TaxDirection now follows the D365 values: IncomingTax 0, OutgoingTax 1, UseTax 2, TaxExemptPurchase 3,
    /// TaxExemptSales 4. Stored exempt values move up by one (3 first, so 2 does not collide). No column changes.
    /// </summary>
    public partial class AlignTaxDirectionWithD365 : Migration
    {
        private static readonly string[] Tables = ["TaxTrans", "TaxJournalTrans"];

        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            foreach (var table in Tables)
            {
                migrationBuilder.Sql($"UPDATE {table} SET TaxDirection = 4 WHERE TaxDirection = 3;");
                migrationBuilder.Sql($"UPDATE {table} SET TaxDirection = 3 WHERE TaxDirection = 2;");
            }
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            foreach (var table in Tables)
            {
                migrationBuilder.Sql($"UPDATE {table} SET TaxDirection = 2 WHERE TaxDirection = 3;");
                migrationBuilder.Sql($"UPDATE {table} SET TaxDirection = 3 WHERE TaxDirection = 4;");
            }
        }
    }
}
