using IAX.IXApi.Infrastructure.Persistence.SeedingERPV2.Core.Models;
using IAX.IXApi.Modules.Finance.Common;
using IAX.IXApi.Modules.Finance.Entities;
using IAX.IXApi.Modules.Finance.Foundation.Dimensions.LedgerDimension;
using Microsoft.EntityFrameworkCore;

namespace IAX.IXApi.Infrastructure.Persistence.SeedingERPV2.Modules.GeneralLedger;

/// <summary>
/// The ledger account a baseline posting profile points at. Profit and loss accounts fall under the PL structure,
/// where Department is mandatory, so they resolve with the ADMIN department; balance sheet accounts resolve
/// under BS without one. Resolution goes through <see cref="LedgerDimensionResolver"/>, so an existing
/// combination is reused, never duplicated.
/// </summary>
public static class BaselineLedgerDimension
{
    public const string DefaultDepartment = "ADMIN";

    public static async Task<long?> ResolveAsync(SeedContext c, string mainAccountId, CancellationToken ct)
    {
        var account = await c.Rows<MainAccount>().FirstOrDefaultAsync(x => x.MainAccountId == mainAccountId, ct);
        return account == null ? null : (await ResolveAsync(c, account, ct)).RecId;
    }

    public static async Task<DimensionAttributeValueCombination> ResolveAsync(SeedContext c, MainAccount account, CancellationToken ct)
    {
        var profitAndLoss = account.Type is MainAccountType.Revenue or MainAccountType.Expense;
        var structure = await c.Rows<DimensionHierarchy>().SingleAsync(x =>
            x.Name == (profitAndLoss ? "PL" : "BS") && !x.IsDeleted && x.IsDraft == NoYes.No, ct);
        IReadOnlyList<(string Dimension, string Value)> values = profitAndLoss ? [("Department", DefaultDepartment)] : [];
        var result = await new LedgerDimensionResolver(c.Db).ResolveAsync(account.DataAreaId, account, values, structure.RecId, ct);
        if (result.IsDeleted || result.MainAccount != account.RecId || (profitAndLoss && result.DepartmentValue != DefaultDepartment))
            throw new InvalidOperationException($"The ledger account for '{account.MainAccountId}' conflicts with existing data; it was preserved.");
        return result;
    }

    /// <summary>
    /// True when a profile's current account is unusable: missing, or a profit and loss account without the
    /// department its structure makes mandatory. Only then may a baseline seeder repoint an existing profile.
    /// </summary>
    public static async Task<bool> NeedsDepartmentAsync(SeedContext c, long ledgerDimension, CancellationToken ct)
    {
        var current = await c.Db.Set<DimensionAttributeValueCombination>().IgnoreQueryFilters()
            .FirstOrDefaultAsync(x => x.RecId == ledgerDimension, ct);
        return current == null || (current.MainAccountValue.Length > 0 && current.DepartmentValue.Length == 0
            && current.MainAccountValue[0] is '4' or '5' or '6' or '7' or '8');
    }
}
