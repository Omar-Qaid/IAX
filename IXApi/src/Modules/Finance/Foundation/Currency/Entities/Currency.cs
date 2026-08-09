using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using IAX.IXApi.Modules.Finance.Shared.Features;
using IAX.IXApi.Modules.Finance.Common;

namespace IAX.IXApi.Modules.Finance.Entities
{
    [Table("Currency")]
    public class Currency : Entity<long>
    {
        //----------------------------------------- Core Information
        // Basic Properties
        [Required]
        [StringLength(FieldLengths.CurrencyCode)]
        public string CurrencyCode { get; set; } = string.Empty;

        [Required]
        [StringLength(FieldLengths.CurrencyCodeIso)]
        public string CurrencyCodeIso { get; set; } = string.Empty;

        [StringLength(FieldLengths.Txt)]
        public string Txt { get; set; } = string.Empty;

        [StringLength(FieldLengths.Symbol)]
        public string Symbol { get; set; } = string.Empty;

        // Enum Properties
        public NoYes IsEuro { get; set; }

        // ==========================================================
        // Sales Rounding rules
        // ==========================================================
        // Basic Properties
        public decimal RoundOffSales { get; set; }

        // Enum Properties
        public RoundOffType RoundOffTypeSales { get; set; }

        // ==========================================================
        // Purchase Rounding rules
        // ==========================================================
        // Basic Properties
        public decimal RoundOffPurch { get; set; }

        // Enum Properties
        public RoundOffType RoundOffTypePurch { get; set; }

        // ==========================================================
        // Price Rounding rules
        // ==========================================================
        // Basic Properties
        public decimal RoundOffPrice { get; set; }

        // Enum Properties
        public RoundOffType RoundOffTypePrice { get; set; }

        // ==========================================================
        // General & Line Amount Precision Rules
        // ==========================================================
        // Basic Properties
        public decimal RoundingPrecision { get; set; }
        public decimal LtmRoundOffLineAmount { get; set; }

        // Enum Properties
        public RoundOffType LtmRoundOffTypeLineAmount { get; set; }

        #region Navigation Properties List
          [InverseProperty(nameof(ExchangeRateCurrencyPair.FromCurrency))]
          public virtual ICollection<ExchangeRateCurrencyPair> FromExchangeRateCurrencyPairs { get; set; } = new HashSet<ExchangeRateCurrencyPair>();
         
          [InverseProperty(nameof(ExchangeRateCurrencyPair.ToCurrency))]
          public virtual ICollection<ExchangeRateCurrencyPair> ToExchangeRateCurrencyPairs { get; set; }  = new HashSet<ExchangeRateCurrencyPair>();
        #endregion
    }
}
