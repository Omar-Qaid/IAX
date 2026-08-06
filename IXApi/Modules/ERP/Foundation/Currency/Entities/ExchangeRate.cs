using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using IAX.IXApi.Modules.ERP.Shared.Features;
using IAX.IXApi.Modules.ERP.Common;

namespace IAX.IXApi.Modules.ERP.Entities
{
    [Table("ExchangeRate")]
    public class ExchangeRate : Entity<long>
    {
        //----------------------------------------- Core Information
        // Basic Properties
        public decimal ExchangeRateValue { get; set; } // Renamed from EXCHANGERATE to avoid conflict with Class Name
        public long ExchangeRateCurrencyPair { get; set; }

        // ==========================================================
        // Validity & Timestamps
        // ==========================================================
        // Basic Properties
        public DateTime ValidFrom { get; set; }
        public DateTime ValidTo { get; set; }


        #region Navigation Properties Row

        [ForeignKey(nameof(ExchangeRateCurrencyPair))]
        public virtual ExchangeRateCurrencyPair? ExchangeRateCurrencyPairTable { get; set; }

        #endregion
    }
}
