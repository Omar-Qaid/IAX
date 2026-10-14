namespace IAX.IXApi.Modules.Finance.AccountsReceivable.PackingSlip.Dtos;

public sealed record PackingSlipJournalResult(int StatusCode, object? Data, string? Message = null);

public sealed record PackingSlipPostingLineRequest(string SalesLineId, decimal Quantity);

public sealed record PostPackingSlipRequest(
    DateTime? DeliveryDate,
    string QuantityMode,
    bool Posting,
    string RequestId,
    IReadOnlyList<PackingSlipPostingLineRequest> Lines);

public sealed class PackingSlipPostingPreviewDto
{
    public PackingSlipPostingHeaderDto Header { get; set; } = new();
    public List<PackingSlipPostingLineDto> Lines { get; set; } = [];
}

public sealed class PackingSlipPostingHeaderDto
{
    public string Id { get; set; } = string.Empty;
    public string SalesId { get; set; } = string.Empty;
    public string CustomerName { get; set; } = string.Empty;
    public string CurrencyCode { get; set; } = string.Empty;
    public string SiteId { get; set; } = string.Empty;
}

public sealed class PackingSlipPostingLineDto
{
    public string Id { get; set; } = string.Empty;
    public decimal LineNumber { get; set; }
    public string ItemId { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string SalesCategory { get; set; } = string.Empty;
    public string ProductNumber { get; set; } = string.Empty;
    public string InventSiteId { get; set; } = string.Empty;
    public string InventLocationId { get; set; } = string.Empty;
    public string WmsLocationId { get; set; } = string.Empty;
    public decimal CatchWeightQuantity { get; set; }
    public decimal Quantity { get; set; }
    public decimal NetAmount { get; set; }
}

public sealed class PackingSlipJournalSummaryDto
{
    public string Id { get; set; } = string.Empty;
    public string SalesId { get; set; } = string.Empty;
    public string PackingSlipId { get; set; } = string.Empty;
    public string Version { get; set; } = string.Empty;
    public DateTime DeliveryDate { get; set; }
    public DateTime InvoiceDueDate { get; set; }
    public string Terms { get; set; } = string.Empty;
    public string ModeOfDelivery { get; set; } = string.Empty;
    public bool BlindShipment { get; set; }
    public string IntercompanyCompanyId { get; set; } = string.Empty;
    public bool IntercompanyPosted { get; set; }
    public bool Printed { get; set; }
    public string OwnerId { get; set; } = string.Empty;
    public string ContractorId { get; set; } = string.Empty;
}

public sealed class PackingSlipJournalLineDto
{
    public string Id { get; set; } = string.Empty;
    public string PackingSlipId { get; set; } = string.Empty;
    public string SalesId { get; set; } = string.Empty;
    public string OriginalSalesId { get; set; } = string.Empty;
    public decimal LineNumber { get; set; }
    public string ItemId { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string SalesUnit { get; set; } = string.Empty;
    public decimal Quantity { get; set; }
    public decimal InventQuantity { get; set; }
    public decimal CatchWeightQuantity { get; set; }
    public string SiteId { get; set; } = string.Empty;
    public string WarehouseId { get; set; } = string.Empty;
    public string BatchNumber { get; set; } = string.Empty;
    public string SerialNumber { get; set; } = string.Empty;
    public string InventoryTransactionId { get; set; } = string.Empty;
    public DateTime RequestedShipDate { get; set; }
    public DateTime ConfirmedShipDate { get; set; }
    public int DeliveryType { get; set; }
    public decimal Amount { get; set; }
    public decimal AccountingAmount { get; set; }
    public bool FullyMatched { get; set; }
    public bool Scrap { get; set; }
}
