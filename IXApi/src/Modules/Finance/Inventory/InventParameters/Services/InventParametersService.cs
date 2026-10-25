using IAX.IXApi.Infrastructure.Persistence.Repositories;
using IAX.IXApi.Modules.Administration.NumberSequences;
using IAX.IXApi.Modules.Finance.Common;
using IAX.IXApi.Modules.Finance.Entities;
using IAX.IXApi.Modules.Finance.Inventory.DimensionGroups.Entities;
using IAX.IXApi.Shared.Application.Identity;
using Microsoft.EntityFrameworkCore;

namespace IAX.IXApi.Modules.Finance.Inventory;

public sealed class InventParametersService(IUnitOfWork unitOfWork, ICompanyExecutionContext company) : IInventParametersService
{
    private DbContext Db => unitOfWork.Context;
    public async Task<InventoryReadinessDto?> GetReadinessAsync(CancellationToken ct = default)
    {
        var companyId = company.GetDataAreaId();
        if (string.IsNullOrWhiteSpace(companyId)) return null;
        var issues = new List<InventoryReadinessIssueDto>();
        void Add(string code, string message) => issues.Add(new(code, message));

        if (!await Db.Set<UnitOfMeasure>().AsNoTracking().AnyAsync(x => x.Symbol == "EA", ct))
            Add("MISSING_INVENTORY_UNIT", "Configure the EA inventory unit.");
        if (!await Db.Set<InventSite>().AsNoTracking().AnyAsync(x => x.DataAreaId == companyId, ct))
            Add("MISSING_SITE", "Configure at least one inventory site.");
        if (!await Db.Set<InventLocation>().AsNoTracking().AnyAsync(x => x.DataAreaId == companyId, ct))
            Add("MISSING_WAREHOUSE", "Configure at least one warehouse under a site.");
        var storageGroup = await Db.Set<EcoResStorageDimensionGroup>().AsNoTracking()
            .FirstOrDefaultAsync(x => x.Name == "SITE-WH", ct);
        if (storageGroup is null || !await Db.Set<EcoResStorageDimensionGroupFldSetup>().AsNoTracking()
                .AnyAsync(x => x.StorageDimensionGroup == storageGroup.RecId && x.DimensionFieldId == InventDimensionFields.Site
                    && x.IsActive && x.IsPhysicalInventoryEnabled && x.IsFinancialInventoryEnabled && !x.IsAllowBlankReceiptEnabled && !x.IsAllowBlankIssueEnabled, ct)
            || !await Db.Set<EcoResStorageDimensionGroupFldSetup>().AsNoTracking()
                .AnyAsync(x => x.StorageDimensionGroup == storageGroup.RecId && x.DimensionFieldId == InventDimensionFields.Warehouse
                    && x.IsActive && x.IsPhysicalInventoryEnabled && !x.IsFinancialInventoryEnabled && !x.IsAllowBlankReceiptEnabled && !x.IsAllowBlankIssueEnabled, ct))
            Add("MISSING_BASIC_STORAGE_GROUP", "Configure SITE-WH with required site and warehouse, with site as the financial dimension.");
        var trackingGroup = await Db.Set<EcoResTrackingDimensionGroup>().AsNoTracking()
            .FirstOrDefaultAsync(x => x.Name == "NONE", ct);
        if (trackingGroup is null || await Db.Set<EcoResTrackingDimensionGroupFldSetup>().AsNoTracking()
                .AnyAsync(x => x.TrackingDimensionGroup == trackingGroup.RecId && x.IsActive, ct))
            Add("MISSING_BASIC_TRACKING_GROUP", "Configure NONE without active batch or serial tracking.");
        if (!await Db.Set<InventItemGroup>().AsNoTracking().AnyAsync(x => x.DataAreaId == companyId && x.ItemGroupId == "TRADE", ct))
            Add("MISSING_ITEM_GROUP", "Configure the TRADE item group.");
        if (!await Db.Set<IAX.IXApi.Modules.Finance.Entities.InventModelGroup>().AsNoTracking().AnyAsync(x => x.DataAreaId == companyId && x.ModelGroupId == "FIFO-BASIC"
                && x.InventModel == InventModel.FIFO && x.StockedProduct == NoYes.Yes && x.NegativePhysical == NoYes.No
                && x.NegativeFinancial == NoYes.No && x.InclPhysicalValueInCost == NoYes.No && x.PostOnHandFinancial == NoYes.Yes, ct))
            Add("MISSING_FIFO_MODEL_GROUP", "Configure FIFO-BASIC with stocked products, financial posting, and negative inventory disabled.");

        var readyForItemSetup = issues.Count == 0;
        var ledger = await Db.Set<Ledger>().AsNoTracking().FirstOrDefaultAsync(x => x.DataAreaId == companyId, ct);
        if (ledger is null || string.IsNullOrWhiteSpace(ledger.AccountingCurrency))
            Add("MISSING_LEDGER", "Configure the company ledger and accounting currency.");
        var parameters = await Db.Set<InventParameters>().AsNoTracking().FirstOrDefaultAsync(x => x.DataAreaId == companyId, ct);
        if (parameters is null || parameters.ReserveOnOrdered == NoYes.Yes ||
            string.IsNullOrWhiteSpace(parameters.MovementJournalNameId) || string.IsNullOrWhiteSpace(parameters.LossProfitJournalNameId) ||
            string.IsNullOrWhiteSpace(parameters.TransferJournalNameId) || string.IsNullOrWhiteSpace(parameters.CountJournalNameId))
            Add("MISSING_INVENTORY_PARAMETERS", "Set all basic journal defaults and disable ordered reservation.");
        var journalNames = await Db.Set<InventJournalName>().AsNoTracking().Where(x => x.DataAreaId == companyId).ToListAsync(ct);
        var sequences = await Db.Set<SysNumberSequence>().IgnoreQueryFilters().AsNoTracking()
            .Where(x => x.DataAreaId == companyId || x.DataAreaId == string.Empty && x.NumberSequenceScope == null)
            .ToListAsync(ct);
        var usableSequenceIds = sequences.Where(InventoryNumberSequencePolicy.CanAllocate)
            .Select(x => x.RecId).ToHashSet();
        foreach (var (type, selectedName) in new[]
        {
            (InventJournalType.Movement, parameters?.MovementJournalNameId),
            (InventJournalType.Adjustment, parameters?.LossProfitJournalNameId),
            (InventJournalType.Transfer, parameters?.TransferJournalNameId),
            (InventJournalType.Counting, parameters?.CountJournalNameId)
        })
            if (!journalNames.Any(x => x.JournalType == type && x.JournalNameId == selectedName &&
                    usableSequenceIds.Contains(x.VoucherNumberSequenceTable)))
                Add($"MISSING_{type.ToString().ToUpperInvariant()}_JOURNAL",
                    $"Select a {type} journal name with an allocatable voucher sequence.");
        foreach (var code in new[] { nameof(InventJournalTable), "InventTransId", "InventDim" })
        {
            var selectedSequence = sequences.Where(x => x.NumberSequence == code && !x.IsDeleted)
                .OrderByDescending(x => x.DataAreaId == companyId).FirstOrDefault();
            if (selectedSequence is null || !InventoryNumberSequencePolicy.CanAllocate(selectedSequence))
                Add("MISSING_NUMBER_SEQUENCE", $"Configure an active {code} number sequence.");
        }
        var postingTypes = await Db.Set<InventPosting>().AsNoTracking().Where(x => x.DataAreaId == companyId && x.LedgerDimension != 0
                && x.CustVendCode == TableGroupAll.All && (x.ItemCode == TableGroupAll.All
                    || x.ItemCode == TableGroupAll.Group && x.ItemRelation == "TRADE"))
            .Select(x => x.InventAccountType).Distinct().ToListAsync(ct);
        foreach (var type in new[] { InventAccountType.InventReceipt, InventAccountType.InventIssue, InventAccountType.InventProfit, InventAccountType.InventLoss })
            if (!postingTypes.Contains(type))
                Add($"MISSING_{type.ToString().ToUpperInvariant()}_ACCOUNT", $"Configure an inventory posting account for {type} that applies to item group TRADE.");
        return new InventoryReadinessDto(readyForItemSetup, issues.Count == 0, issues);
    }
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
        var sequences = await Db.Set<SysNumberSequence>().IgnoreQueryFilters().AsNoTracking()
            .Where(x => !x.IsDeleted && x.IsActive && x.InUse != 0 &&
                (x.DataAreaId == companyId || (x.DataAreaId == string.Empty && x.NumberSequenceScope == null)))
            .OrderByDescending(x => x.DataAreaId == companyId).ThenBy(x => x.NumberSequence).ToListAsync(ct);
        return new InventParametersLookupsDto
        {
            Units = await Db.Set<UnitOfMeasure>().AsNoTracking().OrderBy(x => x.Symbol).Select(x => new InventParametersLookupDto(x.Symbol, x.Symbol)).ToListAsync(ct),
            Sites = await Db.Set<InventSite>().AsNoTracking().Where(x => x.DataAreaId == companyId).OrderBy(x => x.SiteId).Select(x => new InventParametersLookupDto(x.SiteId, x.Name)).ToListAsync(ct),
            Warehouses = await Db.Set<InventLocation>().AsNoTracking().Where(x => x.DataAreaId == companyId).OrderBy(x => x.InventLocationId).Select(x => new InventParametersWarehouseLookupDto(x.InventLocationId, x.Name, x.InventSiteId)).ToListAsync(ct),
            JournalNames = await Db.Set<InventJournalName>().AsNoTracking().Where(x => x.DataAreaId == companyId).OrderBy(x => x.JournalNameId).Select(x => new InventParametersJournalLookupDto(x.JournalNameId, x.Description, (int)x.JournalType)).ToListAsync(ct),
            NumberSequences = sequences.Select(x => new InventParametersNumberSequenceLookupDto(x.RecId, x.NumberSequence, x.Txt)).ToList(),
        };
    }
    public async Task<(InventParametersDto? Value, string? Error)> SaveAsync(InventParametersDto input, CancellationToken ct = default)
    {
        var companyId = company.GetDataAreaId(); if (string.IsNullOrWhiteSpace(companyId)) return (null, "A company must be selected.");
        if (input.ReserveOnOrdered) return (null, "Reservation against ordered supply is disabled in the basic inventory scope.");
        input.DefaultUnitId = input.DefaultUnitId.Trim(); input.MultiSiteDefaultInventSiteId = input.MultiSiteDefaultInventSiteId.Trim().ToUpperInvariant(); input.MultiSiteDefaultInventLocationId = input.MultiSiteDefaultInventLocationId.Trim().ToUpperInvariant();
        input.MovementJournalNameId = input.MovementJournalNameId.Trim().ToUpperInvariant();
        input.LossProfitJournalNameId = input.LossProfitJournalNameId.Trim().ToUpperInvariant();
        input.TransferJournalNameId = input.TransferJournalNameId.Trim().ToUpperInvariant();
        input.CountJournalNameId = input.CountJournalNameId.Trim().ToUpperInvariant();
        if (input.DefaultUnitId.Length > 0 && !await Db.Set<UnitOfMeasure>().AnyAsync(x => x.Symbol == input.DefaultUnitId, ct)) return (null, "Default unit was not found.");
        if (input.MultiSiteDefaultInventSiteId.Length > 0 && !await Db.Set<InventSite>().AnyAsync(x => x.DataAreaId == companyId && x.SiteId == input.MultiSiteDefaultInventSiteId, ct)) return (null, "Default site was not found in the selected company.");
        if (input.MultiSiteDefaultInventLocationId.Length > 0 && !await Db.Set<InventLocation>().AnyAsync(x => x.DataAreaId == companyId && x.InventLocationId == input.MultiSiteDefaultInventLocationId && x.InventSiteId == input.MultiSiteDefaultInventSiteId, ct)) return (null, "Default warehouse must belong to the default site in the selected company.");
        var journals = new[] { (input.MovementJournalNameId, InventJournalType.Movement), (input.LossProfitJournalNameId, InventJournalType.Adjustment), (input.TransferJournalNameId, InventJournalType.Transfer), (input.CountJournalNameId, InventJournalType.Counting) };
        foreach (var (id, type) in journals.Where(x => !string.IsNullOrWhiteSpace(x.Item1)))
            if (!await Db.Set<InventJournalName>().AnyAsync(x => x.DataAreaId == companyId && x.JournalNameId == id && x.JournalType == type, ct)) return (null, $"Journal name '{id}' does not exist for {type} in the selected company.");
        var row = await Db.Set<InventParameters>().SingleOrDefaultAsync(x => x.DataAreaId == companyId, ct) ?? new InventParameters { DataAreaId = companyId };
        if (row.RecId == 0) Db.Add(row);
        row.DefaultUnitId = input.DefaultUnitId; row.MultiSiteDefaultInventSiteId = input.MultiSiteDefaultInventSiteId; row.MultiSiteDefaultInventLocationId = input.MultiSiteDefaultInventLocationId;
        row.MovementJournalNameId = input.MovementJournalNameId; row.LossProfitJournalNameId = input.LossProfitJournalNameId; row.TransferJournalNameId = input.TransferJournalNameId; row.CountJournalNameId = input.CountJournalNameId;
        row.ReserveOnOrdered = input.ReserveOnOrdered ? NoYes.Yes : NoYes.No; row.MandatoryItemTaxGroup = input.MandatoryItemTaxGroup ? NoYes.Yes : NoYes.No;
        await unitOfWork.CompleteAsync(ct); return (Map(row), null);
    }
    private static InventParametersDto Map(InventParameters x) => new() { RecId = x.RecId, DefaultUnitId = x.DefaultUnitId, MultiSiteDefaultInventSiteId = x.MultiSiteDefaultInventSiteId, MultiSiteDefaultInventLocationId = x.MultiSiteDefaultInventLocationId, MovementJournalNameId = x.MovementJournalNameId, LossProfitJournalNameId = x.LossProfitJournalNameId, TransferJournalNameId = x.TransferJournalNameId, CountJournalNameId = x.CountJournalNameId, ReserveOnOrdered = x.ReserveOnOrdered == NoYes.Yes, MandatoryItemTaxGroup = x.MandatoryItemTaxGroup == NoYes.Yes };
}
