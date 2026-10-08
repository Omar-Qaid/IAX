namespace IAX.IXApi.Modules.Finance.AccountsReceivable.Confirm;

public sealed record PostConfirmationRequest(DateTime? ConfirmationDate = null, bool Posting = true);
public sealed record ConfirmationResult(int StatusCode, object? Data, string? Message = null);

public sealed class ConfirmationListItemDto
{
    public string Id { get; set; } = string.Empty;
    public string ConfirmId { get; set; } = string.Empty;
    public DateTime ConfirmDate { get; set; }
    public string CurrencyCode { get; set; } = string.Empty;
    public decimal Qty { get; set; }
    public decimal SalesBalance { get; set; }
    public decimal SumMarkup { get; set; }
    public decimal SumTax { get; set; }
    public decimal ConfirmAmount { get; set; }
}

public sealed class ConfirmationHeaderDto
{
    public string Id { get; set; } = string.Empty;
    public string ConfirmId { get; set; } = string.Empty;
    public DateTime ConfirmDate { get; set; }
    public string SalesId { get; set; } = string.Empty;
    public string OrderAccount { get; set; } = string.Empty;
    public string InvoiceAccount { get; set; } = string.Empty;
    public string CurrencyCode { get; set; } = string.Empty;
    public string Payment { get; set; } = string.Empty;
    public string DeliveryName { get; set; } = string.Empty;
    public string DeliveryPostalAddress { get; set; } = string.Empty;
    public string DlvMode { get; set; } = string.Empty;
    public string DlvTerm { get; set; } = string.Empty;
    public decimal Qty { get; set; }
    public decimal SalesBalance { get; set; }
    public decimal SumLineDisc { get; set; }
    public decimal EndDisc { get; set; }
    public decimal SumMarkup { get; set; }
    public decimal SumTax { get; set; }
    public decimal ConfirmAmount { get; set; }
}

public sealed class ConfirmationLineDto
{
    public string Id { get; set; } = string.Empty;
    public string ConfirmId { get; set; } = string.Empty;
    public string SalesId { get; set; } = string.Empty;
    public decimal LineNum { get; set; }
    public string ItemId { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public decimal Qty { get; set; }
    public decimal InventQty { get; set; }
    public string SalesUnit { get; set; } = string.Empty;
    public string InventDimId { get; set; } = string.Empty;
    public string InventTransId { get; set; } = string.Empty;
    public decimal SalesPrice { get; set; }
    public decimal PriceUnit { get; set; }
    public decimal LineDisc { get; set; }
    public decimal MultiLnDisc { get; set; }
    public decimal DiscAmount { get; set; }
    public decimal LineAmount { get; set; }
    public decimal TaxAmount { get; set; }
    public decimal SalesMarkup { get; set; }
    public string CurrencyCode { get; set; } = string.Empty;
}
