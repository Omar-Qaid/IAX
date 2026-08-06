using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using IAX.IXApi.Modules.ERP.Shared.Features;
using IAX.IXApi.Modules.ERP.Common;

namespace IAX.IXApi.Modules.ERP.Entities
{
    [Table("ExchangeRateType")]
    public class ExchangeRateType : Entity<long>
    {
        //----------------------------------------- Core Information
        // Basic Properties
        [Required]
        [StringLength(FieldLengths.Name)]
        public string Name { get; set; } = string.Empty;

        [Required]
        [StringLength(FieldLengths.Description)]
        public string Description { get; set; } = string.Empty;


        #region Navigation Properties List
        [InverseProperty(nameof(ExchangeRateCurrencyPair.ExchangeRateTypeTable))]
        public virtual ICollection<ExchangeRateCurrencyPair> ExchangeRateCurrencyPairs { get; set; } = new HashSet<ExchangeRateCurrencyPair>();
        #endregion
    }
}