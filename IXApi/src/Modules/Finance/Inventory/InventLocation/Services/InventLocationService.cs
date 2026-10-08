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
        var locations = await _db.Set<InventLocation>().AsNoTracking()
            .OrderBy(x => x.InventLocationId).ToListAsync(ct);
        return locations.Adapt<List<InventLocationResponseDto>>();
    }

    public async Task<object> LookupsAsync(CancellationToken ct)
    {
        var sites = await _db.Set<InventSite>().AsNoTracking().OrderBy(x => x.SiteId)
            .Select(x => new { id = x.SiteId, code = x.SiteId, name = x.Name }).ToListAsync(ct);
        var warehouses = await _db.Set<InventLocation>().AsNoTracking().OrderBy(x => x.InventLocationId)
            .Select(x => new { id = x.InventLocationId, code = x.InventLocationId, name = x.Name, siteId = x.InventSiteId }).ToListAsync(ct);
        return new { sites, warehouses };
    }

    public async Task<LocationOperationResult> CreateAsync(LocationInputDto input, CancellationToken ct)
    {
        var code = input.InventLocationId.Trim().ToUpperInvariant();
        Normalize(input);
        if (await _db.Set<InventLocation>().AnyAsync(x => x.InventLocationId == code, ct))
            return new(null, LocationOperationStatus.Duplicate);
        if (!await _db.Set<InventSite>().AnyAsync(x => x.SiteId == input.InventSiteId, ct))
            return new(null, LocationOperationStatus.SiteNotFound);
        var referenceError = await ValidateWarehouseReferences(input, code, ct);
        if (referenceError != null) return new(null, LocationOperationStatus.InvalidReference, referenceError);
        var location = new InventLocation { InventLocationId = code, DataAreaId = _company.GetDataAreaId() ?? "dat" };
        input.Adapt<LocationInputDto, InventLocation>(location);
        _db.Set<InventLocation>().Add(location);
        await _unitOfWork.CompleteAsync(ct);
        return new(location.Adapt<InventLocationResponseDto>(), LocationOperationStatus.Success);
    }

    public async Task<LocationOperationResult> UpdateAsync(long recId, LocationInputDto input, CancellationToken ct)
    {
        var location = await _db.Set<InventLocation>().FirstOrDefaultAsync(x => x.RecId == recId, ct);
        if (location == null) return new(null, LocationOperationStatus.NotFound);
        Normalize(input);
        if (!(await _updateValidator.ValidateAsync(new InventLocationUpdateValidation(location.InventLocationId, input), ct)).IsValid)
            return new(null, LocationOperationStatus.CodeChanged);
        if (!await _db.Set<InventSite>().AnyAsync(x => x.SiteId == input.InventSiteId, ct))
            return new(null, LocationOperationStatus.SiteNotFound);
        var referenceError = await ValidateWarehouseReferences(input, location.InventLocationId, ct);
        if (referenceError != null) return new(null, LocationOperationStatus.InvalidReference, referenceError);
        input.Adapt<LocationInputDto, InventLocation>(location);
        await _unitOfWork.CompleteAsync(ct);
        return new(location.Adapt<InventLocationResponseDto>(), LocationOperationStatus.Success);
    }

    public async Task<LocationOperationStatus> DeleteAsync(long recId, CancellationToken ct)
    {
        var location = await _db.Set<InventLocation>().FirstOrDefaultAsync(x => x.RecId == recId, ct);
        if (location == null) return LocationOperationStatus.NotFound;
        var code = location.InventLocationId;
        if (await _db.Set<InventLocation>().AnyAsync(x => x.RecId != recId &&
            (x.InventLocationIdTransit == code || x.InventLocationIdQuarantine == code ||
             x.InventLocationIdReqMain == code || x.ItmInventLocationIdGit == code ||
             x.ItmInventLocationIdUnder == code), ct))
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

    private async Task<string?> ValidateWarehouseReferences(LocationInputDto input, string currentCode, CancellationToken ct)
    {
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
            .Where(x => references.Contains(x.InventLocationId) && x.InventSiteId == input.InventSiteId)
            .Select(x => x.InventLocationId).ToListAsync(ct);
        var missing = references.Except(existing, StringComparer.OrdinalIgnoreCase).FirstOrDefault();
        return missing == null ? null : $"Warehouse '{missing}' was not found in site '{input.InventSiteId}'.";
    }

}
