using IAX.IXApi.Api.Controllers;
using IAX.IXApi.Modules.Identity.Permissions;
using IAX.IXApi.Shared.Application.Contracts;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace IAX.IXApi.Modules.Finance.AccountsPayable;

[ApiController]
[Route("api/v1/[controller]")]
[DomainPermission("AccountsPayable", "PurchaseOrders", "View")]
public class PurchConfirmationController : BaseController<VendPurchOrderJour, VendPurchOrderJourDto>
{
    private readonly IPurchConfirmationService _confirmationService;

    public PurchConfirmationController(
        IPurchConfirmationService service,
        ILogger<PurchConfirmationController> logger)
        : base(service, logger)
    {
        _confirmationService = service;
    }

    [HttpPost("{recId:long}")]
    [DomainPermission("AccountsPayable", "PurchaseOrders", "Confirm")]
    public async Task<IActionResult> Confirm(long recId, CancellationToken ct) =>
        Present(await _confirmationService.ConfirmAsync(recId, ct));

    [NonAction]
    public override Task<ActionResult<APIResponse<VendPurchOrderJourDto>>> Create(
        VendPurchOrderJourDto dto,
        CancellationToken cancellationToken = default) =>
        base.Create(dto, cancellationToken);

    [NonAction]
    public override Task<ActionResult<APIResponse<VendPurchOrderJourDto>>> Update(
        string id,
        VendPurchOrderJourDto dto,
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
        _ => StatusCode(result.StatusCode, APIResponse<object>.Fail(result.Message ?? "Purchase confirmation failed."))
    };
}
