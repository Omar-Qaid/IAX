using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using IAX.IXApi.Modules.Finance.Common;

namespace IAX.IXApi.Modules.Finance.Entities
{
    /// <summary>
    /// Exchange gain and loss accounts for a ledger, one row per account type. This is the ledger
    /// level of D365's lookup: the per-currency override in
    /// <see cref="CurrencyLedgerGainLossAccount"/> is consulted first, and this row is the
    /// fallback when no currency-specific account is configured.
    ///
    /// Only the realized types are used by the basic cycle, which posts the realized difference
    /// when a payment settles a foreign-currency transaction at a different rate. The unrealized
    /// types are part of revaluation of open balances, which is outside the basic scope.
    /// </summary>
    [Table("LedgerGainLossAccount")]
    public class LedgerGainLossAccount : Entity<long>
    {
        public long Ledger { get; set; }

        public CurrencyGainLossAccountType AccountType { get; set; }

        public long LedgerDimension { get; set; }
    }

    /// <summary>
    /// Exchange gain and loss accounts for one currency within a ledger. Overrides
    /// <see cref="LedgerGainLossAccount"/> for that currency.
    /// </summary>
    [Table("CurrencyLedgerGainLossAccount")]
    public class CurrencyLedgerGainLossAccount : Entity<long>
    {
        public long Ledger { get; set; }

        [Required]
        [StringLength(FieldLengths.CurrencyCode)]
        public string CurrencyCode { get; set; } = string.Empty;

        public CurrencyGainLossAccountType AccountType { get; set; }

        public long LedgerDimension { get; set; }
    }
}
