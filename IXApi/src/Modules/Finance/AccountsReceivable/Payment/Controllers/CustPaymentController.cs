using IAX.IXApi.Modules.Identity.Permissions;
using IAX.IXApi.Shared.Application.Contracts;
using Microsoft.AspNetCore.Mvc;

namespace IAX.IXApi.Modules.Finance.AccountsReceivable.Payment;

[ApiController]
[Route("api/v1/customer-payments")]
[DomainPermission("AccountsReceivable", "CustomerPayments", "View")]
public sealed class CustPaymentController(ICustPaymentService service) : ControllerBase
{
    [HttpPost]
    [DomainPermission("AccountsReceivable", "CustomerPayments", "Post")]
    public async Task<IActionResult> Post([FromBody] CustPaymentPostDto request, CancellationToken ct)
    {
        var result = await service.PostAsync(request, ct);
        return result.StatusCode switch
        {
            200 => Ok(APIResponse<object>.Ok(result.Data!, result.Message ?? string.Empty)),
            400 => BadRequest(APIResponse<object>.Fail(result.Message!)),
            404 => NotFound(APIResponse<object>.Fail(result.Message!)),
            _ => UnprocessableEntity(APIResponse<object>.Fail(result.Message ?? "Customer payment posting failed."))
        };
    }
}
