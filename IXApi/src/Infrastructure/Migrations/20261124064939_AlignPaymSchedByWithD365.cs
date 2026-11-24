using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace IAX.IXApi.Infrastructure.Migrations
{
    /// <summary>
    /// PaySched.PayBy now uses the D365 enum PaymSchedBy (Total 0, AmountByPayment 1, NumOfPayment 2, Specified 3)
    /// instead of the earlier Equal 0 / Amount 1. Stored values keep their meaning: "Equal" (equal installments) is
    /// D365 NumOfPayment, and "Amount" (a fixed amount per installment) is D365 AmountByPayment, which keeps value 1.
    /// No column changes.
    /// </summary>
    public partial class AlignPaymSchedByWithD365 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("UPDATE PaymSched SET PayBy = 2 WHERE PayBy = 0;");
            // PaymSchedLine.PercentAmount is the D365 enum PercentAmount (Percent 0, Amount 1); the percentage itself
            // belongs in Value. Earlier data stored the percentage in PercentAmount with Value 0: move it.
            migrationBuilder.Sql("UPDATE PaymSchedLine SET Value = PercentAmount, PercentAmount = 0 WHERE PercentAmount > 1 AND Value = 0;");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("UPDATE PaymSched SET PayBy = 0 WHERE PayBy = 2;");
        }
    }
}
