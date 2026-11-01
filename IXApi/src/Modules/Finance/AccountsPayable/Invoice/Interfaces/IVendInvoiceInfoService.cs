using IAX.IXApi.Infrastructure.Persistence.Services;

namespace IAX.IXApi.Modules.Finance.AccountsPayable;

public interface IVendInvoiceInfoService : IBaseService<VendInvoiceInfoTable>
{
    /// <summary>Enters a pending vendor invoice against a purchase order and holds it.</summary>
    Task<PurchResult> CreateFromOrderAsync(long purchTableRecId, VendInvoiceInfoCreateDto input, CancellationToken ct);

    /// <summary>Reads a held pending invoice with its lines.</summary>
    Task<PurchResult> GetByParmIdAsync(string parmId, CancellationToken ct);

    /// <summary>Discards a held pending invoice without posting it.</summary>
    Task<PurchResult> DiscardAsync(string parmId, CancellationToken ct);

    /// <summary>Posts a held pending invoice through the vendor invoice posting path.</summary>
    Task<PurchResult> PostAsync(string parmId, CancellationToken ct);
}
