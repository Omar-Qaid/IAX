using IAX.IXApi.Infrastructure.Persistence.SeedingERPV2.Core.Contracts;
using IAX.IXApi.Infrastructure.Persistence.SeedingERPV2.Core.Models;
using IAX.IXApi.Modules.Finance.Entities;
using IAX.IXApi.Modules.Finance.Common;
using IAX.IXApi.Modules.Finance.AccountsPayable;
using IAX.IXApi.Modules.Finance.AccountsReceivable;
using IAX.IXApi.Modules.Administration.NumberSequences;
using IAX.IXApi.Modules.Finance.Inventory.InventTable;
using IAX.IXApi.Modules.Finance.Inventory.ProductCategories.Entities;
using IAX.IXApi.Modules.Finance.Foundation.Dimensions.LedgerDimension;
using IAX.IXApi.Modules.Finance.Foundation.Dimensions.AccountStructure;
using Microsoft.EntityFrameworkCore;
namespace IAX.IXApi.Infrastructure.Persistence.SeedingERPV2.Modules.Inventory;
public sealed class EcoResCategorySeeder : IDataSeeder
{
    public string Id => "ERPV2.EcoResCategorySeeder";
    public IReadOnlyCollection<string> Dependencies => ["ERPV2.EcoResProductSeeder"];
    public async Task SeedAsync(SeedContext c, CancellationToken ct)
    {

        var name = $"APV2-{c.Company}";
        var hierarchy = await c.EnsureAsync<EcoResCategoryHierarchy>(x => x.Name == name, new() { Name = name }, ct);
        var category = await c.EnsureAsync<EcoResCategory>(x => x.Name == name && x.CategoryHierarchy == hierarchy.RecId,
            new() { Name = name, CategoryHierarchy = hierarchy.RecId }, ct);
        var number = $"APV2-{c.Company}-SVC";
        var product = await c.RequireAsync<EcoResProduct>(x => x.DisplayProductNumber == number, ct);
        await c.EnsureAsync<EcoResProductCategory>(x => x.Product == product.RecId && x.Category == category.RecId,
            new() { Product = product.RecId, Category = category.RecId, CategoryHierarchy = hierarchy.RecId }, ct);
    }
}
