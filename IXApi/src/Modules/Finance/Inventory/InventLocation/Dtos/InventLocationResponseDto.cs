namespace IAX.IXApi.Modules.Finance.Inventory;

public sealed class InventLocationResponseDto
{
    public string Id { get; set; } = string.Empty;
    public long RecId { get; set; }
    public string InventLocationId { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string InventSiteId { get; set; } = string.Empty;
    public int InventLocationType { get; set; }
    public int InventLocationLevel { get; set; }
    public string InventLocationIdTransit { get; set; } = string.Empty;
    public string InventLocationIdQuarantine { get; set; } = string.Empty;
    public string InventLocationIdReqMain { get; set; } = string.Empty;
    public string ItmInventLocationIdGit { get; set; } = string.Empty;
    public string ItmInventLocationIdUnder { get; set; } = string.Empty;
    public string VendAccount { get; set; } = string.Empty;
    public bool WorkflowApproval { get; set; }
    public bool Manual { get; set; }
    public bool ReqRefill { get; set; }
    public string WmsLocationIdDefaultReceipt { get; set; } = string.Empty;
    public string WmsLocationIdDefaultIssue { get; set; } = string.Empty;
    public string DefaultProductionInputLocation { get; set; } = string.Empty;
    public string DefaultProductionFinishGoodsLocation { get; set; } = string.Empty;
    public string DefaultKanbanFinishedGoodsLocation { get; set; } = string.Empty;
    public string DefaultReturnCreditOnlyLocation { get; set; } = string.Empty;
    public string DefaultStatusId { get; set; } = string.Empty;
    public string WmsRackFormat { get; set; } = string.Empty;
    public string WmsLevelFormat { get; set; } = string.Empty;
    public string WmsPositionFormat { get; set; } = string.Empty;
    public bool WhsEnabled { get; set; }
    public bool WarehouseAutoReleaseReservation { get; set; }
    public bool AutoUpdateShipment { get; set; }
    public bool ReserveAtLoadPost { get; set; }
    public bool DecrementLoadLine { get; set; }
    public bool PrintBolBeforeShipConfirm { get; set; }
    public bool CycleCountAllowPalletMove { get; set; }
    public bool AllowLaborStandards { get; set; }
    public bool AllowMarkingReservationRemoval { get; set; }
    public bool UseWmsOrders { get; set; }
    public bool WmsAisleNameActive { get; set; }
    public bool WmsRackNameActive { get; set; }
    public bool WmsLevelNameActive { get; set; }
    public bool WmsPositionNameActive { get; set; }
    public bool UniqueCheckDigits { get; set; }
    public bool EnableQualityManagement { get; set; }
    public bool RemoveInventBlockingOnStatusChange { get; set; }
    public bool ProdReserveOnlyWhse { get; set; }
    public bool WhsProdOrderBackflushMustUseReservedQty { get; set; }
    public bool FshStore { get; set; }
    public bool ConsolidateShipAtRtw { get; set; }
    public bool RetailInventNegPhysical { get; set; }
    public bool RetailInventNegFinancial { get; set; }
    public bool EnableExternalWarehouse { get; set; }
    public int MaxPickingRouteTime { get; set; }
    public int PickingLineTime { get; set; }
}
