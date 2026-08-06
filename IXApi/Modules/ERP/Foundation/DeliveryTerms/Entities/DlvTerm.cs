using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using IAX.IXApi.Modules.ERP.Shared.Features;
using IAX.IXApi.Modules.ERP.Common;

namespace IAX.IXApi.Modules.ERP.Entities
{
    [Table("DlvTerm")]
    public class DlvTerm : Entity<long>
    {
        //----------------------------------------- Core Information
        // Basic Properties
        [Required]
        [StringLength(FieldLengths.Code)]
        public string Code { get; set; } = string.Empty;

        [Required]
        [StringLength(FieldLengths.Txt)]
        public string Txt { get; set; } = string.Empty;

        // ==========================================================
        // Freight, Logistics & Tax Locations
        // ==========================================================
        // Basic Properties
        public decimal ShipCarrierFreeMinimum { get; set; }

        // Enum Properties
        public int FreightChargeTerm { get; set; } // Map to FreightChargeTerm enum if preferred
        public int TaxLocationRole { get; set; } // Map to TaxLocationRole enum if preferred

        // ==========================================================
        // Landed Cost & Global Trade (ITM)
        // ==========================================================
        // Enum Properties
        public NoYes ItmGoodsInTransitControl { get; set; }
        public NoYes ItmPortMandatory { get; set; }
    }
}