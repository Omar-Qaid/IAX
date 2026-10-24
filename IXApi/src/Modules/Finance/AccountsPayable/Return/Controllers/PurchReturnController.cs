using IAX.IXApi.Modules.Identity.Permissions;
using IAX.IXApi.Shared.Application.Contracts;
using Microsoft.AspNetCore.Mvc;

namespace IAX.IXApi.Modules.Finance.AccountsPayable;

[ApiController]
[Route("api/v1/vendor-returns")]
[DomainPermission("AccountsPayable", "PurchaseOrders", "View")]
public sealed class PurchReturnController(IPurchReturnService service) : ControllerBase
{
    [HttpPost]
    [DomainPermission("AccountsPayable", "PurchaseOrders", "Create")]
    public async Task<IActionResult> Create([FromBody] PurchReturnCreateDto input, CancellationToken ct) =>
        Present(await service.CreateAsync(input, ct));

    [HttpPost("{recId:long}/shipments")]
    [DomainPermission("AccountsPayable", "PurchaseOrders", "Post")]
    public async Task<IActionResult> Ship(long recId, [FromBody] PurchReturnShipDto input, CancellationToken ct) =>
        Present(await service.ShipAsync(recId, input, ct));

    [HttpPost("{recId:long}/credit-notes")]
    [DomainPermission("AccountsPayable", "PurchaseOrders", "Post")]
    public async Task<IActionResult> Credit(long recId, [FromBody] VendCreditNoteDto input, CancellationToken ct) =>
        Present(await service.CreditAsync(recId, input, ct));

    private IActionResult Present(PurchResult result) => result.StatusCode switch
    {
        200 => Ok(APIResponse<object>.Ok(result.Data!, result.Message ?? string.Empty)),
        400 => BadRequest(APIResponse<object>.Fail(result.Message!)),
        404 => NotFound(APIResponse<object>.Fail(result.Message!)),
        _ => UnprocessableEntity(APIResponse<object>.Fail(result.Message ?? "Vendor return operation failed."))
    };
}
