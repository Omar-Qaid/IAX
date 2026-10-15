namespace IAX.IXApi.Modules.Finance.Inventory;

public sealed class InventModelGroupRecordDto
{
    public string Id { get; init; } = string.Empty;
    public string ModelGroupId { get; init; } = string.Empty;
    public string Name { get; init; } = string.Empty;
    public int InventModel { get; init; }
    public bool StockedProduct { get; init; }
    public bool PostOnHandPhysical { get; init; }
    public bool PostOnHandFinancial { get; init; }
    public bool PostPhysicalRevenue { get; init; }
    public bool PostPhysicalPurchase { get; init; }
    public bool StandardCost { get; init; }
    public bool MandatoryRegister { get; init; }
    public bool MandatoryReceive { get; init; }
    public bool MandatoryPick { get; init; }
    public bool MandatoryDeduct { get; init; }
    public bool MandatoryWmsOrder { get; init; }
    public bool NegativePhysical { get; init; }
    public bool NegativeFinancial { get; init; }
    public bool QuarantineControl { get; init; }
    public bool InclPhysicalValueInCost { get; init; }
    public bool IncludePhysicalValueForAverage { get; init; }
    public bool ItemProdReservationActive { get; init; }
    public int ItemProdReservation { get; init; }
    public bool ReserveByDate { get; init; }
    public bool ReserveReversed { get; init; }
    public bool McrReservation { get; init; }
    public string PdsDispositionCode { get; init; } = string.Empty;
    public bool PdsCheck { get; init; }
    public bool PdsConsReq { get; init; }
    public bool PdsReqVendBatchDetail { get; init; }
    public bool PdsSameLot { get; init; }
    public bool PdsVendorCheckItem { get; init; }
    public int PdsPickCriteria { get; init; }
    public int PickingListBatchExpirationDateValidationRule { get; init; }
}
