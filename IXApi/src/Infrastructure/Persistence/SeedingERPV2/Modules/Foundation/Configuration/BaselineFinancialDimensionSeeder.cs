using IAX.IXApi.Infrastructure.Persistence.SeedingERPV2.Core.Contracts;
using IAX.IXApi.Infrastructure.Persistence.SeedingERPV2.Core.Models;
using IAX.IXApi.Modules.Finance.Common;
using IAX.IXApi.Modules.Finance.Entities;

namespace IAX.IXApi.Infrastructure.Persistence.SeedingERPV2.Modules.Foundation;

/// <summary>The custom financial dimensions the account structures constrain (Department: ADMIN, SALES, SERVICE).</summary>
public sealed class BaselineFinancialDimensionSeeder : IBaselineSeeder
{
    public const string SeederId = "Finance.Foundation.FinancialDimensions";
    public string Id => SeederId;
    public IReadOnlyCollection<string> Dependencies => [BaselineCompanySeeder.SeederId];

    public async Task SeedAsync(SeedContext c, CancellationToken ct)
    {
        foreach (var dimension in FoundationBaselineCatalog.Load().Dimensions)
        {
            var attribute = await c.AddMissingAsync<DimensionAttribute>(x => x.Name == dimension.Name,
                new() { Name = dimension.Name, Type = DimensionAttributeType.Custom, HashKey = Guid.NewGuid() }, ct);
            foreach (var value in dimension.Values)
                await c.AddMissingAsync<DimensionAttributeValue>(x => x.DimensionAttribute == attribute.RecId && x.DisplayValue == value,
                    new() { DimensionAttribute = attribute.RecId, DisplayValue = value, HashKey = Guid.NewGuid() }, ct);
        }
    }
}
