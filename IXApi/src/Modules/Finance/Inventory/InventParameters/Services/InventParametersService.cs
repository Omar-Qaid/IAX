using IAX.IXApi.Infrastructure.Identity;
using IAX.IXApi.Infrastructure.Persistence.Repositories;
using IAX.IXApi.Infrastructure.Persistence.Services;
using IAX.IXApi.Modules.Administration.NumberSequences;
using IAX.IXApi.Modules.Finance.Common;
using IAX.IXApi.Modules.Finance.Entities;
using IAX.IXApi.Modules.Finance.Inventory.DimensionGroups.Entities;
using IAX.IXApi.Shared.Application.Identity;
using Microsoft.EntityFrameworkCore;
using InventParametersEntity = IAX.IXApi.Modules.Finance.Entities.InventParameters;

namespace IAX.IXApi.Modules.Finance.Inventory;

public sealed class InventParametersService : BaseService<InventParametersEntity>, IInventParametersService
{
    private readonly ICompanyExecutionContext _company;

    public InventParametersService(IUnitOfWork unitOfWork, ICurrentUserService currentUser, ICompanyExecutionContext company)
        : base(unitOfWork, currentUser)
    {
        _company = company;
    }

    public InventParametersService(IUnitOfWork unitOfWork, ICompanyExecutionContext company)
        : this(unitOfWork, new NullCurrentUserService(company), company)
    {
    }

    private DbContext Db => _unitOfWork.Context;

    public async Task<InventoryReadinessDto?> GetReadinessAsync(CancellationToken ct = default)
    {
        var companyId = _company.GetDataAreaId();
        if (string.IsNullOrWhiteSpace(companyId)) return null;
        var issues = new List<InventoryReadinessIssueDto>();
        void Add(string code, string message) => issues.Add(new(code, message));

        if (!await Db.Set<UnitOfMeasure>().AsNoTracking().AnyAsync(ct))
            Add("MISSING_INVENTORY_UNIT", "Configure an inventory unit of measure.");
        if (!await Db.Set<InventSite>().AsNoTracking().AnyAsync(x => x.DataAreaId == companyId, ct))
            Add("MISSING_SITE", "Configure at least one inventory site.");
        if (!await Db.Set<InventLocation>().AsNoTracking().AnyAsync(x => x.DataAreaId == companyId, ct))
            Add("MISSING_WAREHOUSE", "Configure at least one warehouse under a site.");
        var storageGroups = await Db.Set<EcoResStorageDimensionGroup>().AsNoTracking().Select(x => x.RecId).ToListAsync(ct);
        var storageFields = await Db.Set<EcoResStorageDimensionGroupFldSetup>().AsNoTracking().Where(x => x.IsActive).ToListAsync(ct);
        if (!storageGroups.Any(id => storageFields.Any(x => x.StorageDimensionGroup == id && x.DimensionFieldId == InventDimensionFields.Site
                    && x.IsPhysicalInventoryEnabled && x.IsFinancialInventoryEnabled && !x.IsAllowBlankReceiptEnabled && !x.IsAllowBlankIssueEnabled)
                && storageFields.Any(x => x.StorageDimensionGroup == id && x.DimensionFieldId == InventDimensionFields.Warehouse
                    && x.IsPhysicalInventoryEnabled && !x.IsFinancialInventoryEnabled && !x.IsAllowBlankReceiptEnabled && !x.IsAllowBlankIssueEnabled)))
            Add("MISSING_BASIC_STORAGE_GROUP", "Configure a storage dimension group with required site and warehouse, with site as the financial dimension.");
        var trackingGroups = await Db.Set<EcoResTrackingDimensionGroup>().AsNoTracking().Select(x => x.RecId).ToListAsync(ct);
        var activeTrackingGroups = await Db.Set<EcoResTrackingDimensionGroupFldSetup>().AsNoTracking()
            .Where(x => x.IsActive).Select(x => x.TrackingDimensionGroup).Distinct().ToListAsync(ct);
        if (!trackingGroups.Any(id => !activeTrackingGroups.Contains(id)))
            Add("MISSING_BASIC_TRACKING_GROUP", "Configure a tracking dimension group without active batch or serial tracking.");
        var itemGroupIds = await Db.Set<InventItemGroup>().AsNoTracking().Where(x => x.DataAreaId == companyId)
            .Select(x => x.ItemGroupId).ToListAsync(ct);
        if (itemGroupIds.Count == 0)
            Add("MISSING_ITEM_GROUP", "Configure an item group.");
        if (!await Db.Set<IAX.IXApi.Modules.Finance.Entities.InventModelGroup>().AsNoTracking().AnyAsync(x => x.DataAreaId == companyId
                && (x.InventModel == InventModel.FIFO || x.InventModel == InventModel.LIFO || x.InventModel == InventModel.WeightedAvg)
                && x.StockedProduct == NoYes.Yes && x.NegativePhysical == NoYes.No
                && x.NegativeFinancial == NoYes.No && (x.InclPhysicalValueInCost == NoYes.No || x.InventModel == InventModel.WeightedAvg) && x.PostOnHandFinancial == NoYes.Yes
                && x.MandatoryReceive == NoYes.No, ct))
            Add("MISSING_MODEL_GROUP", "Configure a supported stocked item model group with financial posting and negative inventory disabled.");

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
                    || x.ItemCode == TableGroupAll.Group && itemGroupIds.Contains(x.ItemRelation)))
            .Select(x => x.InventAccountType).Distinct().ToListAsync(ct);
        var requiredPostingTypes = new List<InventAccountType>
        {
            InventAccountType.InventReceipt, InventAccountType.InventIssue, InventAccountType.InventProfit,
            InventAccountType.InventLoss, InventAccountType.SalesConsump, InventAccountType.PurchConsump
        };
        var physicalPosting = await Db.Set<IAX.IXApi.Modules.Finance.Entities.InventModelGroup>().AsNoTracking()
            .Where(x => x.DataAreaId == companyId && x.StockedProduct == NoYes.Yes)
            .Select(x => new { x.PostPhysicalPurchase, x.PostPhysicalRevenue }).ToListAsync(ct);
        if (physicalPosting.Any(x => x.PostPhysicalPurchase == NoYes.Yes))
            requiredPostingTypes.Add(InventAccountType.PurchPackingSlipOffsetAccount);
        if (physicalPosting.Any(x => x.PostPhysicalRevenue == NoYes.Yes))
            requiredPostingTypes.Add(InventAccountType.SalesPackingSlipOffsetAccount);
        foreach (var type in requiredPostingTypes)
            if (!postingTypes.Contains(type))
                Add($"MISSING_{type.ToString().ToUpperInvariant()}_ACCOUNT", $"Configure an inventory posting account for {type} that applies to an item group or all items.");
        return new InventoryReadinessDto(readyForItemSetup, issues.Count == 0, issues);
    }
    public async Task<InventParametersDto?> GetAsync(CancellationToken ct = default)
    {
        var companyId = _company.GetDataAreaId(); if (string.IsNullOrWhiteSpace(companyId)) return null;
        var row = await Db.Set<InventParameters>().AsNoTracking().SingleOrDefaultAsync(x => x.DataAreaId == companyId, ct);
        return row is null ? new InventParametersDto() : Map(row);
    }
    public async Task<InventParametersLookupsDto?> GetLookupsAsync(CancellationToken ct = default)
    {
        var companyId = _company.GetDataAreaId();
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
        var companyId = _company.GetDataAreaId(); if (string.IsNullOrWhiteSpace(companyId)) return (null, "A company must be selected.");
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
        await _unitOfWork.CompleteAsync(ct); return (Map(row), null);
    }
    private static InventParametersDto Map(InventParameters x) => new() { RecId = x.RecId, DefaultUnitId = x.DefaultUnitId, MultiSiteDefaultInventSiteId = x.MultiSiteDefaultInventSiteId, MultiSiteDefaultInventLocationId = x.MultiSiteDefaultInventLocationId, MovementJournalNameId = x.MovementJournalNameId, LossProfitJournalNameId = x.LossProfitJournalNameId, TransferJournalNameId = x.TransferJournalNameId, CountJournalNameId = x.CountJournalNameId, ReserveOnOrdered = x.ReserveOnOrdered == NoYes.Yes, MandatoryItemTaxGroup = x.MandatoryItemTaxGroup == NoYes.Yes };

    private sealed class NullCurrentUserService(ICompanyExecutionContext company) : ICurrentUserService
    {
        public string GetCurrentUserId() => string.Empty;
        public string GetOwnerAccountId() => string.Empty;
        public string GetDataAreaId() => company.GetDataAreaId();
    }
}
