using Mapster;

namespace IAX.IXApi.Modules.Finance.Inventory;

public sealed class InventTransMapping : IRegister
{
    public void Register(TypeAdapterConfig config)
    {
        config.NewConfig<InventTransMappingSource, InventTransListDto>()
            .Map(d => d.RecId, s => s.Transaction.RecId)
            .Map(d => d.ItemNumber, s => s.Transaction.ItemId)
            .Map(d => d.PhysicalDate, s => AsPostedDate(s.Transaction.DatePhysical))
            .Map(d => d.FinancialDate, s => AsPostedDate(s.Transaction.DateFinancial))
            .Map(d => d.Reference, s => ReferenceLabel((s.Origin == null ? null : s.Origin.ReferenceCategory)))
            .Map(d => d.ReferenceNumber, s => (s.Origin == null ? string.Empty : s.Origin.ReferenceId ?? string.Empty))
            .Map(d => d.InventTransId, s => (s.Origin == null ? string.Empty : s.Origin.InventTransId ?? string.Empty))
            .Map(d => d.ReceiptStatus, s => StatusLabel(s.Transaction.StatusReceipt))
            .Map(d => d.IssueStatus, s => StatusLabel(s.Transaction.StatusIssue))
            .Map(d => d.Quantity, s => s.Transaction.Qty)
            .Map(d => d.UnitPrice, s => s.UnitPrice)
            .Map(d => d.UnitCost, s => s.Transaction.Qty == 0 ? 0 : Math.Abs(s.CostAmount / s.Transaction.Qty))
            .Map(d => d.CostAmount, s => s.CostAmount)
            .Map(d => d.CurrencyCode, s => s.Transaction.CurrencyCode)
            .Map(d => d.Site, s => (s.Dimension == null ? string.Empty : s.Dimension.InventSiteId ?? string.Empty))
            .Map(d => d.Warehouse, s => (s.Dimension == null ? string.Empty : s.Dimension.InventLocationId ?? string.Empty))
            .Map(d => d.BatchNumber, s => (s.Dimension == null ? string.Empty : s.Dimension.InventBatchId ?? string.Empty))
            .Map(d => d.SerialNumber, s => (s.Dimension == null ? string.Empty : s.Dimension.InventSerialId ?? string.Empty))
            .Map(d => d.ExpectedDate, s => AsPostedDate(s.Transaction.DateExpected))
            .Map(d => d.Voucher, s => s.Transaction.Voucher)
            .Map(d => d.DataAreaId, s => s.Transaction.DataAreaId);
    }

    private static DateTime? AsPostedDate(DateTime value) => value == default ? null : value;

    private static string StatusLabel(Common.StatusIssue status) => status switch
    {
        Common.StatusIssue.None => string.Empty,
        Common.StatusIssue.Ordered => "On order",
        _ => status.ToString()
    };

    private static string StatusLabel(Common.StatusReceipt status) => status switch
    {
        Common.StatusReceipt.None => string.Empty,
        Common.StatusReceipt.Ordered => "Ordered",
        _ => status.ToString()
    };

    private static string ReferenceLabel(Common.InventRefType? reference) => reference switch
    {
        Common.InventRefType.SalesTable => "Sales order",
        Common.InventRefType.PurchaseOrder => "Purchase order",
        Common.InventRefType.TransferOrder => "Transfer order",
        Common.InventRefType.ProductionOrder => "Production order",
        Common.InventRefType.InventoryJournal => "Inventory journal",
        Common.InventRefType.InventoryAdjustment => "Inventory adjustment",
        Common.InventRefType.PhysicalinventoryCount => "Physical inventory count",
        Common.InventRefType.ReturnOrder => "Return order",
        _ => string.Empty
    };
}
