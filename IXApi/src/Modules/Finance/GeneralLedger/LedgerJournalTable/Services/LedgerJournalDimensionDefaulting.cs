using IAX.IXApi.Modules.Finance.Entities;
using IAX.IXApi.Modules.Finance.Foundation.Dimensions.AccountStructure;
using IAX.IXApi.Modules.Finance.Foundation.Dimensions.LedgerDimension;
using Microsoft.EntityFrameworkCore;

namespace IAX.IXApi.Modules.Finance.GeneralLedger.LedgerJournalTable;

internal sealed class LedgerJournalDimensionDefaulting(DbContext db)
{
    public async Task<DimensionAttributeValueCombination> ApplyAsync(string area,
        DimensionAttributeValueCombination entered, CancellationToken ct, params long[] defaultSets)
    {
        if (db.Model.FindEntityType(typeof(DimensionConstraintTree)) == null) return entered;
        var validator = new AccountStructureValidator(db);
        var structure = await validator.ResolveAsync(area, entered, ct);
        var values = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);

        foreach (var set in defaultSets.Where(x => x > 0))
            foreach (var pair in await SetValuesAsync(area, set, ct))
                values.TryAdd(pair.Key, pair.Value);

        foreach (var pair in await validator.SegmentValuesAsync(area, entered, ct))
            values[pair.Key] = pair.Value;

        var account = await db.Set<MainAccount>().AsNoTracking().SingleAsync(x => x.DataAreaId == area
            && (entered.MainAccount > 0 ? x.RecId == entered.MainAccount : x.MainAccountId == entered.MainAccountValue), ct);
        var company = await db.Set<CompanyInfo>().AsNoTracking().Where(x => x.DataAreaId == area)
            .Select(x => x.RecId).FirstOrDefaultAsync(ct);
        var legal = company == 0 ? null : await db.Set<MainAccountLegalEntity>().AsNoTracking()
            .SingleOrDefaultAsync(x => x.LegalEntity == company && x.MainAccount == account.RecId, ct);
        if (legal?.DefaultDimension > 0)
            foreach (var pair in await SetValuesAsync(area, legal.DefaultDimension, ct)) values.TryAdd(pair.Key, pair.Value);
        if (legal?.FixedDimensions > 0)
            foreach (var pair in await SetValuesAsync(area, legal.FixedDimensions, ct)) values[pair.Key] = pair.Value;

        return await new LedgerDimensionResolver(db).ResolveAsync(area, account,
            structure.Dimensions.Select(x => (x, values.GetValueOrDefault(x, string.Empty))).ToList(), structure.RecId, ct);
    }

    private async Task<Dictionary<string, string>> SetValuesAsync(string area, long set, CancellationToken ct) =>
        await (from item in db.Set<DimensionAttributeValueSetItem>().AsNoTracking()
               join value in db.Set<DimensionAttributeValue>().AsNoTracking() on item.DimensionAttributeValue equals value.RecId
               join attribute in db.Set<DimensionAttribute>().AsNoTracking() on value.DimensionAttribute equals attribute.RecId
               where item.DataAreaId == area && item.DimensionAttributeValueSet == set
               select new { attribute.Name, item.DisplayValue }).ToDictionaryAsync(x => x.Name, x => x.DisplayValue, StringComparer.OrdinalIgnoreCase, ct);
}
