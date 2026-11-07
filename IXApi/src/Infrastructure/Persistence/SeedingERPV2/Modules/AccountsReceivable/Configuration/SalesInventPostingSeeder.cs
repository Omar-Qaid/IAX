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
using IAX.IXApi.Infrastructure.Persistence.SeedingERPV2.Modules.GeneralLedger;
namespace IAX.IXApi.Infrastructure.Persistence.SeedingERPV2.Modules.AccountsReceivable;

/// <summary>
/// Sales inventory posting for the shared demo item group. Delivery posts the cost of delivered
/// units against its deferred offset; the invoice posts the inventory issue, its cost of goods sold
/// and the revenue. The purchase side of the same group stays owned by the inventory seeder.
/// </summary>
public sealed class SalesInventPostingSeeder : IDataSeeder
{
    public string Id => "ERPV2.SalesInventPostingSeeder";
    public IReadOnlyCollection<string> Dependencies => ["ERPV2.SalesAccountStructureSeeder", "ERPV2.DimensionAttributeValueCombinationSeeder", "ERPV2.ItemGroupSeeder"];
    public async Task SeedAsync(SeedContext c, CancellationToken ct)
    {
        foreach (var (type, code) in new[] {
            (InventAccountType.SalesPackingSlip, GeneralLedgerSeedExample.DeliveredNotInvoicedInterim),
            (InventAccountType.SalesPackingSlipOffsetAccount, GeneralLedgerSeedExample.DeliveredNotInvoiced),
            (InventAccountType.SalesIssue, GeneralLedgerSeedExample.Inventory),
            (InventAccountType.SalesConsump, GeneralLedgerSeedExample.CostOfGoodsSold),
            (InventAccountType.SalesRevenue, GeneralLedgerSeedExample.SalesRevenue),
            (InventAccountType.SalesDisc, GeneralLedgerSeedExample.SalesCashDiscount) })
        {
            var dimension = await c.RequireAsync<DimensionAttributeValueCombination>(x => x.MainAccountValue == code, ct);
            await c.EnsureAsync<InventPosting>(x => x.InventAccountType == type && x.ItemCode == TableGroupAll.Group && x.ItemRelation == "APV2" && x.CustVendCode == TableGroupAll.All,
                new() { InventAccountType = type, ItemCode = TableGroupAll.Group, ItemRelation = "APV2", CustVendCode = TableGroupAll.All, LedgerDimension = dimension.RecId }, ct);
        }
    }
}
