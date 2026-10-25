using IAX.IXApi.Infrastructure.Identity;
using IAX.IXApi.Infrastructure.Persistence.Repositories;
using IAX.IXApi.Infrastructure.Persistence.Services;
using IAX.IXApi.Modules.Finance.Common;
using IAX.IXApi.Modules.Finance.Entities;
using IAX.IXApi.Shared.Application.Identity;
using Microsoft.EntityFrameworkCore;

namespace IAX.IXApi.Modules.Finance.Inventory;

public sealed class WMSLocationService : BaseService<WMSLocation>, IWMSLocationService
{
    private readonly ICompanyExecutionContext _company;
    private DbContext Db => _unitOfWork.Context;

    public WMSLocationService(IUnitOfWork unitOfWork, ICurrentUserService currentUser, ICompanyExecutionContext company)
        : base(unitOfWork, currentUser)
    {
        _company = company;
    }

    public WMSLocationService(IUnitOfWork unitOfWork, ICompanyExecutionContext company)
        : this(unitOfWork, NullCurrentUserService.Instance, company)
    {
    }

    public async Task<List<WMSLocationDto>> ListAsync(CancellationToken ct)
    {
        var companyId = RequireCompany();
        return await (from location in Db.Set<WMSLocation>().AsNoTracking()
                      join warehouse in Db.Set<InventLocation>().AsNoTracking()
                          on new { location.DataAreaId, location.InventLocationId } equals new { warehouse.DataAreaId, warehouse.InventLocationId }
                      where location.DataAreaId == companyId
                      orderby location.InventLocationId, location.WMSLocationId
                      select new WMSLocationDto
                      {
                          RecId = location.RecId, InventLocationId = location.InventLocationId,
                          WarehouseName = warehouse.Name, WMSLocationId = location.WMSLocationId,
                          InputLocation = location.InputLocation, CheckText = location.CheckText,
                          LocationType = location.LocationType, SortCode = location.SortCode
                      }).ToListAsync(ct);
    }

    public async Task<(WMSLocationDto? Location, string? Error)> CreateAsync(WMSLocationInputDto input, CancellationToken ct)
    {
        var companyId = RequireCompany();
        Normalize(input);
        var error = await ValidateAsync(companyId, input, null, ct);
        if (error != null) return (null, error);
        var entity = new WMSLocation
        {
            DataAreaId = companyId, InventLocationId = input.InventLocationId, WMSLocationId = input.WMSLocationId,
            InputLocation = input.InputLocation, CheckText = input.CheckText, LocationType = input.LocationType,
            SortCode = input.SortCode, ManualName = NoYes.No
        };
        Db.Add(entity);
        await _unitOfWork.CompleteAsync(ct);
        return (await MapAsync(entity, companyId, ct), null);
    }

    public async Task<(WMSLocationDto? Location, string? Error)> UpdateAsync(long recId, WMSLocationInputDto input, CancellationToken ct)
    {
        var companyId = RequireCompany();
        var entity = await Db.Set<WMSLocation>().FirstOrDefaultAsync(x => x.DataAreaId == companyId && x.RecId == recId, ct);
        if (entity == null) return (null, "Inventory location was not found.");
        Normalize(input);
        if (input.InventLocationId != entity.InventLocationId || input.WMSLocationId != entity.WMSLocationId)
            return (null, "Warehouse and location codes cannot be changed after creation.");
        var error = await ValidateAsync(companyId, input, recId, ct);
        if (error != null) return (null, error);
        entity.InputLocation = input.InputLocation;
        entity.CheckText = input.CheckText;
        entity.LocationType = input.LocationType;
        entity.SortCode = input.SortCode;
        await _unitOfWork.CompleteAsync(ct);
        return (await MapAsync(entity, companyId, ct), null);
    }

    public async Task<string?> DeleteAsync(long recId, CancellationToken ct)
    {
        var companyId = RequireCompany();
        var entity = await Db.Set<WMSLocation>().FirstOrDefaultAsync(x => x.DataAreaId == companyId && x.RecId == recId, ct);
        if (entity == null) return "Inventory location was not found.";
        if (await Db.Set<InventDim>().AnyAsync(x => x.DataAreaId == companyId &&
            x.InventLocationId == entity.InventLocationId && x.WmsLocationId == entity.WMSLocationId, ct))
            return "This inventory location is referenced by inventory dimensions and cannot be deleted.";
        Db.Remove(entity);
        await _unitOfWork.CompleteAsync(ct);
        return null;
    }

    private async Task<string?> ValidateAsync(string companyId, WMSLocationInputDto input, long? ignoreRecId, CancellationToken ct)
    {
        if (input.InventLocationId.Length == 0 || input.WMSLocationId.Length == 0) return "Warehouse and location are required.";
        if (input.WMSLocationId.Length > FieldLengths.WmsLocationId) return $"Location ID cannot exceed {FieldLengths.WmsLocationId} characters.";
        if (!await Db.Set<InventLocation>().AnyAsync(x => x.DataAreaId == companyId && x.InventLocationId == input.InventLocationId, ct))
            return "The selected warehouse does not exist in the current company.";
        if (await Db.Set<WMSLocation>().AnyAsync(x => x.DataAreaId == companyId && x.InventLocationId == input.InventLocationId &&
            x.WMSLocationId == input.WMSLocationId && x.RecId != ignoreRecId, ct))
            return "This location already exists in the selected warehouse.";
        if (input.InputLocation.Length > 0 && !await Db.Set<WMSLocation>().AnyAsync(x => x.DataAreaId == companyId &&
            x.InventLocationId == input.InventLocationId && x.WMSLocationId == input.InputLocation, ct))
            return "The destination location must exist in the same warehouse.";
        if (input.LocationType < 0 || input.SortCode < 0) return "Location type and sort code must be non-negative.";
        return null;
    }

    private async Task<WMSLocationDto> MapAsync(WMSLocation entity, string companyId, CancellationToken ct)
    {
        var name = await Db.Set<InventLocation>().AsNoTracking().Where(x => x.DataAreaId == companyId &&
            x.InventLocationId == entity.InventLocationId).Select(x => x.Name).FirstOrDefaultAsync(ct) ?? string.Empty;
        return new WMSLocationDto
        {
            RecId = entity.RecId, InventLocationId = entity.InventLocationId, WarehouseName = name,
            WMSLocationId = entity.WMSLocationId, InputLocation = entity.InputLocation,
            CheckText = entity.CheckText, LocationType = entity.LocationType, SortCode = entity.SortCode
        };
    }

    private static void Normalize(WMSLocationInputDto input)
    {
        input.InventLocationId = (input.InventLocationId ?? string.Empty).Trim().ToUpperInvariant();
        input.WMSLocationId = (input.WMSLocationId ?? string.Empty).Trim().ToUpperInvariant();
        input.InputLocation = (input.InputLocation ?? string.Empty).Trim().ToUpperInvariant();
        input.CheckText = (input.CheckText ?? string.Empty).Trim();
    }

    private string RequireCompany() => _company.GetDataAreaId() ?? throw new InvalidOperationException("A company must be selected.");
}
