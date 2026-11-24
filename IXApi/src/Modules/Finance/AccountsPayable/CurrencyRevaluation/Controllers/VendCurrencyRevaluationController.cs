using IAX.IXApi.Modules.Finance.Common;
using IAX.IXApi.Modules.Identity.Permissions;
using IAX.IXApi.Shared.Application.Contracts;
using Microsoft.AspNetCore.Mvc;

namespace IAX.IXApi.Modules.Finance.AccountsPayable.CurrencyRevaluation;

[ApiController]
[Route("api/v1/VendCurrencyRevaluation")]
public sealed class VendCurrencyRevaluationController(IVendCurrencyRevaluationService service) : ControllerBase
{
    [HttpGet("{transactionRecId:long}/preview")]
    [DomainPermission("AccountsPayable", "Transactions", "View")]
    public async Task<ActionResult<APIResponse<VendRevaluationPreview>>> Preview(long transactionRecId, [FromQuery] DateOnly rateDate,
        CancellationToken ct)
    {
        if (rateDate == default) return BadRequest("A rate date is required.");
        try
        {
            var result = await service.PreviewAsync(transactionRecId, rateDate, ct);
            return result is null ? NotFound() : Ok(APIResponse<VendRevaluationPreview>.Ok(result));
        }
        catch (SubledgerPostingException ex)
        {
            return UnprocessableEntity(ex.Message);
        }
    }

    [HttpPost("{transactionRecId:long}/post")]
    [DomainPermission("AccountsPayable", "CurrencyRevaluation", "Post")]
    public Task<IActionResult> Post(long transactionRecId, [FromBody] VendRevaluationPostRequest request, CancellationToken ct) =>
        ExecuteAsync(() => service.PostAsync(transactionRecId, request.PostingDate, request.RateDate, ct));

    [HttpPost("{transactionRecId:long}/reverse")]
    [DomainPermission("AccountsPayable", "CurrencyRevaluation", "Post")]
    public Task<IActionResult> Reverse(long transactionRecId, [FromBody] VendRevaluationReverseRequest request, CancellationToken ct) =>
        ExecuteAsync(() => service.ReverseAsync(transactionRecId, request.PostingDate, ct));

    private async Task<IActionResult> ExecuteAsync(Func<Task<string?>> post)
    {
        try
        {
            var voucher = await post();
            return voucher is null ? NoContent() : Ok(APIResponse<string>.Ok(voucher));
        }
        catch (SubledgerPostingException ex)
        {
            return UnprocessableEntity(APIResponse<string>.Fail(ex.Message));
        }
    }
}
