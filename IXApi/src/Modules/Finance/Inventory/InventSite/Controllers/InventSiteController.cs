using IAX.IXApi.Modules.Identity.Permissions;
using IAX.IXApi.Shared.Application.Contracts;
using Microsoft.AspNetCore.Mvc;

namespace IAX.IXApi.Modules.Finance.Inventory;

[ApiController]
[Route("api/v1/InventSite")]
[DomainPermission("Inventory", "Transactions")]
public sealed class InventSiteController : ControllerBase
{
    private readonly IInventSiteService _sites;

    public InventSiteController(IInventSiteService sites)
    {
        _sites = sites;
    }

    public sealed class SiteInput : SiteInputDto { }

    [HttpGet]
    public async Task<IActionResult> List(CancellationToken ct)
    {
        var sites = await _sites.ListAsync(ct);
        return Ok(APIResponse<object>.Ok(sites));
    }

    [HttpPost]
    public async Task<IActionResult> Create(SiteInput input, CancellationToken ct)
    {
        var result = await _sites.CreateAsync(input, ct);
        return result.Conflict
            ? Conflict(APIResponse<object>.Fail("A site with this ID already exists."))
            : Ok(APIResponse<object>.Ok(result.Site!));
    }

    [HttpPut("{recId:long}")]
    public async Task<IActionResult> Update(long recId, SiteInput input, CancellationToken ct)
    {
        var result = await _sites.UpdateAsync(recId, input, ct);
        if (result.CodeChanged) return UnprocessableEntity(APIResponse<object>.Fail("Site ID cannot be changed after creation."));
        return result.Site == null
            ? NotFound(APIResponse<object>.Fail("Site was not found."))
            : Ok(APIResponse<object>.Ok(result.Site));
    }

    [HttpDelete("{recId:long}")]
    public async Task<IActionResult> Delete(long recId, CancellationToken ct)
    {
        var result = await _sites.DeleteAsync(recId, ct);
        return result switch
        {
            InventSiteDeleteResult.NotFound => NotFound(APIResponse<object>.Fail("Site was not found.")),
            InventSiteDeleteResult.HasWarehouses => Conflict(APIResponse<object>.Fail("Delete the site's warehouses before deleting the site.")),
            _ => Ok(APIResponse<bool>.Ok(true))
        };
    }

}
