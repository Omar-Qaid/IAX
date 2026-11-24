using IAX.IXApi.Modules.Finance.Common;
using IAX.IXApi.Modules.Identity.Permissions;
using IAX.IXApi.Shared.Application.Contracts;
using Microsoft.AspNetCore.Mvc;

namespace IAX.IXApi.Modules.Finance.AccountsPayable.InvoiceRegister;

/// <summary>D365 invoice register (PurchaseLedger) and invoice approval (Approval) journals.</summary>
[ApiController]
[Route("api/v1/VendInvoiceRegister")]
public sealed class VendInvoiceRegisterController(IVendInvoiceRegisterService service) : ControllerBase
{
    [HttpPost("register")]
    [DomainPermission("AccountsPayable", "InvoiceRegister", "Post")]
    public Task<IActionResult> Register([FromBody] VendInvoiceRegisterRequest request, CancellationToken ct) =>
        ExecuteAsync(() => service.RegisterAsync(request, ct));

    [HttpPost("approve")]
    [DomainPermission("AccountsPayable", "InvoiceRegister", "Post")]
    public Task<IActionResult> Approve([FromBody] VendInvoiceApprovalRequest request, CancellationToken ct) =>
        ExecuteAsync(() => service.ApproveAsync(request, ct));

    private async Task<IActionResult> ExecuteAsync(Func<Task<string>> post)
    {
        try
        {
            return Ok(APIResponse<string>.Ok(await post()));
        }
        catch (SubledgerPostingException ex)
        {
            return UnprocessableEntity(APIResponse<string>.Fail(ex.Message));
        }
    }
}
