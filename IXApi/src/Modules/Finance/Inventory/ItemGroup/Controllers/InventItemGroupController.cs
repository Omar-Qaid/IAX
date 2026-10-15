using IAX.IXApi.Modules.Identity.Permissions;
using IAX.IXApi.Shared.Application.Contracts;
using Microsoft.AspNetCore.Mvc;

namespace IAX.IXApi.Modules.Finance.Inventory;

[ApiController]
[Route("api/v1/InventItemGroup")]
[DomainPermission("Inventory", "Transactions")]
public sealed class InventItemGroupController(IInventItemGroupService service) : ControllerBase
{
    [HttpGet]
    public async Task<IActionResult> List(CancellationToken ct)
    {
        var rows = await service.ListAsync(ct);
        return rows is null ? BadRequest(APIResponse<List<InventItemGroupRecordDto>>.Fail("A legal entity must be selected.")) : Ok(APIResponse<List<InventItemGroupRecordDto>>.Ok(rows));
    }

    [HttpGet("ledger-accounts")]
    public async Task<IActionResult> LedgerAccounts(CancellationToken ct)
    {
        var rows = await service.LedgerAccountsAsync(ct);
        return rows is null ? BadRequest(APIResponse<List<InventItemGroupPostingDto>>.Fail("A legal entity must be selected.")) : Ok(APIResponse<List<InventItemGroupPostingDto>>.Ok(rows));
    }

    [HttpPost]
    public async Task<IActionResult> Create(InventItemGroupRecordDto input, CancellationToken ct)
    {
        var result = await service.CreateAsync(input, ct);
        return result.Group is null ? Conflict(APIResponse<InventItemGroupRecordDto>.Fail(result.Error!)) : Ok(APIResponse<InventItemGroupRecordDto>.Ok(result.Group));
    }

    [HttpPut("{recId:long}")]
    public async Task<IActionResult> Update(long recId, InventItemGroupRecordDto input, CancellationToken ct)
    {
        var result = await service.UpdateAsync(recId, input, ct);
        return result.Group is null ? BadRequest(APIResponse<InventItemGroupRecordDto>.Fail(result.Error!)) : Ok(APIResponse<InventItemGroupRecordDto>.Ok(result.Group));
    }

    [HttpDelete("{recId:long}")]
    public async Task<IActionResult> Delete(long recId, CancellationToken ct)
    {
        var result = await service.DeleteAsync(recId, ct);
        return result.Deleted ? Ok(APIResponse<bool>.Ok(true)) : Conflict(APIResponse<bool>.Fail(result.Error!));
    }
}
