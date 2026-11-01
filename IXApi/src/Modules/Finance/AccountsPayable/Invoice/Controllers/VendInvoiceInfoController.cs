using IAX.IXApi.Api.Controllers;
using IAX.IXApi.Modules.Identity.Permissions;
using IAX.IXApi.Shared.Application.Contracts;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace IAX.IXApi.Modules.Finance.AccountsPayable;

[ApiController]
[Route("api/v1/[controller]")]
[DomainPermission("AccountsPayable", "PurchaseOrders", "View")]
public class VendInvoiceInfoController : BaseController<VendInvoiceInfoTable, VendInvoiceInfoTableDto>
{
    private readonly IVendInvoiceInfoService _pending;

    public VendInvoiceInfoController(
        IVendInvoiceInfoService service,
        ILogger<VendInvoiceInfoController> logger)
        : base(service, logger)
    {
        _pending = service;
    }

    /// <summary>Enters and holds a pending vendor invoice against a purchase order.</summary>
    [HttpPost("order/{recId:long}")]
    [DomainPermission("AccountsPayable", "PurchaseOrders", "Post")]
    public async Task<IActionResult> CreateFromOrder(long recId, [FromBody] VendInvoiceInfoCreateDto input, CancellationToken ct) =>
        Present(await _pending.CreateFromOrderAsync(recId, input, ct));

    [HttpGet("{parmId}")]
    public async Task<IActionResult> GetByParmId(string parmId, CancellationToken ct) =>
        Present(await _pending.GetByParmIdAsync(parmId, ct));

    /// <summary>Discards a held pending vendor invoice without posting it.</summary>
    [HttpDelete("{parmId}")]
    [DomainPermission("AccountsPayable", "PurchaseOrders", "Post")]
    public async Task<IActionResult> Discard(string parmId, CancellationToken ct) =>
        Present(await _pending.DiscardAsync(parmId, ct));

    /// <summary>Posts a held pending vendor invoice.</summary>
    [HttpPost("{parmId}/post")]
    [DomainPermission("AccountsPayable", "PurchaseOrders", "Post")]
    public async Task<IActionResult> Post(string parmId, CancellationToken ct) =>
        Present(await _pending.PostAsync(parmId, ct));

    [NonAction]
    public override Task<ActionResult<APIResponse<VendInvoiceInfoTableDto>>> Create(
        VendInvoiceInfoTableDto dto,
        CancellationToken cancellationToken = default) =>
        base.Create(dto, cancellationToken);

    [NonAction]
    public override Task<ActionResult<APIResponse<VendInvoiceInfoTableDto>>> Update(
        string id,
        VendInvoiceInfoTableDto dto,
        CancellationToken cancellationToken = default) =>
        base.Update(id, dto, cancellationToken);

    [NonAction]
    public override Task<ActionResult<APIResponse<bool>>> Delete(
        string id,
        CancellationToken cancellationToken = default) =>
        base.Delete(id, cancellationToken);

    private IActionResult Present(PurchResult result) => result.StatusCode switch
    {
        200 => Ok(APIResponse<object>.Ok(result.Data!, result.Message ?? string.Empty)),
        400 => BadRequest(APIResponse<object>.Fail(result.Message!)),
        404 => NotFound(APIResponse<object>.Fail(result.Message!)),
        422 => UnprocessableEntity(APIResponse<object>.Fail(result.Message!)),
        _ => StatusCode(result.StatusCode, APIResponse<object>.Fail(result.Message ?? "Pending vendor invoice failed."))
    };
}
