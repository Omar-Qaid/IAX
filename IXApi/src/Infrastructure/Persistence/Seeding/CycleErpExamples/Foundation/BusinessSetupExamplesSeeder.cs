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
    public override IReadOnlyCollection<string> DependsOn => new[] { "Finance.OperationalExamples", "Finance.Foundation.Logistics", "Finance.Foundation.HcmExamples" };
    public override Task SeedAsync(SeedingContext context, CancellationToken ct) => SeedDataAsync(context.Db, ct);

    public async Task SeedDataAsync(DbContext db, CancellationToken ct = default)
    {
        var hierarchy = await Ensure(db, x => x.Name == "Example product categories",
            new EcoResCategoryHierarchy { Name = "Example product categories" }, ct);
        var root = await Ensure(db, x => x.CategoryHierarchy == hierarchy.RecId && x.Name == "Office supplies",
            new EcoResCategory { CategoryHierarchy = hierarchy.RecId, Name = "Office supplies" }, ct);
        var paper = await Ensure(db, x => x.CategoryHierarchy == hierarchy.RecId && x.Name == "Paper and stationery",
            new EcoResCategory { CategoryHierarchy = hierarchy.RecId, ParentCategory = root.RecId, Name = "Paper and stationery" }, ct);
        var product = await db.Set<EcoResProduct>().IgnoreQueryFilters().SingleAsync(x => x.DisplayProductNumber == "ITM-1002", ct);
        await Ensure(db, x => x.Product == product.RecId && x.Category == paper.RecId,
            new EcoResProductCategory { Product = product.RecId, Category = paper.RecId, CategoryHierarchy = hierarchy.RecId, DisplayOrder = 1 }, ct);
        await Ensure(db, x => x.StateId == "DEMO-ACTIVE", new EcoResProductLifecycleState {
            StateId = "DEMO-ACTIVE", Description = "Example: available for purchase and sale" }, ct);
        await Ensure(db, x => x.StateId == "DEMO-RETIRED", new EcoResProductLifecycleState {
            StateId = "DEMO-RETIRED", Description = "Example: retired product", BlockPurchase = NoYes.Yes,
            BlockSales = NoYes.Yes, BlockInventory = NoYes.Yes }, ct);
        await Ensure(db, x => x.DataAreaId == "dat" && x.CountGroupId == "DEMO-MONTH",
            new InventCountGroup { CountGroupId = "DEMO-MONTH", Name = "Example monthly count", CountCode = InventCountCode.Period, CountPeriod = 30 }, ct);
        await Ensure(db, x => x.DataAreaId == "dat" && x.CountGroupId == "DEMO-MAN",
            new InventCountGroup { CountGroupId = "DEMO-MAN", Name = "Example manual count", CountCode = InventCountCode.Manual }, ct);
        await Ensure(db, x => x.DataAreaId == "dat" && x.PackagingGroupId == "DEMO-PAPER",
            new InventPackagingGroup { PackagingGroupId = "DEMO-PAPER", Name = "Example paper packaging" }, ct);
        var each = await db.Set<UnitOfMeasure>().IgnoreQueryFilters().SingleAsync(x => x.Symbol == "EA", ct);
        await Ensure(db, x => x.UnitOfMeasure == each.RecId && x.CodeSymbol == InternalCodeSymbol.Symbol,
            new UnitOfMeasureInternalCode { UnitOfMeasure = each.RecId, CodeSymbol = InternalCodeSymbol.Symbol }, ct);
        await Ensure(db, x => x.DataAreaId == "dat" && x.ItemCode == TableGroupAll.All && x.Unit == "EA" && x.InventDimId == "",
            new InventPackagingUnit { ItemCode = TableGroupAll.All, Unit = "EA", Factor = 1 }, ct);
        var dimension = await db.Set<InventDim>().IgnoreQueryFilters().SingleAsync(x => x.DataAreaId == "dat" && x.InventDimId == "ITEM001-JED-1", ct);
        foreach (var (barcode, blocked) in new[] { ("DEMO-CHAIR-001", NoYes.No), ("DEMO-CHAIR-OLD", NoYes.Yes) })
            await Ensure(db, x => x.DataAreaId == "dat" && x.ItemBarcode == barcode,
                new InventItemBarcode { ItemBarcode = barcode, ItemId = "ITEM-001", UnitId = "PCS", Qty = 1,
                    Description = "Example internal chair barcode", UseForInput = NoYes.Yes, Blocked = blocked }, ct);
        await Ensure(db, x => x.DataAreaId == "dat" && x.ItemId == "ITEM-001" && x.InventDimId == dimension.InventDimId,
            new InventItemLocation { ItemId = "ITEM-001", InventDimId = dimension.InventDimId, CountGroupId = "DEMO-MONTH",
                WmsLocationIdDefaultReceipt = "RECEIPT", WmsLocationIdDefaultIssue = "ISSUE", PickingLocationMaxQty = 100, PickingLocationRefillMin = 10 }, ct);
        await Ensure(db, x => x.DataAreaId == "dat" && x.ItemId == "ITM-1002" && x.InventBatchId == "DEMO-LOT-001",
            new InventBatch { ItemId = "ITM-1002", InventBatchId = "DEMO-LOT-001", ProdDate = new DateTime(2026, 1, 1),
                ExpDate = new DateTime(2028, 1, 1), PdsBestBeforeDate = new DateTime(2027, 12, 1) }, ct);
        await Ensure(db, x => x.DataAreaId == "dat" && x.ItemId == "ITEM-001" && x.InventSerialId == "DEMO-SERIAL-001",
            new InventSerial { ItemId = "ITEM-001", InventSerialId = "DEMO-SERIAL-001", ProdDate = new DateTime(2026, 1, 1) }, ct);
        await Ensure(db, x => x.DataAreaId == "dat" && x.ItemId == "ITEM-001" && x.VersionId == "DEMO-PLAN",
            new InventItemPrice { ItemId = "ITEM-001", VersionId = "DEMO-PLAN", InventDimId = dimension.InventDimId,
                CostingType = CostingType.PlannedCost, PriceType = PreferredPriceType.CostPrice, Price = 100, PriceUnit = 1,
                PriceQty = 1, UnitId = "PCS", ActivationDate = new DateTime(2026, 1, 1), LastPriceUniquenessAllowance = new Guid("6f5f577e-d2bd-4d70-b9d9-bc9209101001") }, ct);
        var customer = await db.Set<CustTable>().IgnoreQueryFilters().SingleAsync(x => x.DataAreaId == "dat" && x.AccountNum == "CUST-100", ct);
        var responsibleWorker = await db.Set<HcmWorker>().IgnoreQueryFilters().SingleAsync(x => x.DataAreaId == "dat" && x.PersonnelNumber == "DEMO-SELLER-A", ct);
        var contactParty = await Ensure(db, x => x.PartyNumber == "DEMO-CONTACT-01",
            new DirPartyTable { PartyNumber = "DEMO-CONTACT-01", Name = "Example receiving contact", NameAlias = "جهة اتصال الاستلام التجريبية",
                LanguageId = "ar-sa", IsActive = NoYes.Yes }, ct);
        await Ensure(db, x => x.DataAreaId == "dat" && x.ContactPersonId == "DEMO-CONTACT-01",
            new ContactPerson { ContactPersonId = "DEMO-CONTACT-01", Party = contactParty.RecId, ContactForParty = customer.Party,
                CustAccount = customer.AccountNum, MainResponsibleWorker = responsibleWorker.RecId,
                TimeAvailableFrom = 8 * 3600, TimeAvailableTo = 17 * 3600 }, ct);

        // Associate each legal entity with its existing postal location; do not replace customer addresses.
        var businessRole = await db.Set<LogisticsLocationRole>().IgnoreQueryFilters().SingleAsync(x => x.Name == "Business", ct);
        foreach (var company in new[] { "dat", "HBMC" })
        {
            var legalEntity = await db.Set<CompanyInfo>().IgnoreQueryFilters().SingleAsync(x => x.DataArea == company, ct);
            var locationCode = company == "dat" ? "DAT-LOC-1" : "HBMC-LOC-1";
            var location = await db.Set<LogisticsLocation>().IgnoreQueryFilters().SingleAsync(x => x.LocationId == locationCode, ct);
            var link = await Ensure(db, x => x.Party == legalEntity.Party && x.Location == location.RecId,
                new DirPartyLocation { Party = legalEntity.Party, Location = location.RecId, IsPostalAddress = NoYes.Yes,
                    IsRoleBusiness = NoYes.Yes, AssignmentDate = new DateTime(2026, 1, 1), DataAreaId = company }, ct);
            await Ensure(db, x => x.PartyLocation == link.RecId && x.LocationRole == businessRole.RecId,
                new DirPartyLocationRole { PartyLocation = link.RecId, LocationRole = businessRole.RecId, DataAreaId = company }, ct);
        }
    }

    private static async Task<T> Ensure<T>(DbContext db, Expression<Func<T, bool>> key, T row, CancellationToken ct) where T : class
    {
        var existing = await db.Set<T>().IgnoreQueryFilters().SingleOrDefaultAsync(key, ct);
        if (existing != null) return existing;
        db.Add(row);
        await db.SaveChangesAsync(ct);
        return row;
    }
}
