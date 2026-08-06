using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using IAX.IXApi.Modules.ERP.Shared.Features;
using IAX.IXApi.Modules.ERP.Common;

namespace IAX.IXApi.Modules.ERP.Entities
{
    [Table("ExchangeRateCurrencyPair")]
    public class ExchangeRateCurrencyPair : Entity<long>
    {
        //----------------------------------------- Core Information
        // Basic Properties
        [Required]
        [StringLength(FieldLengths.FromCurrencyCode)]
        public string FromCurrencyCode { get; set; } = string.Empty;

        [Required]
        [StringLength(FieldLengths.ToCurrencyCode)]
        public string ToCurrencyCode { get; set; } = string.Empty;

        public long ExchangeRateType { get; set; }

        // Enum Properties
        public ExchangeRateDisplayFactor ExchangeRateDisplayFactor { get; set; }

        /*
         
         ExchangeRateCurrencyPair.ExchangeRateType == ExchangeRateType.RecId
         ExchangeRateCurrencyPair.FromCurrencyCode == Currency.CurrencyCode
         ExchangeRateCurrencyPair.ToCurrencyCode == Currency.CurrencyCode
         
         */

        #region Navigation Properties Row

        [ForeignKey(nameof(ExchangeRateType))]
        public virtual ExchangeRateType? ExchangeRateTypeTable { get; set; }

        [ForeignKey(nameof(FromCurrencyCode))]
        public virtual Currency? FromCurrency { get; set; }

        [ForeignKey(nameof(ToCurrencyCode))]
        public virtual Currency? ToCurrency { get; set; }

        #endregion


        #region Navigation Properties List
        public virtual ICollection<ExchangeRate> ExchangeRates { get; set; }  = new HashSet<ExchangeRate>();

        #endregion
    }
}
