using FluentValidation;
using IAX.IXApi.Modules.Finance.Common;
using IAX.IXApi.Modules.Finance.Entities;
using IAX.IXApi.Infrastructure.Persistence.Repositories;
using IAX.IXApi.Infrastructure.Persistence.Services;
using IAX.IXApi.Infrastructure.Identity;
using Mapster;
using IAX.IXApi.Shared.Application.Identity;
using Microsoft.EntityFrameworkCore;

namespace IAX.IXApi.Modules.Finance.Inventory;

public sealed class InventLocationService : BaseService<InventLocation>, IInventLocationService
{
    private DbContext _db => _unitOfWork.Context;
    private readonly ICompanyExecutionContext _company;
    private readonly IValidator<InventLocationUpdateValidation> _updateValidator;

    public InventLocationService(IUnitOfWork unitOfWork, ICurrentUserService currentUser, ICompanyExecutionContext company,
        IValidator<InventLocationUpdateValidation> updateValidator)
        : base(unitOfWork, currentUser)
    {
        _company = company;
        _updateValidator = updateValidator;
    }

    public async Task<List<InventLocationResponseDto>> ListAsync(CancellationToken ct)
    {
        var companyId = RequireCompany();
        var locations = await _db.Set<InventLocation>().AsNoTracking()
            .Where(x => x.DataAreaId == companyId)
            .OrderBy(x => x.InventLocationId).ToListAsync(ct);
        return locations.Adapt<List<InventLocationResponseDto>>();
    }

    public async Task<object> LookupsAsync(CancellationToken ct)
    {
        var companyId = RequireCompany();
        var sites = await _db.Set<InventSite>().AsNoTracking().Where(x => x.DataAreaId == companyId).OrderBy(x => x.SiteId)
            .ProjectToType<InventSiteLookupDto>().ToListAsync(ct);
        var warehouses = await _db.Set<InventLocation>().AsNoTracking().Where(x => x.DataAreaId == companyId).OrderBy(x => x.InventLocationId)
            .ProjectToType<InventWarehouseLookupDto>().ToListAsync(ct);
        return new InventLocationLookupDto { Sites = sites, Warehouses = warehouses };
    }

    public async Task<LocationOperationResult> CreateAsync(LocationInputDto input, CancellationToken ct)
    {
        var companyId = RequireCompany();
        var code = input.InventLocationId.Trim().ToUpperInvariant();
        Normalize(input);
        if (UsesAdvancedWarehouse(input))
            return new(null, LocationOperationStatus.InvalidReference, "Advanced warehouse options are outside the basic inventory scope.");
        if (await _db.Set<InventLocation>().AnyAsync(x => x.DataAreaId == companyId && x.InventLocationId == code, ct))
            return new(null, LocationOperationStatus.Duplicate);
        if (!await _db.Set<InventSite>().AnyAsync(x => x.DataAreaId == companyId && x.SiteId == input.InventSiteId, ct))
            return new(null, LocationOperationStatus.SiteNotFound);
        var referenceError = await ValidateWarehouseReferences(input, code, ct);
        if (referenceError != null) return new(null, LocationOperationStatus.InvalidReference, referenceError);
        var location = new InventLocation { InventLocationId = code, DataAreaId = companyId };
        input.Adapt<LocationInputDto, InventLocation>(location);
        _db.Set<InventLocation>().Add(location);
        await _unitOfWork.CompleteAsync(ct);
        return new(location.Adapt<InventLocationResponseDto>(), LocationOperationStatus.Success);
    }

    public async Task<LocationOperationResult> UpdateAsync(long recId, LocationInputDto input, CancellationToken ct)
    {
        var companyId = RequireCompany();
        var location = await _db.Set<InventLocation>().FirstOrDefaultAsync(x => x.DataAreaId == companyId && x.RecId == recId, ct);
        if (location == null) return new(null, LocationOperationStatus.NotFound);
        Normalize(input);
        if (UsesAdvancedWarehouse(input))
            return new(null, LocationOperationStatus.InvalidReference, "Advanced warehouse options are outside the basic inventory scope.");
        if (!(await _updateValidator.ValidateAsync(new InventLocationUpdateValidation(location.InventLocationId, input), ct)).IsValid)
            return new(null, LocationOperationStatus.CodeChanged);
        if (location.InventSiteId != input.InventSiteId &&
            (await _db.Set<InventDim>().AnyAsync(x => x.DataAreaId == companyId && x.InventLocationId == location.InventLocationId, ct)
             || await _db.Set<InventSum>().AnyAsync(x => x.DataAreaId == companyId && x.InventLocationId == location.InventLocationId, ct)
             || await _db.Set<InventJournalTable>().AnyAsync(x => x.DataAreaId == companyId && x.InventLocationId == location.InventLocationId, ct)))
            return new(null, LocationOperationStatus.InvalidReference, "Warehouse site cannot be changed after inventory dimensions, balances, or journals reference it.");
        if (!await _db.Set<InventSite>().AnyAsync(x => x.DataAreaId == companyId && x.SiteId == input.InventSiteId, ct))
            return new(null, LocationOperationStatus.SiteNotFound);
        var referenceError = await ValidateWarehouseReferences(input, location.InventLocationId, ct);
        if (referenceError != null) return new(null, LocationOperationStatus.InvalidReference, referenceError);
        input.Adapt<LocationInputDto, InventLocation>(location);
        await _unitOfWork.CompleteAsync(ct);
        return new(location.Adapt<InventLocationResponseDto>(), LocationOperationStatus.Success);
    }

    public async Task<LocationOperationStatus> DeleteAsync(long recId, CancellationToken ct)
    {
        var companyId = RequireCompany();
        var location = await _db.Set<InventLocation>().FirstOrDefaultAsync(x => x.DataAreaId == companyId && x.RecId == recId, ct);
        if (location == null) return LocationOperationStatus.NotFound;
        var code = location.InventLocationId;
        if (await _db.Set<InventLocation>().AnyAsync(x => x.DataAreaId == companyId && x.RecId != recId &&
            (x.InventLocationIdTransit == code || x.InventLocationIdQuarantine == code ||
             x.InventLocationIdReqMain == code || x.ItmInventLocationIdGit == code ||
             x.ItmInventLocationIdUnder == code), ct))
            return LocationOperationStatus.Referenced;
        if (await _db.Set<InventDim>().AnyAsync(x => x.DataAreaId == companyId && x.InventLocationId == code, ct)
            || await _db.Set<InventSum>().AnyAsync(x => x.DataAreaId == companyId && x.InventLocationId == code, ct)
            || await _db.Set<InventJournalTable>().AnyAsync(x => x.DataAreaId == companyId && x.InventLocationId == code, ct))
            return LocationOperationStatus.Referenced;
        _db.Set<InventLocation>().Remove(location);
        await _unitOfWork.CompleteAsync(ct);
        return LocationOperationStatus.Success;
    }

    private static void Normalize(LocationInputDto input)
    {
        input.InventSiteId = input.InventSiteId.Trim().ToUpperInvariant();
        input.InventLocationIdTransit = input.InventLocationIdTransit.Trim().ToUpperInvariant();
        input.InventLocationIdQuarantine = input.InventLocationIdQuarantine.Trim().ToUpperInvariant();
        input.InventLocationIdReqMain = input.InventLocationIdReqMain.Trim().ToUpperInvariant();
        input.ItmInventLocationIdGit = input.ItmInventLocationIdGit.Trim().ToUpperInvariant();
        input.ItmInventLocationIdUnder = input.ItmInventLocationIdUnder.Trim().ToUpperInvariant();
    }

    private static bool UsesAdvancedWarehouse(LocationInputDto input) =>
        input.WhsEnabled || input.UseWmsOrders || input.EnableExternalWarehouse ||
        input.WmsAisleNameActive || input.WmsRackNameActive || input.WmsLevelNameActive || input.WmsPositionNameActive ||
        input.WmsLocationIdDefaultReceipt.Length > 0 || input.WmsLocationIdDefaultIssue.Length > 0 ||
        input.InventLocationIdTransit.Length > 0 || input.InventLocationIdQuarantine.Length > 0;

    private async Task<string?> ValidateWarehouseReferences(LocationInputDto input, string currentCode, CancellationToken ct)
    {
        var companyId = RequireCompany();
        var references = new[]
        {
            input.InventLocationIdTransit,
            input.InventLocationIdQuarantine,
            input.InventLocationIdReqMain,
            input.ItmInventLocationIdGit,
            input.ItmInventLocationIdUnder
        }.Where(x => !string.IsNullOrEmpty(x)).Distinct(StringComparer.OrdinalIgnoreCase).ToArray();

        if (references.Any(x => string.Equals(x, currentCode, StringComparison.OrdinalIgnoreCase)))
            return "A warehouse cannot reference itself.";
        if (references.Length == 0) return null;

        var existing = await _db.Set<InventLocation>().AsNoTracking()
            .Where(x => x.DataAreaId == companyId && references.Contains(x.InventLocationId) && x.InventSiteId == input.InventSiteId)
            .Select(x => x.InventLocationId).ToListAsync(ct);
        var missing = references.Except(existing, StringComparer.OrdinalIgnoreCase).FirstOrDefault();
        return missing == null ? null : $"Warehouse '{missing}' was not found in site '{input.InventSiteId}'.";
    }

    private string RequireCompany() => _company.GetDataAreaId()
        ?? throw new InvalidOperationException("A company must be selected.");

}
