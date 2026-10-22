using IAX.IXApi.Modules.Finance.Common;

namespace IAX.IXApi.Modules.Finance.Inventory;

public sealed class InventJournalTableDto
{
    public long RecId { get; set; }
    public string JournalId { get; set; } = string.Empty;
    public string JournalNameId { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public InventJournalType JournalType { get; set; }
    public int NumOfLines { get; set; }
    public bool Posted { get; set; }
    public DateTime? PostedDateTime { get; set; }
    public string InventSiteId { get; set; } = string.Empty;
    public string InventLocationId { get; set; } = string.Empty;
    public List<InventJournalTransDto> Lines { get; set; } = [];
}

public sealed class InventJournalCreateDto
{
    public string JournalNameId { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string InventSiteId { get; set; } = string.Empty;
    public string InventLocationId { get; set; } = string.Empty;
}

public sealed class InventJournalTransDto
{
    public long RecId { get; set; }
    public decimal LineNum { get; set; }
    public DateTime TransDate { get; set; }
    public string ItemId { get; set; } = string.Empty;
    public string InventDimId { get; set; } = string.Empty;
    public string ToInventDimId { get; set; } = string.Empty;
    public string InventSiteId { get; set; } = string.Empty;
    public string InventLocationId { get; set; } = string.Empty;
    public string InventBatchId { get; set; } = string.Empty;
    public string InventSerialId { get; set; } = string.Empty;
    public string InventStatusId { get; set; } = string.Empty;
    public string WmsLocationId { get; set; } = string.Empty;
    public string ToWmsLocationId { get; set; } = string.Empty;
    public string ToInventSiteId { get; set; } = string.Empty;
    public string ToInventLocationId { get; set; } = string.Empty;
    public decimal Qty { get; set; }
    public decimal InventOnHand { get; set; }
    public decimal Counted { get; set; }
    public string Unit { get; set; } = string.Empty;
    public decimal CostPrice { get; set; }
    public decimal CostAmount { get; set; }
    public long LedgerDimension { get; set; }
}

public sealed class InventJournalLineInputDto
{
    public DateTime TransDate { get; set; }
    public string ItemId { get; set; } = string.Empty;
    public string InventSiteId { get; set; } = string.Empty;
    public string InventLocationId { get; set; } = string.Empty;
    public string InventBatchId { get; set; } = string.Empty;
    public string InventSerialId { get; set; } = string.Empty;
    public string InventStatusId { get; set; } = string.Empty;
    public string WmsLocationId { get; set; } = string.Empty;
    public string ToInventSiteId { get; set; } = string.Empty;
    public string ToInventLocationId { get; set; } = string.Empty;
    public string ToWmsLocationId { get; set; } = string.Empty;
    public decimal Qty { get; set; }
    public decimal Counted { get; set; }
    public string Unit { get; set; } = string.Empty;
    public decimal CostPrice { get; set; }
    public long LedgerDimension { get; set; }
}

public sealed record InventJournalLookupDto(string Code, string Description);

public sealed class InventJournalLookupsDto
{
    public List<InventJournalLookupDto> JournalNames { get; set; } = [];
    public List<InventJournalLookupDto> Items { get; set; } = [];
    public List<InventJournalLookupDto> Sites { get; set; } = [];
    public List<InventJournalLookupDto> Warehouses { get; set; } = [];
    public List<InventJournalLookupDto> Units { get; set; } = [];
}

public sealed class InventJournalPostResultDto
{
    public string JournalId { get; set; } = string.Empty;
    public bool Posted { get; set; }
    public DateTime PostedDateTime { get; set; }
    public int InventoryTransactionsCreated { get; set; }
    public int LedgerEntriesCreated { get; set; }
    public List<string> Vouchers { get; set; } = [];
}

