using IAX.IXApi.Modules.Identity.Permissions;
using IAX.IXApi.Shared.Application.Contracts;
using Microsoft.AspNetCore.Mvc;

namespace IAX.IXApi.Modules.Finance.Inventory;

[ApiController]
[Route("api/v1/InventPosting")]
[DomainPermission("Inventory", "Transactions")]
public sealed class InventPostingController(IInventPostingService service) : ControllerBase
{
    [HttpGet]
    public async Task<IActionResult> List([FromQuery] int accountType, CancellationToken ct)
    {
        var rows = await service.ListAsync(accountType, ct);
        return rows is null
            ? BadRequest(APIResponse<List<InventPostingRuleDto>>.Fail("A legal entity must be selected."))
            : Ok(APIResponse<List<InventPostingRuleDto>>.Ok(rows));
    }

    [HttpGet("lookups")]
    public async Task<IActionResult> Lookups(CancellationToken ct)
    {
        var rows = await service.LookupsAsync(ct);
        return rows is null
            ? BadRequest(APIResponse<InventPostingLookupsDto>.Fail("A legal entity must be selected."))
            : Ok(APIResponse<InventPostingLookupsDto>.Ok(rows));
    }

    [HttpPost]
    public async Task<IActionResult> Create(InventPostingRuleDto input, CancellationToken ct)
    {
        var result = await service.CreateAsync(input, ct);
        return result.Rule is null
            ? BadRequest(APIResponse<InventPostingRuleDto>.Fail(result.Error!))
            : Ok(APIResponse<InventPostingRuleDto>.Ok(result.Rule));
    }

    [HttpPut("{recId:long}")]
    public async Task<IActionResult> Update(long recId, InventPostingRuleDto input, CancellationToken ct)
    {
        var result = await service.UpdateAsync(recId, input, ct);
        return result.Rule is null
            ? BadRequest(APIResponse<InventPostingRuleDto>.Fail(result.Error!))
            : Ok(APIResponse<InventPostingRuleDto>.Ok(result.Rule));
    }

    [HttpDelete("{recId:long}")]
    public async Task<IActionResult> Delete(long recId, CancellationToken ct)
    {
        var result = await service.DeleteAsync(recId, ct);
        return result.Deleted
            ? Ok(APIResponse<bool>.Ok(true))
            : NotFound(APIResponse<bool>.Fail(result.Error!));
    }
}
