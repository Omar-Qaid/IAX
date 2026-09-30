using System.ComponentModel.DataAnnotations;
using IAX.IXApi.Modules.Finance.Common;
using IAX.IXApi.Modules.Finance.Entities;
using IAX.IXApi.Modules.Finance.Persistence;
using IAX.IXApi.Modules.Identity.Permissions;
using IAX.IXApi.Shared.Application.Contracts;
using IAX.IXApi.Shared.Application.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace IAX.IXApi.Modules.Finance.Inventory;

[ApiController]
[Route("api/v1/InventSite")]
[DomainPermission("Inventory", "Transactions")]
public sealed class InventSiteController : ControllerBase
{
    private readonly IFinanceDataContext _db;
    private readonly ICompanyExecutionContext _company;

    public InventSiteController(IFinanceDataContext db, ICompanyExecutionContext company)
    {
        _db = db;
        _company = company;
    }

    public sealed class SiteInput
    {
        [Required, StringLength(FieldLengths.InventSiteId)] public string SiteId { get; set; } = string.Empty;
        [Required, StringLength(FieldLengths.Name)] public string Name { get; set; } = string.Empty;
        [StringLength(FieldLengths.DefaultInventStatusID)] public string DefaultInventStatusId { get; set; } = string.Empty;
        public int TimeZone { get; set; }
        public bool IsReceivingWarehouseOverrideAllowed { get; set; }
        public long DefaultDimension { get; set; }
    }

    [HttpGet]
    public async Task<IActionResult> List(CancellationToken ct)
    {
        var sites = await _db.Set<InventSite>().AsNoTracking().OrderBy(x => x.SiteId).ToListAsync(ct);
        var siteIds = sites.Select(x => x.SiteId).ToList();
        var locations = await _db.Set<InventLocation>().AsNoTracking()
            .Where(x => siteIds.Contains(x.InventSiteId))
            .OrderBy(x => x.InventSiteId).ThenBy(x => x.InventLocationId).ToListAsync(ct);
        return Ok(APIResponse<object>.Ok(sites.Select(site => Map(site,
            locations.Where(location => location.InventSiteId == site.SiteId)))));
    }

    [HttpPost]
    public async Task<IActionResult> Create(SiteInput input, CancellationToken ct)
    {
        var code = input.SiteId.Trim().ToUpperInvariant();
        if (await _db.Set<InventSite>().AnyAsync(x => x.SiteId == code, ct))
            return Conflict(APIResponse<object>.Fail("A site with this ID already exists."));
        var site = Apply(new InventSite { SiteId = code, DataAreaId = _company.GetDataAreaId() ?? "dat" }, input);
        _db.Set<InventSite>().Add(site);
        await _db.SaveChangesAsync(ct);
        return Ok(APIResponse<object>.Ok(Map(site, [])));
    }

    [HttpPut("{recId:long}")]
    public async Task<IActionResult> Update(long recId, SiteInput input, CancellationToken ct)
    {
        var site = await _db.Set<InventSite>().FirstOrDefaultAsync(x => x.RecId == recId, ct);
        if (site == null) return NotFound(APIResponse<object>.Fail("Site was not found."));
        if (!string.Equals(site.SiteId, input.SiteId.Trim(), StringComparison.OrdinalIgnoreCase))
            return UnprocessableEntity(APIResponse<object>.Fail("Site ID cannot be changed after creation."));
        Apply(site, input);
        await _db.SaveChangesAsync(ct);
        var locations = await _db.Set<InventLocation>().AsNoTracking()
            .Where(x => x.InventSiteId == site.SiteId).OrderBy(x => x.InventLocationId).ToListAsync(ct);
        return Ok(APIResponse<object>.Ok(Map(site, locations)));
    }

    [HttpDelete("{recId:long}")]
    public async Task<IActionResult> Delete(long recId, CancellationToken ct)
    {
        var site = await _db.Set<InventSite>().FirstOrDefaultAsync(x => x.RecId == recId, ct);
        if (site == null) return NotFound(APIResponse<object>.Fail("Site was not found."));
        if (await _db.Set<InventLocation>().AnyAsync(x => x.InventSiteId == site.SiteId, ct))
            return Conflict(APIResponse<object>.Fail("Delete the site's warehouses before deleting the site."));
        _db.Set<InventSite>().Remove(site);
        await _db.SaveChangesAsync(ct);
        return Ok(APIResponse<bool>.Ok(true));
    }

    private static InventSite Apply(InventSite site, SiteInput input)
    {
        site.Name = input.Name.Trim();
        site.DefaultInventStatusID = input.DefaultInventStatusId.Trim();
        site.TimeZone = (Timezone)input.TimeZone;
        site.IsReceivingWarehouseOverrideAllowed = input.IsReceivingWarehouseOverrideAllowed ? NoYes.Yes : NoYes.No;
        site.DefaultDimension = input.DefaultDimension;
        return site;
    }

    private static object Map(InventSite site, IEnumerable<InventLocation> locations) => new
    {
        id = site.RecId.ToString(), site.RecId, site.SiteId, site.Name,
        defaultInventStatusId = site.DefaultInventStatusID, timeZone = (int)site.TimeZone,
        isReceivingWarehouseOverrideAllowed = site.IsReceivingWarehouseOverrideAllowed == NoYes.Yes,
        site.DefaultDimension,
        warehouses = locations.Select(x => new { id = x.RecId.ToString(), x.InventLocationId, x.Name, x.InventSiteId })
    };
}
