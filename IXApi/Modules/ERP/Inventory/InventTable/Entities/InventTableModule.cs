using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using IAX.IXApi.Modules.ERP.Shared.Features;
using IAX.IXApi.Modules.ERP.Common;
using IAX.IXApi.Shared.Domain.Entities;
using IAX.IXApi.Modules.ERP.Entities;
using IAX.IXApi.Modules.Organization.Employees.Entities;
using IAX.IXApi.Modules.Administration.AuditLogs.Entities;
using IAX.IXApi.Modules.Administration.DataManagement.Contracts;
using IAX.IXApi.Infrastructure.Persistence.Seeding.Entities;

namespace IAX.IXApi.Modules.ERP.Entities
{
    [Table("InventTableModule")]
    public class InventTableModule : Entity<long>
    {
        //----------------------------------------- Core Identity & Module Context
        // Basic Properties
        [Required]
        [StringLength(FieldLengths.ItemId)]
        public string ItemId { get; set; } = string.Empty;

        // Enum Properties
        public ModuleInventPurchSales ModuleType { get; set; } // 0: Inventory, 1: Purchase, 2: Sales

        // ==========================================================
        // Logistical Units & Delivery Tolerances
        // ==========================================================
        // Basic Properties
        [Required]
        [StringLength(FieldLengths.UnitId)]
        public string UnitId { get; set; } = string.Empty;

        public decimal OverDeliveryPct { get; set; }
        public decimal UnderDeliveryPct { get; set; }

        // Enum Properties
        public NoYes IntercompanyBlocked { get; set; }

        // ==========================================================
        // Base Pricing, Unit Metrics & Markup Controls
        // ==========================================================
        // Basic Properties
        public decimal Price { get; set; }
        public decimal PriceQty { get; set; }
        public decimal PriceUnit { get; set; }
        public decimal Markup { get; set; }
        public DateTime PriceDate { get; set; }

        [Required]
        [StringLength(FieldLengths.MarkupGroupId)]
        public string MarkupGroupId { get; set; } = string.Empty;

        // Enum Properties
        public NoYes AllocateMarkup { get; set; }
        public BasePricePurchase BasePricePurchase { get; set; } // Pricing calculation mechanism context

        // ==========================================================
        // Trade Agreement Discounts & Taxation Overrides
        // ==========================================================
        // Basic Properties
        [Required]
        [StringLength(FieldLengths.LineDisc)]
        public string LineDisc { get; set; } = string.Empty; // Item discount group string link

        [Required]
        [StringLength(FieldLengths.TaxItemGroupId)]
        public string TaxItemGroupId { get; set; } = string.Empty;

        // Enum Properties
        public NoYes EndDisc { get; set; } // Determines eligibility for total/end discount calculations

        // ==========================================================
        // Commerce / Retail Multi-Channel Constraints
        // ==========================================================
        // Basic Properties
        public decimal RetailInventoryAvailabilityBuffer { get; set; }

        [Required]
        [StringLength(FieldLengths.RetailInventoryAvailabilityLevelProfile)]
        public string RetailInventoryAvailabilityLevelProfile { get; set; } = string.Empty;

        // ==========================================================
        // Process Manufacturing Pricing Customization
        // ==========================================================
        // Basic Properties
        public int PdsPricingPrecision { get; set; }


        #region Navigation Properties Row

//         [ForeignKey(nameof(ItemId))]
//         public virtual InventTable? ReleasedProduct { get; set; }

//         [ForeignKey(nameof(UnitId))]
//         public virtual UnitOfMeasure? UnitOfMeasure { get; set; }

//         [ForeignKey(nameof(TaxItemGroupId))]
//         public virtual TaxItemGroupHeading? TaxItemGroupHeading { get; set; }

        #endregion
    }
}
