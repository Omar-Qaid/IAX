using FluentValidation;
using IAX.IXApi.Infrastructure.Identity;
using IAX.IXApi.Infrastructure.Persistence.Repositories;
using IAX.IXApi.Infrastructure.Persistence.Services;
using IAX.IXApi.Modules.Finance.Entities;
using IAX.IXApi.Shared.Application.Identity;
using Mapster;
using Microsoft.EntityFrameworkCore;

namespace IAX.IXApi.Modules.Finance.Inventory;

public sealed class InventSiteService : BaseService<InventSite>, IInventSiteService
{
    private readonly ICompanyExecutionContext _company;
    private readonly IValidator<InventSiteUpdateValidation> _updateValidator;

    public InventSiteService(IUnitOfWork unitOfWork, ICurrentUserService currentUser,
        ICompanyExecutionContext company, IValidator<InventSiteUpdateValidation> updateValidator)
        : base(unitOfWork, currentUser)
    {
        _company = company;
        _updateValidator = updateValidator;
    }

    public async Task<List<InventSiteDto>> ListAsync(CancellationToken ct)
    {
        var sites = await _repository.GetQueryable().AsNoTracking().OrderBy(x => x.SiteId).ToListAsync(ct);
        var siteIds = sites.Select(x => x.SiteId).ToList();
        var locations = await _unitOfWork.Repository<InventLocation>().GetQueryable().AsNoTracking()
            .Where(x => siteIds.Contains(x.InventSiteId))
            .OrderBy(x => x.InventSiteId).ThenBy(x => x.InventLocationId).ToListAsync(ct);
        return sites.Select(site => new InventSiteMappingSource(site,
            locations.Where(location => location.InventSiteId == site.SiteId).ToList()).Adapt<InventSiteDto>()).ToList();
    }

    public async Task<(InventSiteDto? Site, bool Conflict)> CreateAsync(SiteInputDto input, CancellationToken ct)
    {
        var code = input.SiteId.Trim().ToUpperInvariant();
        if (await _repository.GetQueryable().AnyAsync(x => x.SiteId == code, ct))
            return (null, true);
        var site = new InventSite { SiteId = code, DataAreaId = _company.GetDataAreaId() ?? "dat" };
        input.Adapt<SiteInputDto, InventSite>(site);
        await AddAsync(site, ct);
        return (new InventSiteMappingSource(site, []).Adapt<InventSiteDto>(), false);
    }

    public async Task<(InventSiteDto? Site, bool CodeChanged)> UpdateAsync(long recId, SiteInputDto input, CancellationToken ct)
    {
        var site = await _repository.GetQueryable().FirstOrDefaultAsync(x => x.RecId == recId, ct);
        if (site == null) return (null, false);
        var validation = await _updateValidator.ValidateAsync(new InventSiteUpdateValidation(site.SiteId, input), ct);
        if (!validation.IsValid) return (null, true);
        input.Adapt<SiteInputDto, InventSite>(site);
        await base.UpdateAsync(site, ct);
        var locations = await _unitOfWork.Repository<InventLocation>().GetQueryable().AsNoTracking()
            .Where(x => x.InventSiteId == site.SiteId).OrderBy(x => x.InventLocationId).ToListAsync(ct);
        return (new InventSiteMappingSource(site, locations).Adapt<InventSiteDto>(), false);
    }

    public async Task<InventSiteDeleteResult> DeleteAsync(long recId, CancellationToken ct)
    {
        var site = await _repository.GetQueryable().FirstOrDefaultAsync(x => x.RecId == recId, ct);
        if (site == null) return InventSiteDeleteResult.NotFound;
        if (await _unitOfWork.Repository<InventLocation>().GetQueryable().AnyAsync(x => x.InventSiteId == site.SiteId, ct))
            return InventSiteDeleteResult.HasWarehouses;
        await RemoveAsync(site, ct);
        return InventSiteDeleteResult.Deleted;
    }

}
