using IAX.IXApi.Infrastructure.Persistence.Repositories;
using IAX.IXApi.Modules.Finance.Common;
using IAX.IXApi.Modules.Finance.Entities;
using IAX.IXApi.Shared.Application.Identity;
using Microsoft.EntityFrameworkCore;

namespace IAX.IXApi.Modules.Finance.Inventory;

public sealed class InventParametersService(IUnitOfWork unitOfWork, ICompanyExecutionContext company) : IInventParametersService
{
    private DbContext Db => unitOfWork.Context;
    public async Task<InventParametersDto?> GetAsync(CancellationToken ct = default)
    {
        var companyId = company.GetDataAreaId(); if (string.IsNullOrWhiteSpace(companyId)) return null;
        var row = await Db.Set<InventParameters>().AsNoTracking().SingleOrDefaultAsync(x => x.DataAreaId == companyId, ct);
        return row is null ? new InventParametersDto() : Map(row);
    }
    public async Task<InventParametersLookupsDto?> GetLookupsAsync(CancellationToken ct = default)
    {
        var companyId = company.GetDataAreaId();
        if (string.IsNullOrWhiteSpace(companyId)) return null;
        return new InventParametersLookupsDto
        {
            Units = await Db.Set<UnitOfMeasure>().AsNoTracking().OrderBy(x => x.Symbol).Select(x => new InventParametersLookupDto(x.Symbol, x.Symbol)).ToListAsync(ct),
            Sites = await Db.Set<InventSite>().AsNoTracking().Where(x => x.DataAreaId == companyId).OrderBy(x => x.SiteId).Select(x => new InventParametersLookupDto(x.SiteId, x.Name)).ToListAsync(ct),
            Warehouses = await Db.Set<InventLocation>().AsNoTracking().Where(x => x.DataAreaId == companyId).OrderBy(x => x.InventLocationId).Select(x => new InventParametersWarehouseLookupDto(x.InventLocationId, x.Name, x.InventSiteId)).ToListAsync(ct),
            JournalNames = await Db.Set<InventJournalName>().AsNoTracking().Where(x => x.DataAreaId == companyId).OrderBy(x => x.JournalNameId).Select(x => new InventParametersJournalLookupDto(x.JournalNameId, x.Description, (int)x.JournalType)).ToListAsync(ct),
        };
    }
    public async Task<(InventParametersDto? Value, string? Error)> SaveAsync(InventParametersDto input, CancellationToken ct = default)
    {
        var companyId = company.GetDataAreaId(); if (string.IsNullOrWhiteSpace(companyId)) return (null, "A company must be selected.");
        input.DefaultUnitId = input.DefaultUnitId.Trim(); input.MultiSiteDefaultInventSiteId = input.MultiSiteDefaultInventSiteId.Trim().ToUpperInvariant(); input.MultiSiteDefaultInventLocationId = input.MultiSiteDefaultInventLocationId.Trim().ToUpperInvariant();
        if (input.DefaultUnitId.Length > 0 && !await Db.Set<UnitOfMeasure>().AnyAsync(x => x.Symbol == input.DefaultUnitId, ct)) return (null, "Default unit was not found.");
        if (input.MultiSiteDefaultInventSiteId.Length > 0 && !await Db.Set<InventSite>().AnyAsync(x => x.DataAreaId == companyId && x.SiteId == input.MultiSiteDefaultInventSiteId, ct)) return (null, "Default site was not found in the selected company.");
        if (input.MultiSiteDefaultInventLocationId.Length > 0 && !await Db.Set<InventLocation>().AnyAsync(x => x.DataAreaId == companyId && x.InventLocationId == input.MultiSiteDefaultInventLocationId && x.InventSiteId == input.MultiSiteDefaultInventSiteId, ct)) return (null, "Default warehouse must belong to the default site in the selected company.");
        var journals = new[] { (input.MovementJournalNameId, InventJournalType.Movement), (input.LossProfitJournalNameId, InventJournalType.Adjustment), (input.TransferJournalNameId, InventJournalType.Transfer), (input.CountJournalNameId, InventJournalType.Counting) };
        foreach (var (id, type) in journals.Where(x => !string.IsNullOrWhiteSpace(x.Item1)))
            if (!await Db.Set<InventJournalName>().AnyAsync(x => x.DataAreaId == companyId && x.JournalNameId == id.Trim() && x.JournalType == type, ct)) return (null, $"Journal name '{id}' does not exist for {type} in the selected company.");
        var row = await Db.Set<InventParameters>().SingleOrDefaultAsync(x => x.DataAreaId == companyId, ct) ?? new InventParameters { DataAreaId = companyId };
        if (row.RecId == 0) Db.Add(row);
        row.DefaultUnitId = input.DefaultUnitId; row.MultiSiteDefaultInventSiteId = input.MultiSiteDefaultInventSiteId; row.MultiSiteDefaultInventLocationId = input.MultiSiteDefaultInventLocationId;
        row.MovementJournalNameId = input.MovementJournalNameId.Trim(); row.LossProfitJournalNameId = input.LossProfitJournalNameId.Trim(); row.TransferJournalNameId = input.TransferJournalNameId.Trim(); row.CountJournalNameId = input.CountJournalNameId.Trim();
        row.ReserveOnOrdered = input.ReserveOnOrdered ? NoYes.Yes : NoYes.No; row.MandatoryItemTaxGroup = input.MandatoryItemTaxGroup ? NoYes.Yes : NoYes.No;
        await unitOfWork.CompleteAsync(ct); return (Map(row), null);
    }
    private static InventParametersDto Map(InventParameters x) => new() { RecId = x.RecId, DefaultUnitId = x.DefaultUnitId, MultiSiteDefaultInventSiteId = x.MultiSiteDefaultInventSiteId, MultiSiteDefaultInventLocationId = x.MultiSiteDefaultInventLocationId, MovementJournalNameId = x.MovementJournalNameId, LossProfitJournalNameId = x.LossProfitJournalNameId, TransferJournalNameId = x.TransferJournalNameId, CountJournalNameId = x.CountJournalNameId, ReserveOnOrdered = x.ReserveOnOrdered == NoYes.Yes, MandatoryItemTaxGroup = x.MandatoryItemTaxGroup == NoYes.Yes };
}
