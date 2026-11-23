using IAX.IXApi.Infrastructure.Persistence.SeedingERPV2.Core.Contracts;
using IAX.IXApi.Infrastructure.Persistence.SeedingERPV2.Core.Models;
using IAX.IXApi.Infrastructure.Persistence.SeedingERPV2.Modules.GeneralLedger;
using IAX.IXApi.Modules.Administration.NumberSequences;
using IAX.IXApi.Modules.Finance.Common;
using IAX.IXApi.Modules.Finance.Entities;
using IAX.IXApi.Modules.Finance.Inventory.DimensionGroups.Entities;
using Microsoft.EntityFrameworkCore;

namespace IAX.IXApi.Infrastructure.Persistence.SeedingERPV2.Modules.Inventory;

/// <summary>
/// Item model groups, storage and tracking dimension groups, inventory journal names, inventory parameters and the
/// company-wide inventory posting profile. Two settings are converged because posting depends on them: a journal
/// name without a voucher sequence gets the shared one, and reservation on ordered quantities is kept off. A
/// posting profile is repointed only when its account cannot post (see <see cref="BaselineLedgerDimension.NeedsDepartmentAsync"/>).
/// </summary>
public sealed class BaselineInventorySetupSeeder : IBaselineSeeder
{
    public const string SeederId = "Finance.Inventory.Setup";
    public string Id => SeederId;
    public IReadOnlyCollection<string> Dependencies =>
    [
        BaselineUnitOfMeasureSeeder.SeederId, BaselineSiteWarehouseSeeder.SeederId, BaselineItemGroupSeeder.SeederId,
        BaselineMainAccountSeeder.SeederId, BaselineAccountStructureSeeder.SeederId,
        // The journal names draw their vouchers from the shared Voucher sequence.
        Core.Registration.BaselineSeedingRegistration.NumberSequences
    ];

    public async Task SeedAsync(SeedContext c, CancellationToken ct)
    {
        var catalog = InventoryBaselineCatalog.Load();
        foreach (var group in catalog.ModelGroups)
            await c.AddMissingAsync<InventModelGroup>(x => x.ModelGroupId == group.ModelGroupId, new()
            {
                ModelGroupId = group.ModelGroupId, Name = group.Name, InventModel = group.Model,
                StockedProduct = YesNo(group.Stocked), PostOnHandPhysical = YesNo(group.Stocked), PostOnHandFinancial = YesNo(group.Stocked),
                MandatoryReceive = NoYes.No, NegativePhysical = NoYes.No, NegativeFinancial = NoYes.No, PdsDispositionCode = string.Empty
            }, ct);

        foreach (var group in catalog.StorageGroups)
        {
            var row = await c.AddMissingAsync<EcoResStorageDimensionGroup>(x => x.Name == group.Name,
                new() { Name = group.Name, Description = group.Description }, ct);
            if (!await c.Rows<EcoResStorageDimensionGroupFldSetup>().AnyAsync(x => x.StorageDimensionGroup == row.RecId, ct))
            {
                c.Db.AddRange(
                    Storage(row.RecId, InventDimensionFields.Site, 1, true), Storage(row.RecId, InventDimensionFields.Warehouse, 2, true),
                    Storage(row.RecId, InventDimensionFields.Location, 3, group.Location));
                await c.Db.SaveChangesAsync(ct);
            }
        }
        foreach (var group in catalog.TrackingGroups)
        {
            var row = await c.AddMissingAsync<EcoResTrackingDimensionGroup>(x => x.Name == group.Name,
                new() { Name = group.Name, Description = group.Description }, ct);
            if (!await c.Rows<EcoResTrackingDimensionGroupFldSetup>().AnyAsync(x => x.TrackingDimensionGroup == row.RecId, ct))
            {
                c.Db.AddRange(
                    Tracking(row.RecId, InventDimensionFields.BatchNumber, 1, group.Batch), Tracking(row.RecId, InventDimensionFields.SerialNumber, 2, group.Serial));
                await c.Db.SaveChangesAsync(ct);
            }
        }

        var voucher = await c.Db.Set<SysNumberSequence>().IgnoreQueryFilters()
            .FirstOrDefaultAsync(x => x.NumberSequence == catalog.VoucherSequence && !x.IsDeleted, ct);
        foreach (var journal in catalog.JournalNames)
        {
            var row = await c.AddMissingAsync<InventJournalName>(x => x.JournalNameId == journal.JournalNameId, new()
            {
                JournalNameId = journal.JournalNameId, Description = journal.Description, JournalType = journal.Type,
                VoucherChange = JournalVoucherChange.Change, VoucherDraw = JournalVoucherDraw.Draw, VoucherNumberSequenceTable = voucher?.RecId ?? 0
            }, ct);
            if (row.VoucherNumberSequenceTable == 0 && voucher != null) row.VoucherNumberSequenceTable = voucher.RecId;
        }

        var p = catalog.Parameters;
        var parameters = await c.AddMissingAsync<InventParameters>(x => true, new()
        {
            DefaultUnitId = p.DefaultUnitId, MultiSiteDefaultInventSiteId = p.DefaultSite, MultiSiteDefaultInventLocationId = p.DefaultWarehouse,
            MovementJournalNameId = p.MovementJournal, LossProfitJournalNameId = p.LossProfitJournal,
            TransferJournalNameId = p.TransferJournal, CountJournalNameId = p.CountJournal,
            ReserveOnOrdered = NoYes.No, MandatoryItemTaxGroup = NoYes.No
        }, ct);
        parameters.ReserveOnOrdered = NoYes.No;
        await c.Db.SaveChangesAsync(ct);

        foreach (var rule in catalog.PostingRules)
        {
            var account = await BaselineLedgerDimension.ResolveAsync(c, rule.MainAccount, ct);
            if (account == null) continue;
            var existing = await c.Rows<InventPosting>().FirstOrDefaultAsync(x => x.InventAccountType == rule.AccountType
                && x.ItemCode == TableGroupAll.All && x.CustVendCode == TableGroupAll.All, ct);
            if (existing == null)
                await c.AddMissingAsync<InventPosting>(x => x.InventAccountType == rule.AccountType && x.ItemCode == TableGroupAll.All
                    && x.CustVendCode == TableGroupAll.All, new()
                {
                    InventAccountType = rule.AccountType, ItemCode = TableGroupAll.All, ItemRelation = string.Empty,
                    CustVendCode = TableGroupAll.All, CustVendRelation = string.Empty, LedgerDimension = account.Value, TaxGroupId = string.Empty
                }, ct);
            else if (existing.LedgerDimension != account.Value && await BaselineLedgerDimension.NeedsDepartmentAsync(c, existing.LedgerDimension, ct))
                existing.LedgerDimension = account.Value;
        }
        await c.Db.SaveChangesAsync(ct);
    }

    private static NoYes YesNo(bool value) => value ? NoYes.Yes : NoYes.No;

    private static EcoResStorageDimensionGroupFldSetup Storage(long group, int field, int line, bool active) =>
        new() { StorageDimensionGroup = group, DimensionFieldId = field, LineNumber = line, IsActive = active };

    private static EcoResTrackingDimensionGroupFldSetup Tracking(long group, int field, int line, bool active) =>
        new() { TrackingDimensionGroup = group, DimensionFieldId = field, LineNumber = line, IsActive = active };
}
