using IAX.IXApi.Modules.Identity.Permissions;
using IAX.IXApi.Shared.Application.Contracts;
using Microsoft.AspNetCore.Mvc;

namespace IAX.IXApi.Modules.Finance.AccountsReceivable.Confirm;

[ApiController]
[Route("api/v1/SalesTable/{recId:long}/confirmations")]
[DomainPermission("AccountsReceivable", "SalesOrders", "View")]
public sealed class SalesOrderConfirmationController(ISalesOrderConfirmationService service) : ControllerBase
{
    [HttpGet]
    public async Task<IActionResult> List(long recId, CancellationToken ct) =>
        Present(await service.ListAsync(recId, ct));

    [HttpGet("{confirmationRecId:long}")]
    public async Task<IActionResult> Get(long recId, long confirmationRecId, CancellationToken ct) =>
        Present(await service.GetAsync(recId, confirmationRecId, ct));

    [HttpPost]
    [DomainPermission("AccountsReceivable", "SalesOrders", "Edit")]
    public async Task<IActionResult> Post(long recId, [FromBody] PostConfirmationRequest? request, CancellationToken ct) =>
        Present(await service.PostAsync(recId, request, ct));

    private IActionResult Present(ConfirmationResult result) => result.StatusCode switch
    {
        200 => Ok(APIResponse<object>.Ok(result.Data!, result.Message)),
        400 => BadRequest(APIResponse<object>.Fail(result.Message!)),
        404 => NotFound(APIResponse<object>.Fail(result.Message!)),
        422 => UnprocessableEntity(APIResponse<object>.Fail(result.Message!)),
        _ => StatusCode(result.StatusCode, APIResponse<object>.Fail(result.Message ?? "Confirmation failed."))
    };
}
