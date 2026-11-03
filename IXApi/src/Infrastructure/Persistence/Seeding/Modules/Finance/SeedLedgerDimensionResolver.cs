using IAX.IXApi.Modules.Finance.Common;
using IAX.IXApi.Modules.Finance.Entities;
using IAX.IXApi.Modules.Finance.Foundation.Dimensions.LedgerDimension;
using Microsoft.EntityFrameworkCore;

namespace IAX.IXApi.Infrastructure.Persistence.Seeding.Modules.Finance;

/// <summary>Resolve example account combinations against the existing BS/PL lab structures.</summary>
internal static class SeedLedgerDimensionResolver
{
    internal static async Task<DimensionAttributeValueCombination> ResolveAsync(DbContext db, MainAccount account, CancellationToken ct)
    {
        var profitAndLoss = account.Type is MainAccountType.Revenue or MainAccountType.Expense;
        var structureName = profitAndLoss ? "PL" : "BS";
        var structure = await db.Set<DimensionHierarchy>().IgnoreQueryFilters().SingleAsync(x =>
            x.DataAreaId == account.DataAreaId && x.Name == structureName && !x.IsDeleted && x.IsDraft == NoYes.No, ct);
        IReadOnlyList<(string Dimension, string Value)> values = profitAndLoss ? [("Department", "ADMIN")] : [];
        var result = await new LedgerDimensionResolver(db).ResolveAsync(account.DataAreaId, account, values, structure.RecId, ct);
        if (result.IsDeleted || result.MainAccount != account.RecId || (profitAndLoss && result.DepartmentValue != "ADMIN"))
            throw new InvalidOperationException($"The example combination for '{account.MainAccountId}' conflicts with existing data; it was preserved.");
        return result;
    }
}
