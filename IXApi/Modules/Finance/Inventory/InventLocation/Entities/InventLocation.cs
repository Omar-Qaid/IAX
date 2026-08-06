using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using IAX.IXApi.Modules.Finance.Shared.Features;
using IAX.IXApi.Modules.Finance.Common;

namespace IAX.IXApi.Modules.Finance.Entities
{
    [Table("InventLocation")]
    public class InventLocation : Entity<long>
    {
        //----------------------------------------- Core Identity & Site Topography
        // Basic Properties
        [Required]
        [StringLength(FieldLengths.InventLocationId)]
        public string InventLocationId { get; set; } = string.Empty;

        [Required]
        [StringLength(FieldLengths.Name)]
        public string Name { get; set; } = string.Empty;

        [Required]
        [StringLength(FieldLengths.InventSiteId)]
        public string InventSiteId { get; set; } = string.Empty; // Relational owner anchor linking to the parent physical InventSite

        // Enum Properties
        public InventLocationType InventLocationType { get; set; } // 0: Standard, 1: Quarantine, 2: Transit, 3: Vendor
        public int InventLocationLevel { get; set; }

        // ==========================================================
        // Sub-Warehouse Topology & Inter-Facility Links
        // ==========================================================
        // Basic Properties
        [Required]
        [StringLength(FieldLengths.InventLocationIdTransit)]
        public string InventLocationIdTransit { get; set; } = string.Empty; // Scopes transfer-order scrap/bridging legs

        [Required]
        [StringLength(FieldLengths.InventLocationIdQuarantine)]
        public string InventLocationIdQuarantine { get; set; } = string.Empty; // Quality isolation routing fallback target

        [Required]
        [StringLength(FieldLengths.InventLocationIdReqMain)]
        public string InventLocationIdReqMain { get; set; } = string.Empty; // Alternative replenishment source map anchor

        [Required]
        [StringLength(FieldLengths.ItmInventLocationIdGit)]
        public string ItmInventLocationIdGit { get; set; } = string.Empty; // Landed Cost Goods-in-Transit hub link

        [Required]
        [StringLength(FieldLengths.InventLocationId)]
        public string ItmInventLocationIdUnder { get; set; } = string.Empty; // Landed Cost under-delivery tracking container

        [Required]
        [StringLength(FieldLengths.VendAccount)]
        public string VendAccount { get; set; } = string.Empty; // Direct cross-link reference if managed as a vendor-consigned hub

        // ==========================================================
        // Advanced Warehouse Management Engine Configurations (WHS)
        // ==========================================================
        // Basic Properties
        public decimal MaxPickingRouteVolume { get; set; }
        public int MaxPickingRouteTime { get; set; }
        public int PickingLineTime { get; set; }

        // Enum Properties
        public NoYes WhsEnabled { get; set; } // Switches on advanced structural waving, work items, and mobile scanning engines
        public NoYes WarehouseAutoReleaseReservation { get; set; }
        public NoYes AutoUpdateShipment { get; set; }
        public NoYes ReserveAtLoadPost { get; set; }
        public NoYes DecrementLoadLine { get; set; }
        public NoYes PrintBolBeforeShipConfirm { get; set; } // Bill of Lading documentation hard block step toggle
        public NoYes CycleCountAllowPalletMove { get; set; }
        public NoYes AllowLaborStandards { get; set; }
        public NoYes AllowMarkingReservationRemoval { get; set; }
        public LoadReleaseReservationPolicy LoadReleaseReservationPolicy { get; set; }
        public ReleaseToWarehouseRule ReleaseToWarehouseRule { get; set; }
        public ReleaseRuleFailureOption ReleaseRuleFailureOption { get; set; }

        // ==========================================================
        // Classic WMS Location Naming Formatting & Structural Dimensions
        // ==========================================================
        // Basic Properties
        [Required]
        [StringLength(FieldLengths.WmsRackFormat)]
        public string WmsRackFormat { get; set; } = string.Empty;

        [Required]
        [StringLength(FieldLengths.WmsLevelFormat)]
        public string WmsLevelFormat { get; set; } = string.Empty;

        [Required]
        [StringLength(FieldLengths.WmsPositionFormat)]
        public string WmsPositionFormat { get; set; } = string.Empty;

        // Enum Properties
        public NoYes UseWmsOrders { get; set; } // Enables legacy inventory picking journals vs advanced WHS work items
        public NoYes WmsAisleNameActive { get; set; }
        public NoYes WmsRackNameActive { get; set; }
        public NoYes WmsLevelNameActive { get; set; }
        public NoYes WmsPositionNameActive { get; set; }
        public NoYes UniqueCheckDigits { get; set; }

        // ==========================================================
        // Default Fixed Storage & Bin Resolution Pointers
        // ==========================================================
        // Basic Properties
        [Required]
        [StringLength(FieldLengths.WmsLocationIdDefaultReceipt)]
        public string WmsLocationIdDefaultReceipt { get; set; } = string.Empty; // Target entry bin default

        [Required]
        [StringLength(FieldLengths.WmsLocationIdDefaultIssue)]
        public string WmsLocationIdDefaultIssue { get; set; } = string.Empty; // Source staging exit bin default

        [Required]
        [StringLength(FieldLengths.DefaultStatusID)]
        public string DefaultStatusID { get; set; } = string.Empty; // Default Inventory Status value (e.g., "Available", "Blocked")

        // Enum Properties
        public NoYes EnableQualityManagement { get; set; }
        public NoYes RemoveInventBlockingOnStatusChange { get; set; }

        // ==========================================================
        // Production & Logistics Execution Policies
        // ==========================================================
        // Basic Properties
        [Required]
        [StringLength(FieldLengths.DefaultProductionInputLocation)]
        public string DefaultProductionInputLocation { get; set; } = string.Empty;

        [Required]
        [StringLength(FieldLengths.DefaultProductionFinishGoodsLocation)]
        public string DefaultProductionFinishGoodsLocation { get; set; } = string.Empty;

        [Required]
        [StringLength(FieldLengths.DefaultKanbanFinishedGoodsLocation)]
        public string DefaultKanbanFinishedGoodsLocation { get; set; } = string.Empty;

        // Enum Properties
        public NoYes ProdReserveOnlyWhse { get; set; }
        public NoYes WhsProdOrderBackflushMustUseReservedQty { get; set; }
        public NoYes InventUseDefaultProductionLocationForFormulaBom { get; set; }
        public WhsRawMaterialPolicy WhsRawMaterialPolicy { get; set; }
        public RafPostingMethod RafPostingMethod { get; set; } // Report-As-Finished financial ledger flushing behavior

        // ==========================================================
        // Commerce Channel & Omni-Retail Operations (RBO)
        // ==========================================================
        // Basic Properties
        [Required]
        [StringLength(FieldLengths.RboDefaultWmsLocationID)]
        public string RboDefaultWmsLocationID { get; set; } = string.Empty;

        [Required]
        [StringLength(FieldLengths.RetailWmsLocationIDDefaultReturn)]
        public string RetailWmsLocationIDDefaultReturn { get; set; } = string.Empty;

        [Required]
        [StringLength(FieldLengths.DefaultReturnCreditOnlyLocation)]
        public string DefaultReturnCreditOnlyLocation { get; set; } = string.Empty;

        [Required]
        [StringLength(FieldLengths.RejectOrderFulfillment)]
        public string RejectOrderFulfillment { get; set; } = string.Empty;

        public decimal RetailWeightEx1 { get; set; }

        // Enum Properties
        public NoYes FshStore { get; set; } // Flags location as a retail store brick-and-mortar terminal
        public NoYes ConsolidateShipAtRtw { get; set; }
        public NoYes RetailInventNegPhysical { get; set; } // Permits point-of-sale to push balances below absolute zero
        public NoYes RetailInventNegFinancial { get; set; }

        // ==========================================================
        // System Master Governance Parameters
        // ==========================================================
        // Enum Properties
        public NoYes Manual { get; set; } // Prevents automated master planning engine from scheduling refills
        public NoYes ReqRefill { get; set; }
        public NoYes EnableExternalWarehouse { get; set; } // Third-party logistics (3PL) synchronization flag
        public NoYes WorkflowApproval { get; set; }


        #region Navigation Properties Row

//         [ForeignKey(nameof(InventSiteId))]
//         public virtual InventSite? AssociatedSite { get; set; }

//         [ForeignKey(nameof(InventLocationIdTransit))]
//         public virtual InventLocation? TransitLocation { get; set; }

//         [ForeignKey(nameof(InventLocationIdQuarantine))]
//         public virtual InventLocation? QuarantineLocation { get; set; }

//         [ForeignKey(nameof(InventLocationIdReqMain))]
//         public virtual InventLocation? ReqMainLocation { get; set; }

//         [ForeignKey(nameof(ItmInventLocationIdGit))]
//         public virtual InventLocation? GitLocation { get; set; }

//         [ForeignKey(nameof(ItmInventLocationIdUnder))]
//         public virtual InventLocation? UnderLocation { get; set; }

        #endregion
    }
}

