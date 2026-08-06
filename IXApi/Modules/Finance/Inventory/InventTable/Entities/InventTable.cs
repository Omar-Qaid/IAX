using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using IAX.IXApi.Modules.Finance.Shared.Features;
using IAX.IXApi.Modules.Finance.Common;
using IAX.IXApi.Modules.Finance.Inventory.InventTable;
using IAX.IXApi.Shared.Domain.Entities;
using IAX.IXApi.Modules.Finance.Entities;
using IAX.IXApi.Modules.Organization.Employees.Entities;
using IAX.IXApi.Modules.Administration.AuditLogs.Entities;
using IAX.IXApi.Modules.Administration.DataManagement.Contracts;
using IAX.IXApi.Infrastructure.Persistence.Seeding.Entities;
using Microsoft.EntityFrameworkCore;
using DocumentFormat.OpenXml.Vml.Office;


namespace IAX.IXApi.Modules.Finance.Entities
{
    [Table("InventTable")]
    public class InventTable : Entity<long>
    {
        //----------------------------------------- Core Identifiers & Master References
        // Basic Properties
        [Required]
        [StringLength(FieldLengths.ItemId)]
        public string ItemId { get; set; } = string.Empty;

        public long Product { get; set; } // Global Product Master Definition Link

        [Required]
        [StringLength(FieldLengths.NameAlias)]
        public string NameAlias { get; set; } = string.Empty;

        [Required]
        [StringLength(FieldLengths.Sku)]
        public string Sku { get; set; } = string.Empty;

        // Enum Properties
        public ItemType ItemType { get; set; } // Service, Item, Product, etc.
        public UseAltItemId UseAltItemId { get; set; }

        // ==========================================================
        // Classification & Analytical Demarcations (ABC Metrics)
        // ==========================================================
        // Basic Properties
        public int SortCode { get; set; }

        // Enum Properties
        public ABCValue ABCValue { get; set; }
        public ABCRevenue ABCRevenue { get; set; }
        public ABCTieUp ABCTieUp { get; set; }
        public ABCContributionMargin ABCContributionMargin { get; set; }

        // ==========================================================
        // Physical Dimensions & Packaging Weight Metrics
        // ==========================================================
        // Basic Properties
        public decimal NetWeight { get; set; }
        public decimal TaraWeight { get; set; }
        public decimal GrossHeight { get; set; }
        public decimal GrossWidth { get; set; }
        public decimal GrossDepth { get; set; }
        public decimal Height { get; set; }
        public decimal Width { get; set; }
        public decimal Depth { get; set; }
        public decimal Density { get; set; }
        public decimal UnitVolume { get; set; }
        public decimal StatisticsFactor { get; set; }

        // ==========================================================
        // Sourcing, Purchasing & Trade Frameworks
        // ==========================================================
        // Basic Properties
        [Required]
        [StringLength(FieldLengths.PrimaryVendorID)]
        public string PrimaryVendorID { get; set; } = string.Empty;

        [Required]
        [StringLength(FieldLengths.ItemBuyerGroupId)]
        public string ItemBuyerGroupId { get; set; } = string.Empty;

        // Enum Properties
        public PriceModel PurchModel { get; set; }
        public MatchingPolicy MatchingPolicy { get; set; } // Three-way matching control policies

        // ==========================================================
        // Price Calculation, Sales Margins & Markup Structures
        // ==========================================================
        // Basic Properties
        public decimal SalesPercentMarkup { get; set; }
        public decimal SalesContributionRatio { get; set; }
        public decimal MarketLowestPrice { get; set; }

        // Enum Properties
        public PriceModel SalesModel { get; set; }
        public SalesPriceModelBasic SalesPriceModelBasic { get; set; }

        // ==========================================================
        // Inventory Financial Valuations & Cost Settings
        // ==========================================================
        // Basic Properties
        [Required]
        [StringLength(FieldLengths.CostGroupId)]
        public string CostGroupId { get; set; } = string.Empty;

        public int CostBomLevel { get; set; }

        // Enum Properties
        public PriceModel CostModel { get; set; }
        public NoYes ItemDimCostPrice { get; set; } // Flag indicating if item variant tracks individual costings

        // ==========================================================
        // Fiscal LIFO Calculations (Localization Rules)
        // ==========================================================
        // Basic Properties
        public long InventFiscalLifoGroup { get; set; }
        public decimal FiscalLifoNormalValue { get; set; }

        // Enum Properties
        public NoYes FiscalLifoAvoidCalc { get; set; }
        public NoYes FiscalLifoNormalValueCalc { get; set; }

        // ==========================================================
        // Bills of Materials (BOM) & Production Engineering
        // ==========================================================
        // Basic Properties
        [Required]
        [StringLength(FieldLengths.BomUnitId)]
        public string BomUnitId { get; set; } = string.Empty;

        [Required]
        [StringLength(FieldLengths.BomCalcGroupId)]
        public string BomCalcGroupId { get; set; } = string.Empty;

        [Required]
        [StringLength(FieldLengths.ProdOriginId)]
        public string ProdOriginId { get; set; } = string.Empty;

        [Required]
        [StringLength(FieldLengths.ProdGroupId)]
        public string ProdGroupId { get; set; } = string.Empty;

        public int BomLevel { get; set; }
        public decimal ScrapConst { get; set; }
        public decimal ScrapVar { get; set; }

        // Enum Properties
        public NoYes Phantom { get; set; }
        public NoYes Bundle { get; set; }
        public NoYes AutoReportFinished { get; set; }
        public NoYes BomManualReceipt { get; set; }
        public BomWhsReleasePolicy BomWhsReleasePolicy { get; set; }
        public ProdFlushingPrincip ProdFlushingPrincip { get; set; }

        // ==========================================================
        // Process Manufacturing Extensions (Formula / Batch Tracking)
        // ==========================================================
        // Basic Properties
        [Required]
        [StringLength(FieldLengths.BatchNumGroupId)]
        public string BatchNumGroupId { get; set; } = string.Empty;

        [Required]
        [StringLength(FieldLengths.SerialNumGroupId)]
        public string SerialNumGroupId { get; set; } = string.Empty;

        [Required]
        [StringLength(FieldLengths.PmfPlanningItemId)]
        public string PmfPlanningItemId { get; set; } = string.Empty;

        public decimal PmfYieldPct { get; set; }

        // Enum Properties
        public PmfProductType PmfProductType { get; set; } // Co-Product, By-Product, None
        public BatchMergedDateCalculationMethod BatchMergedDateCalculationMethod { get; set; }

        // ==========================================================
        // Process Manufacturing - Catch Weight (PDS) & Shelf Life
        // ==========================================================
        // Basic Properties
        [Required]
        [StringLength(FieldLengths.PdsBaseAttributeID)]
        public string PdsBaseAttributeID { get; set; } = string.Empty;

        public int PdsShelfLife { get; set; }
        public int PdsShelfAdvice { get; set; }
        public int PdsBestBefore { get; set; }
        public decimal PdsTargetFactor { get; set; }

        // Enum Properties
        public NoYes PdsVendorCheckItem { get; set; }
        public PdsPotencyAttribRecording PdsPotencyAttribRecording { get; set; }

        // ==========================================================
        // Warehouse Management (WMS) & Structural Constraints
        // ==========================================================
        // Basic Properties
        [Required]
        [StringLength(FieldLengths.WmsPalletTypeIdId)]
        public string WmsPalletTypeIdId { get; set; } = string.Empty;

        public int WmsArrivalHandlingTime { get; set; }
        public int WmsPickingQtyTime { get; set; }
        public decimal QtyPerLayer { get; set; }
        public decimal StandardPalletQuantity { get; set; }
        public decimal MinimumPalletQuantity { get; set; }

        // Catch Weight (PDS) Specific WMS Properties
        public decimal PdscwWmsQtyPerLayer { get; set; }
        public decimal PdscwWmsStandardPalletQty { get; set; }
        public decimal PdscwWmsMinimumPalletQty { get; set; }

        // ==========================================================
        // Master Planning & Replenishment Directives
        // ==========================================================
        // Basic Properties
        [Required]
        [StringLength(FieldLengths.ReqGroupId)]
        public string ReqGroupId { get; set; } = string.Empty;

        // Enum Properties
        public NoYes ForecastDmpInclude { get; set; }

        // ==========================================================
        // Standard Configuration Variant Fallbacks
        // ==========================================================
        // Basic Properties
        [Required]
        [StringLength(FieldLengths.StandardConfigId)]
        public string StandardConfigId { get; set; } = string.Empty;

        [Required]
        [StringLength(FieldLengths.StandardInventSizeId)]
        public string StandardInventSizeId { get; set; } = string.Empty;

        [Required]
        [StringLength(FieldLengths.StandardInventColorId)]
        public string StandardInventColorId { get; set; } = string.Empty;

        [Required]
        [StringLength(FieldLengths.StandardInventStyleId)]
        public string StandardInventStyleId { get; set; } = string.Empty;

        // ==========================================================
        // Quality Management System (QMS) Parameters
        // ==========================================================
        // Basic Properties
        public decimal QmsOverDispensePct { get; set; }
        public decimal QmsUnderDispensePct { get; set; }

        // Enum Properties
        public NoYes QmsCustomerCheckItem { get; set; }
        public QmsDispensingControl QmsDispensingControl { get; set; }
        public QmsAuthorizedPersonnel QmsAuthorizedPersonnel { get; set; }

        // ==========================================================
        // Advanced Revenue Recognition Controls (RevRec)
        // ==========================================================
        // Basic Properties
        [Required]
        [StringLength(FieldLengths.RevRecDefaultRevenueRecognitionSchedule)]
        public string RevRecDefaultRevenueRecognitionSchedule { get; set; } = string.Empty;

        public decimal RevRecMedianPriceMinimumTolerance { get; set; }
        public decimal RevRecMedianPriceMaximumTolerance { get; set; }

        // Enum Properties
        public NoYes RevRecRevenueRecognitionEnabled { get; set; }
        public RevRecRevenueType RevRecRevenueType { get; set; }
        public NoYes RevRecBundle { get; set; }
        public NoYes RevRecMedianPrice { get; set; }
        public NoYes RevRecExcludeFromCarveOut { get; set; }

        // ==========================================================
        // Landed Cost Framework (ITM) Attributes
        // ==========================================================
        // Basic Properties
        [Required]
        [StringLength(FieldLengths.ItmOverUnderToleranceGroupId)]
        public string ItmOverUnderToleranceGroupId { get; set; } = string.Empty;

        [Required]
        [StringLength(FieldLengths.ItmCostTypeGroupId)]
        public string ItmCostTypeGroupId { get; set; } = string.Empty;

        [Required]
        [StringLength(FieldLengths.ItmCostTransferGroupId)]
        public string ItmCostTransferGroupId { get; set; } = string.Empty;

        [Required]
        [StringLength(FieldLengths.ItmArrivalGroupId)]
        public string ItmArrivalGroupId { get; set; } = string.Empty;

        // ==========================================================
        // Intrastat, Customs & Regulatory Compliance
        // ==========================================================
        // Basic Properties
        public long IntrastatCommodity { get; set; }
        public decimal IntrastatChargePerKg { get; set; }
        public decimal TaxPackagingQty { get; set; }
        public long TaxRateType { get; set; }

        // Enum Properties
        public NoYes IntrastatExclude { get; set; }
        public NoYes CooDualUseProduct { get; set; } // Strategic/Military export monitoring indicator
        public NoYes IsExclusiveHbmc { get; set; }
        public int HmimIndicator { get; set; } // Hazardous Material Index Indicator

        // ==========================================================
        // External Ledger Dimensions & Sales CRM Modules
        // ==========================================================
        // Basic Properties
        [Required]
        [StringLength(FieldLengths.ProjCategoryId)]
        public string ProjCategoryId { get; set; } = string.Empty;

        [Required]
        [StringLength(FieldLengths.CommissionGroupId)]
        public string CommissionGroupId { get; set; } = string.Empty;

        public long DefaultDimension { get; set; }


        #region Navigation Properties Row

//         [ForeignKey(nameof(DefaultDimension))]
//         public virtual DimensionAttributeValueSet? FinancialDimensionSet { get; set; }

        //[ForeignKey(nameof(Product))]
        //public virtual EcoResProduct? EcoResProduct { get; set; }

        #endregion
    }
}




