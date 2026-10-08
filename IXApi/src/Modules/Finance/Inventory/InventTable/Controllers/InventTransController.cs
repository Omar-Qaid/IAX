using IAX.IXApi.Modules.Identity.Permissions;
using IAX.IXApi.Shared.Application.Contracts;
using Microsoft.AspNetCore.Mvc;

namespace IAX.IXApi.Modules.Finance.Inventory;

[ApiController]
[Route("api/v1/InventTrans")]
[DomainPermission("Inventory", "Transactions", "View")]
public sealed class InventTransController(
    IInventTransService service) : ControllerBase
{
    [HttpGet("list")]
    public async Task<ActionResult<APIResponse<IEnumerable<InventTransListDto>>>> GetList(
        CancellationToken cancellationToken = default)
    {
        var result = await service.GetListAsync(cancellationToken);
        if (result == null)
            return BadRequest(APIResponse<IEnumerable<InventTransListDto>>.Fail(
                "A company must be selected to view inventory transactions."));
        return Ok(APIResponse<IEnumerable<InventTransListDto>>.Ok(result));
    }
}
