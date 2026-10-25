using FluentValidation;
using IAX.IXApi.Modules.Identity.Permissions;
using IAX.IXApi.Shared.Application.Contracts;
using Microsoft.AspNetCore.Mvc;

namespace IAX.IXApi.Modules.Finance.AccountsPayable;

[ApiController]
[Route("api/v1/VendTrans")]
[DomainPermission("AccountsPayable", "Transactions", "View")]
public sealed class VendTransController(IVendTransReadService service, IValidator<VendTransQueryDto> validator)
    : ControllerBase
{
    [HttpGet("{recId:long}")]
    public async Task<ActionResult<APIResponse<VendTransDto>>> Get(long recId, CancellationToken ct)
    {
        var transaction = await service.GetAsync(recId, ct);
        return transaction is null
            ? NotFound(APIResponse<VendTransDto>.Fail("Vendor transaction was not found in the selected company."))
            : Ok(APIResponse<VendTransDto>.Ok(transaction));
    }

    [HttpGet("paged")]
    public async Task<ActionResult<APIResponse<VendTransPageDto>>> Paged(
        [FromQuery] VendTransQueryDto query, CancellationToken ct)
    {
        var validation = await validator.ValidateAsync(query, ct);
        if (!validation.IsValid)
            return BadRequest(APIResponse<VendTransPageDto>.Fail(
                string.Join("; ", validation.Errors.Select(x => x.ErrorMessage))));
        return Ok(APIResponse<VendTransPageDto>.Ok(await service.SearchAsync(query, ct)));
    }
}
