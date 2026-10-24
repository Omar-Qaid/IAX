using IAX.IXApi.Modules.Identity.Permissions;
using IAX.IXApi.Shared.Application.Contracts;
using Microsoft.AspNetCore.Mvc;

namespace IAX.IXApi.Modules.Finance.AccountsReceivable.Return;

[ApiController]
[Route("api/v1/return-orders")]
[DomainPermission("AccountsReceivable", "ReturnOrders", "View")]
public sealed class CustReturnController(ICustReturnService service) : ControllerBase
{
    [HttpPost]
    [DomainPermission("AccountsReceivable", "ReturnOrders", "Create")]
    public async Task<IActionResult> Create([FromBody] CustReturnCreateDto input, CancellationToken ct) =>
        Present(await service.CreateAsync(input, ct));

    [HttpPost("{recId:long}/receipts")]
    [DomainPermission("AccountsReceivable", "ReturnOrders", "Post")]
    public async Task<IActionResult> Receive(long recId, [FromBody] CustReturnReceiptDto input, CancellationToken ct) =>
        Present(await service.ReceiveAsync(recId, input, ct));

    [HttpPost("{recId:long}/credit-notes")]
    [DomainPermission("AccountsReceivable", "ReturnOrders", "Post")]
    public async Task<IActionResult> Credit(long recId, [FromBody] CustCreditNoteDto? input, CancellationToken ct) =>
        Present(await service.CreditAsync(recId, input ?? new CustCreditNoteDto(), ct));

    private IActionResult Present(CustReturnResult result) => result.StatusCode switch
    {
        200 => Ok(APIResponse<object>.Ok(result.Data!, result.Message ?? string.Empty)),
        400 => BadRequest(APIResponse<object>.Fail(result.Message!)),
        404 => NotFound(APIResponse<object>.Fail(result.Message!)),
        _ => UnprocessableEntity(APIResponse<object>.Fail(result.Message ?? "Return order operation failed."))
    };
}
