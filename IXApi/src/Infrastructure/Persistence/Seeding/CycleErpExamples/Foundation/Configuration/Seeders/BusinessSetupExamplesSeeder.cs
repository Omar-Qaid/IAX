using IAX.IXApi.Infrastructure.Persistence.Seeding.CycleErpExamples.Inventory;
using IAX.IXApi.Infrastructure.Persistence.Seeding.CycleErpExamples.Organization;
using IAX.IXApi.Infrastructure.Persistence.Seeding.CycleErpExamples.Foundation;
using System.Linq.Expressions;
using IAX.IXApi.Infrastructure.Persistence.Seeding.Core;
using IAX.IXApi.Modules.Finance.Common;
using IAX.IXApi.Modules.Finance.Entities;
using IAX.IXApi.Modules.Finance.AccountsReceivable;
using IAX.IXApi.Modules.Finance.Foundation.HcmWorkers;
using IAX.IXApi.Modules.Finance.Inventory.ProductCategories.Entities;
using IAX.IXApi.Modules.Finance.Inventory.InventTable;
using IAX.IXApi.Modules.Finance.Inventory.ProductLifecycle.Entities;
using Microsoft.EntityFrameworkCore;

namespace IAX.IXApi.Infrastructure.Persistence.Seeding.Modules.Finance;

/// <summary>Connected, opt-in master data. Existing business keys and values are preserved.</summary>
public sealed class BusinessSetupExamplesSeeder : BaseModuleSeeder
{
    public override string Id => "Finance.BusinessSetupExamples";
    public override SeederCategory Category => SeederCategory.Demo;
    public override string Scope => "dat";
    public override IReadOnlyCollection<string> DependsOn => new[] { "Finance.OperationalExamples", "Finance.Foundation.Logistics", "Finance.Foundation.HcmExamples", "ERP.MasterSetupExamples" };
    public override Task SeedAsync(SeedingContext context, CancellationToken ct) => SeedDataAsync(context.Db, ct);

    public async Task SeedDataAsync(DbContext db, CancellationToken ct = default)
    {
        var hierarchy = await EcoResCategoryHierarchySeeder.SeedAsync(db, ct);
        var root = await EcoResCategorySeeder.SeedAsync(db, hierarchy.RecId, ct);
        var paper = await EcoResCategorySeeder.SeedVariant2Async(db, hierarchy.RecId, root.RecId, ct);
        var product = await db.Set<EcoResProduct>().IgnoreQueryFilters().SingleAsync(x => x.DisplayProductNumber == "ITM-1002", ct);
        await EcoResProductCategorySeeder.SeedAsync(db, hierarchy.RecId, paper.RecId, product.RecId, ct);
        await EcoResProductLifecycleStateSeeder.SeedAsync(db, ct);
        await EcoResProductLifecycleStateSeeder.SeedVariant2Async(db, ct);
        await InventCountGroupSeeder.SeedAsync(db, ct);
        await InventCountGroupSeeder.SeedVariant2Async(db, ct);
        await InventPackagingGroupSeeder.SeedAsync(db, ct);
        var each = await db.Set<UnitOfMeasure>().IgnoreQueryFilters().SingleAsync(x => x.Symbol == "EA", ct);
        await UnitOfMeasureInternalCodeSeeder.SeedAsync(db, each.RecId, ct);
        await InventPackagingUnitSeeder.SeedAsync(db, ct);
        var dimension = await db.Set<InventDim>().IgnoreQueryFilters().SingleAsync(x => x.DataAreaId == "dat" && x.InventDimId == "ITEM001-JED-1", ct);
        foreach (var (barcode, blocked) in new[] { ("DEMO-CHAIR-001", NoYes.No), ("DEMO-CHAIR-OLD", NoYes.Yes) })
            await InventItemBarcodeSeeder.SeedAsync(db, barcode, blocked, ct);
        await InventItemLocationSeeder.SeedAsync(db, dimension.InventDimId, ct);
        await InventBatchSeeder.SeedAsync(db, ct);
        await InventSerialSeeder.SeedAsync(db, ct);
        await InventItemPriceSeeder.SeedAsync(db, dimension.InventDimId, ct);
        var customer = await db.Set<CustTable>().IgnoreQueryFilters().SingleAsync(x => x.DataAreaId == "dat" && x.AccountNum == "CUST-100", ct);
        var responsibleWorker = await db.Set<HcmWorker>().IgnoreQueryFilters().SingleAsync(x => x.DataAreaId == "dat" && x.PersonnelNumber == "DEMO-SELLER-A", ct);
        var contactParty = await DirPartyTableSeeder.SeedAsync(db, ct);
        await ContactPersonSeeder.SeedAsync(db, contactParty.RecId, customer.Party, customer.AccountNum, responsibleWorker.RecId, ct);

        // Associate each legal entity with its existing postal location; do not replace customer addresses.
        var businessRole = await db.Set<LogisticsLocationRole>().IgnoreQueryFilters().SingleAsync(x => x.Name == "Business", ct);
        foreach (var company in new[] { "dat", "HBMC" })
        {
            var legalEntity = await db.Set<CompanyInfo>().IgnoreQueryFilters().SingleAsync(x => x.DataArea == company, ct);
            var locationCode = company == "dat" ? "DAT-LOC-1" : "HBMC-LOC-1";
            var location = await db.Set<LogisticsLocation>().IgnoreQueryFilters().SingleAsync(x => x.LocationId == locationCode, ct);
            var link = await DirPartyLocationSeeder.SeedAsync(db, legalEntity.Party, location.RecId, company, ct);
            await DirPartyLocationRoleSeeder.SeedAsync(db, link.RecId, businessRole.RecId, company, ct);
        }
    }

}
