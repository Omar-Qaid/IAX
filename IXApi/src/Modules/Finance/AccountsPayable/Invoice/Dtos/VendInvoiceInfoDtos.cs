using IAX.IXApi.Shared.Application.Contracts;

namespace IAX.IXApi.Modules.Finance.AccountsPayable;

public class VendInvoiceInfoTableDto : EntityDto<long>
{
    public string ParmId { get; set; } = string.Empty;
    public string Num { get; set; } = string.Empty;
    public string InvoiceAccount { get; set; } = string.Empty;
    public string OrderAccount { get; set; } = string.Empty;
    public DateTime DocumentDate { get; set; }
    public DateTime TransDate { get; set; }
    public string CurrencyCode { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
}

/// <summary>
/// Entry of a pending vendor invoice against a purchase order. Leaving <paramref name="Lines"/>
/// empty takes D365's default quantity selection: everything received and not yet invoiced.
/// The line shape is the module's existing <see cref="PurchPostLineDto"/>.
/// </summary>
public sealed record VendInvoiceInfoCreateDto(
    string Num,
    DateTime? DocumentDate = null,
    DateTime? TransDate = null,
    string? Description = null,
    List<PurchPostLineDto>? Lines = null);

public sealed record VendInvoiceInfoLineReadDto(
    long RecId, string OrigPurchId, decimal LineNum, string ItemId, string InventTransId,
    string InventDimId, string PurchUnit, decimal ReceiveNow, decimal PurchPrice, decimal LineAmount);

public sealed record VendInvoiceInfoReadDto(
    long RecId, string ParmId, string Num, string InvoiceAccount, string OrderAccount,
    DateTime DocumentDate, DateTime TransDate, string CurrencyCode, string Description,
    decimal LineTotal, List<VendInvoiceInfoLineReadDto> Lines);
