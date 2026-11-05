using IAX.IXApi.Infrastructure.Persistence.SeedingERPV2.Core.Contracts;
using IAX.IXApi.Infrastructure.Persistence.SeedingERPV2.Core.Models;
using IAX.IXApi.Modules.Finance.Entities;
using IAX.IXApi.Modules.Finance.Common;
using IAX.IXApi.Modules.Finance.Foundation.Dimensions.AccountStructure;
using IAX.IXApi.Modules.Finance.Foundation.Dimensions.LedgerDimension;
using Microsoft.EntityFrameworkCore;
namespace IAX.IXApi.Infrastructure.Persistence.SeedingERPV2.Modules.Foundation;
public sealed class FoundationDimensionSeeder : IDataSeeder
{
    public string Id => "ERPV2.Foundation.DimensionSeeder";
    public IReadOnlyCollection<string> Dependencies => ["ERPV2.DimensionAttributeValueCombinationSeeder", "ERPV2.Foundation.OrganizationUnitSeeder"];
    public async Task SeedAsync(SeedContext c, CancellationToken ct)
    {
        var unit = await c.RequireAsync<IAX.IXApi.Modules.Finance.Foundation.OrganizationUnits.OrganizationUnit>(x => x.Code == "FNV2-FIN", ct);
        var attribute = await c.EnsureAsync<DimensionAttribute>(x => x.Name == "FNV2Department", new() { Name = "FNV2Department", Type = DimensionAttributeType.Custom }, ct);
        var value = await c.EnsureAsync<DimensionAttributeValue>(x => x.DimensionAttribute == attribute.RecId && x.DisplayValue == "FNV2-FIN",
            new() { DimensionAttribute = attribute.RecId, DisplayValue = "FNV2-FIN", EntityInstance = unit.RecId, BackingRecordDataAreaId = c.Company, ActiveFrom = new(2000,1,1), ActiveTo = new(2099,12,31) }, ct);
        var set = await c.EnsureAsync<DimensionAttributeSet>(x => x.BaseEnumTypeName == "FNV2", new() { BaseEnumTypeName = "FNV2" }, ct);
        await c.EnsureAsync<DimensionAttributeSetItem>(x => x.DimensionAttributeSet == set.RecId && x.DimensionAttribute == attribute.RecId,
            new() { DimensionAttributeSet = set.RecId, DimensionAttribute = attribute.RecId }, ct);
        var values = await c.EnsureAsync<DimensionAttributeValueSet>(x => x.DepartmentValue == "FNV2-FIN", new() { Department = value.RecId, DepartmentValue = "FNV2-FIN" }, ct);
        await c.EnsureAsync<DimensionAttributeValueSetItem>(x => x.DimensionAttributeValueSet == values.RecId && x.DimensionAttributeValue == value.RecId,
            new() { DimensionAttributeValueSet = values.RecId, DimensionAttributeValue = value.RecId, DisplayValue = value.DisplayValue }, ct);
        var chart = await c.RequireAsync<LedgerChartOfAccounts>(x => x.Name == "APV2", ct);
        var account = await c.EnsureAsync<MainAccount>(x => x.MainAccountId == "681099" && x.LedgerChartOfAccounts == chart.RecId,
            new() { MainAccountId = "681099", Name = "Foundation dimension example expense", Type = MainAccountType.Expense, LedgerChartOfAccounts = chart.RecId }, ct);
        var builder = new AccountStructureBuilder(c.Db);
        var structure = await c.Rows<DimensionHierarchy>().SingleOrDefaultAsync(x => x.Name == "FNV2", ct);
        if (structure == null)
        {
            var (defined, error) = await builder.DefineAsync(c.Company, new AccountStructureCreateDto("FNV2", "Foundation dimension example",
                [new AccountStructureSegmentDto("MainAccount", "681099"), new AccountStructureSegmentDto(attribute.Name, value.DisplayValue)]), ct);
            if (defined == null) throw new InvalidOperationException(error);
            var (active, activationError) = await builder.ActivateAsync(c.Company, "FNV2", ct);
            structure = active ?? throw new InvalidOperationException(activationError);
        }
        if (structure.IsDeleted || structure.IsDraft == NoYes.Yes) throw new InvalidOperationException("Foundation account structure is unavailable.");
        var combination = await new LedgerDimensionResolver(c.Db).ResolveAsync(c.Company, account, [(attribute.Name, value.DisplayValue)], structure.RecId, ct);
        if (combination.IsDeleted || combination.AccountStructure != structure.RecId || combination.MainAccount != account.RecId)
            throw new InvalidOperationException("Foundation ledger dimension conflicts.");
    }
}
