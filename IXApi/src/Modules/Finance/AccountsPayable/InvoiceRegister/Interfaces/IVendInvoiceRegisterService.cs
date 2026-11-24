namespace IAX.IXApi.Modules.Finance.AccountsPayable.InvoiceRegister;

/// <summary>D365 invoice register and invoice approval journals. Both return the invoice's voucher.</summary>
public interface IVendInvoiceRegisterService
{
    Task<string> RegisterAsync(VendInvoiceRegisterRequest request, CancellationToken ct);
    Task<string> ApproveAsync(VendInvoiceApprovalRequest request, CancellationToken ct);
}
