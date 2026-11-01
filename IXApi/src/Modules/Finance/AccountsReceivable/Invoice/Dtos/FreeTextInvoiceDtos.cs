namespace IAX.IXApi.Modules.Finance.AccountsReceivable.Invoice;

public sealed class FreeTextInvoiceSaveDto
{
    public string CustomerAccount { get; set; } = string.Empty;
    public string? InvoiceAccount { get; set; }
    public DateTime? InvoiceDate { get; set; }
    public string? CurrencyCode { get; set; }
    public string? PaymentTerms { get; set; }
    public string? PaymentMethod { get; set; }
    public string? PostingProfile { get; set; }
    public long? DefaultDimension { get; set; }
    public string? TaxGroup { get; set; }
    public string? Description { get; set; }
    public List<FreeTextInvoiceLineInputDto> Lines { get; set; } = [];
}

public sealed class FreeTextInvoiceLineInputDto
{
    public string Description { get; set; } = string.Empty;
    public decimal Quantity { get; set; }
    public decimal UnitPrice { get; set; }
    public long LedgerDimension { get; set; }
    public long? DefaultDimension { get; set; }
    public string? TaxGroup { get; set; }
    public string? TaxItemGroup { get; set; }
}

public sealed record FreeTextInvoiceLineDto(
    long RecId,
    decimal LineNum,
    string Description,
    decimal Quantity,
    decimal UnitPrice,
    decimal NetAmount,
    decimal TaxAmount,
    decimal GrossAmount,
    long LedgerDimension,
    long DefaultDimension,
    string TaxGroup,
    string TaxItemGroup);

public sealed record FreeTextInvoiceDto(
    long RecId,
    string InvoiceId,
    string CustomerAccount,
    string InvoiceAccount,
    string Description,
    DateTime InvoiceDate,
    DateTime DueDate,
    string CurrencyCode,
    string PaymentTerms,
    string PaymentMethod,
    string PostingProfile,
    long DefaultDimension,
    string TaxGroup,
    bool Posted,
    decimal NetAmount,
    decimal TaxAmount,
    decimal InvoiceAmount,
    IReadOnlyList<FreeTextInvoiceLineDto> Lines);

public sealed record FreeTextInvoiceResult(int StatusCode, FreeTextInvoiceDto? Invoice, string? Message = null);
