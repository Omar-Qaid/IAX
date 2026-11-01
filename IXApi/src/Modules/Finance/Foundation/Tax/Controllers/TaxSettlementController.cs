using IAX.IXApi.Modules.Identity.Permissions;
using IAX.IXApi.Shared.Application.Contracts;
using Microsoft.AspNetCore.Mvc;

namespace IAX.IXApi.Modules.Finance.Foundation.Tax;

[ApiController]
[Route("api/v1/tax/settlement")]
[DomainPermission("Tax", "Settlement", "View")]
public sealed class TaxSettlementController(ITaxSettlementService service) : ControllerBase
{
    /// <summary>What a settlement run would post, without posting it.</summary>
    [HttpPost("preview")]
    public async Task<IActionResult> Preview([FromBody] TaxSettlementInputDto input, CancellationToken ct)
    {
        var (result, error) = await service.PreviewAsync(input, ct);
        return error is null
            ? Ok(APIResponse<TaxSettlementResultDto>.Ok(result!))
            : UnprocessableEntity(APIResponse<object>.Fail(error));
    }

    [HttpPost]
    [DomainPermission("Tax", "Settlement", "Edit")]
    public async Task<IActionResult> SettleAndPost([FromBody] TaxSettlementInputDto input, CancellationToken ct)
    {
        var (result, error) = await service.SettleAndPostAsync(input, ct);
        return error is null
            ? Ok(APIResponse<TaxSettlementResultDto>.Ok(result!, "Sales tax settled and posted."))
            : UnprocessableEntity(APIResponse<object>.Fail(error));
    }
}
