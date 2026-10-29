using IAX.IXApi.Infrastructure.Persistence.Repositories;
using IAX.IXApi.Infrastructure.Persistence.Seeding.Core;
using IAX.IXApi.Modules.Administration.NumberSequences;
using IAX.IXApi.Modules.Finance.AccountsPayable;
using IAX.IXApi.Modules.Finance.AccountsReceivable;
using IAX.IXApi.Modules.Finance.AccountsReceivable.Confirm;
using IAX.IXApi.Modules.Finance.AccountsReceivable.Invoice;
using IAX.IXApi.Modules.Finance.AccountsReceivable.PackingSlip;
using IAX.IXApi.Modules.Finance.AccountsReceivable.PackingSlip.Dtos;
using IAX.IXApi.Modules.Finance.AccountsReceivable.Return;
using IAX.IXApi.Modules.Finance.AccountsReceivable.SalesOrder.Services;
using IAX.IXApi.Modules.Finance.Common;
using IAX.IXApi.Modules.Finance.Entities;
using IAX.IXApi.Modules.Finance.Inventory;
using IAX.IXApi.Modules.Finance.Inventory.DimensionGroups.Entities;
using IAX.IXApi.Modules.Finance.Inventory.InventTable;
using IAX.IXApi.Modules.Identity.Users;
using Microsoft.EntityFrameworkCore;

namespace IAX.IXApi.Infrastructure.Persistence.Seeding.Modules.Finance;

/// <summary>Dedicated item, strict FIFO policy and service-posted 115-unit worked example.</summary>
public sealed class InventoryFullCycleSeeder : BaseModuleSeeder
{
    private const string Item = "DEMO-FIFO115";
    private const string Group = "DEMOFIFO";
    private const string From = "FIFO-WH1", To = "FIFO-WH2", Transit = "FIFO-TR";
    public override string Id => "Finance.InventoryFullCycle";
    public override SeederCategory Category => SeederCategory.Demo;
    public override SeederExecutionPolicy Policy => SeederExecutionPolicy.RunOnce;
    public override string Scope => "dat";
    public override IReadOnlyCollection<string> DependsOn => new[] { "Finance.SalesAdvancedScenarios" };

    public override async Task SeedAsync(SeedingContext context, CancellationToken ct)
    {
        var actor = new SalesScenarioSeeder.ExampleActor(await context.Db.Set<AspNetUser>().IgnoreQueryFilters()
            .Where(x => x.UserName == "sys").Select(x => x.Id).SingleAsync(ct));
        await using var db = new ApplicationDbContext(new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseSqlServer(context.Db.Database.GetConnectionString()).Options, companyContext: actor);
        var date = DateTime.UtcNow.Date;
        var work = new UnitOfWork(db);
        var numbers = new SysNumberSequenceService(work, actor);
        var inventoryNumbers = new SalesInventoryNumberService(numbers);
        var resolver = new InventDimensionResolver(work, inventoryNumbers);
        var units = new SalesUnitConversionService(work);
        var journals = new InventJournalService(work, actor, actor, numbers, resolver);
        var posting = new InventJournalPostingService(work, actor, actor, numbers, inventoryNumbers);
        await SetupAsync(db, ct);
        var openingDate = await (from h in db.Set<InventJournalTable>() join l in db.Set<InventJournalTrans>()
            on new { h.DataAreaId, h.JournalId } equals new { l.DataAreaId, l.JournalId }
            where h.DataAreaId == "dat" && h.Description == "FIFO115-OPEN" select (DateTime?)l.TransDate).SingleOrDefaultAsync(ct);
        date = openingDate?.Date ?? date;

        async Task Journal(string marker, string name, decimal qty, decimal cost = 0, string? destination = null, decimal? counted = null)
        {
            var header = await db.Set<InventJournalTable>().SingleOrDefaultAsync(x => x.DataAreaId == "dat" && x.Description == marker, ct);
            if (header?.Posted == NoYes.Yes) return;
            if (header == null)
            {
                var created = await journals.CreateAsync(new() { JournalNameId = name, Description = marker, InventSiteId = "JED", InventLocationId = From }, ct);
                Require(created.Error);
                header = await db.Set<InventJournalTable>().SingleAsync(x => x.DataAreaId == "dat" && x.JournalId == created.Journal!.JournalId, ct);
            }
            var line = await db.Set<InventJournalTrans>().SingleOrDefaultAsync(x => x.DataAreaId == "dat" && x.JournalId == header.JournalId, ct);
            if (line == null)
            {
                long offset = name == "IJ-MOV" ? await db.Set<InventPosting>().Where(x => x.DataAreaId == "dat" && x.InventAccountType == InventAccountType.InventLoss
                    && x.ItemCode == TableGroupAll.All && x.CustVendCode == TableGroupAll.All).Select(x => x.LedgerDimension).FirstAsync(ct) : 0;
                Require((await journals.AddLineAsync(header.JournalId, new() { ItemId = Item, Unit = "PCS", Qty = qty, CostPrice = cost, TransDate = date,
                    InventSiteId = "JED", InventLocationId = From, ToInventSiteId = destination == null ? "" : "JED", ToInventLocationId = destination ?? "", LedgerDimension = offset }, ct)).Error);
                line = await db.Set<InventJournalTrans>().SingleAsync(x => x.DataAreaId == "dat" && x.JournalId == header.JournalId, ct);
            }
            if (line.ItemId != Item || line.Unit != "PCS") throw new InvalidOperationException("A customized example journal was preserved.");
            if (counted.HasValue)
            {
                Require(await posting.FreezeCountAsync(header.JournalId, ct));
                Require(await posting.ConfirmCountAsync(header.JournalId, new Dictionary<long, decimal> { [line.RecId] = counted.Value }, ct));
            }
            Require((await posting.PostAsync(header.JournalId, ct)).Error);
        }

        await Journal("FIFO115-OPEN", "IJ-ADJ", 100, 50);
        var purchases = new PurchOrderService(work, actor, actor, numbers, new PurchInventorySupplyService(work, inventoryNumbers, resolver), units);
        var po = await db.Set<PurchTable>().SingleOrDefaultAsync(x => x.DataAreaId == "dat" && x.PurchName == "FIFO115-PURCHASE" && x.ReturnItemNum == "", ct);
        if (po == null)
        {
            var created = await purchases.CreateAsync(new("VEND-100", null, "SAR", date, "JED", From, "FIFO115-PURCHASE"), ct);
            Check(created.StatusCode, created.Message);
            po = (PurchTable)created.Data!;
        }
        var pl = await db.Set<PurchLine>().SingleOrDefaultAsync(x => x.DataAreaId == "dat" && x.PurchId == po.PurchId, ct);
        if (pl == null)
        {
            var added = await purchases.AddLineAsync(po.RecId, new(Item, 50, "PCS", 60, 1, "JED", From, date, "FIFO second cost layer"), ct);
            Check(added.StatusCode, added.Message);
            pl = await db.Set<PurchLine>().SingleAsync(x => x.DataAreaId == "dat" && x.PurchId == po.PurchId, ct);
        }
        if (!await db.Set<VendPackingSlipJour>().AnyAsync(x => x.DataAreaId == "dat" && x.PurchId == po.PurchId, ct))
        {
            var confirmed = await new PurchConfirmationService(work, actor, actor, numbers).ConfirmAsync(po.RecId, ct);
            Check(confirmed.StatusCode, confirmed.Message);
            var received = await new ProductReceiptService(work, actor, actor, numbers).PostAsync(po.RecId, new("FIFO115-PR", date, new() { new(pl.RecId, 50) }), ct);
            Check(received.StatusCode, received.Message);
        }
        if (!await db.Set<VendInvoiceJour>().AnyAsync(x => x.DataAreaId == "dat" && x.PurchId == po.PurchId, ct))
        {
            var invoiced = await new VendorInvoiceService(work, actor, actor, numbers).PostAsync(po.RecId, new("FIFO115-PI", date, new() { new(pl.RecId, 50) }), ct);
            Check(invoiced.StatusCode, invoiced.Message);
        }

        var validation = new SalesOrderValidationService(work, actor);
        var orders = new SalesOrderQuickCreateService(work, actor, numbers, validation);
        var lines = new SalesOrderLineService(work, actor, validation, new SalesInventoryDemandService(work, inventoryNumbers, resolver), units);
        var so = await db.Set<SalesTable>().SingleOrDefaultAsync(x => x.DataAreaId == "dat" && x.CustomerRef == "FIFO115-SALE" && x.SalesType != SalesType.ReturnItem, ct);
        if (so == null)
        {
            Require((await orders.CreateAsync(new() { CustomerAccount = "CUST-100", CustomerReference = "FIFO115-SALE", SalesName = "FIFO115-SALE",
                CurrencyCode = "SAR", InventSiteId = "JED", InventLocationId = From, TaxGroupId = "DOM", RequestedShipDate = date, RequestedReceiptDate = date }, ct)).Error);
            so = await db.Set<SalesTable>().SingleAsync(x => x.DataAreaId == "dat" && x.CustomerRef == "FIFO115-SALE", ct);
        }
        var sl = await db.Set<SalesLine>().SingleOrDefaultAsync(x => x.DataAreaId == "dat" && x.SalesId == so.SalesId, ct);
        if (sl == null)
        {
            var added = await lines.AddAsync(so.RecId, new() { ItemNumber = Item, Quantity = 30, Unit = "PCS", UnitPrice = 100, PriceUnit = 1,
                InventSiteId = "JED", InventLocationId = From, TaxGroup = "DOM", TaxItemGroup = "FULL", DeliveryDate = date }, ct);
            Require(added.Error);
            sl = await db.Set<SalesLine>().SingleAsync(x => x.DataAreaId == "dat" && x.SalesId == so.SalesId, ct);
        }
        if (!await db.Set<CustPackingSlipJour>().AnyAsync(x => x.DataAreaId == "dat" && x.SalesId == so.SalesId, ct))
        {
            var confirmed = await new SalesOrderConfirmationService(work, actor, numbers, actor, new SalesOrderConfirmationValidator()).PostAsync(so.RecId, new(date, true, "FIFO115-CONF"), ct);
            Check(confirmed.StatusCode, confirmed.Message);
            var reservations = new InventoryReservationService(work, actor, actor);
            Require((await reservations.ReserveAsync(sl.InventTransId, 30, false, ct)).Error);
            Require((await reservations.SetPickedAsync(sl.InventTransId, 30, ct)).Error);
            var shipped = await new PackingSlipJournalService(work, actor, actor, numbers).PostAsync(so.RecId,
                new(date, "Picked", true, "FIFO115-SHIP", new[] { new PackingSlipPostingLineRequest(sl.RecId.ToString(), 30) }), ct);
            Check(shipped.StatusCode, shipped.Message);
        }
        if (!await db.Set<CustInvoiceJour>().AnyAsync(x => x.DataAreaId == "dat" && x.SalesId == so.SalesId, ct))
        {
            var invoiced = await new SalesInvoiceService(work, actor, actor, numbers).PostAsync(so.RecId, new(date, "FIFO115-SI"), ct);
            Check(invoiced.StatusCode, invoiced.Message);
        }
        await Journal("FIFO115-TRANSFER", "IJ-TRF", 20, destination: To);
        await Journal("FIFO115-DAMAGE", "IJ-MOV", -5);
        await Journal("FIFO115-COUNT", "IJ-CNT", 0, counted: 98);

        var transfers = new InventTransferOrderService(work, actor, journals, posting);
        Require((await transfers.CreateAsync(new("FIFO115-TRANSIT", Item, "PCS", 40, "JED", From, Transit, To, "IJ-TRF"), ct)).Error);
        Require((await transfers.PostAsync("FIFO115-TRANSIT", new("SHIP", 40, date), false, ct)).Error);
        Require((await transfers.PostAsync("FIFO115-TRANSIT", new("RECEIVE", 40, date), true, ct)).Error);

        var customerReturns = new CustReturnService(work, actor, numbers, inventoryNumbers);
        var returnLine = await db.Set<SalesLine>().SingleOrDefaultAsync(x => x.DataAreaId == "dat" && x.InventTransIdReturn == sl.InventTransId && x.SalesType == SalesType.ReturnItem, ct);
        if (returnLine == null)
        {
            var sold = await db.Set<CustInvoiceTrans>().SingleAsync(x => x.DataAreaId == "dat" && x.SalesId == so.SalesId, ct);
            var created = await customerReturns.CreateAsync(new(sold.InvoiceId, "UNWANTED", new() { new(sold.RecId, 2, "RESTOCK") }), ct);
            Check(created.StatusCode, created.Message);
            returnLine = await db.Set<SalesLine>().SingleAsync(x => x.DataAreaId == "dat" && x.InventTransIdReturn == sl.InventTransId && x.SalesType == SalesType.ReturnItem, ct);
        }
        var returnOrder = await db.Set<SalesTable>().SingleAsync(x => x.DataAreaId == "dat" && x.SalesId == returnLine.SalesId, ct);
        if (!await db.Set<CustInvoiceJour>().AnyAsync(x => x.DataAreaId == "dat" && x.SalesId == returnOrder.SalesId, ct))
        {
            var received = await customerReturns.ReceiveAsync(returnOrder.RecId, new("FIFO115-CR", date, new() { new(returnLine.RecId, 2) }), ct);
            Check(received.StatusCode, received.Message);
            var credited = await customerReturns.CreditAsync(returnOrder.RecId, new(date, "FIFO115-CC"), ct);
            Check(credited.StatusCode, credited.Message);
        }
        var vendorReturns = new PurchReturnService(work, actor, numbers, inventoryNumbers);
        var vr = await db.Set<PurchTable>().SingleOrDefaultAsync(x => x.DataAreaId == "dat" && x.ReturnItemNum == "FIFO115-VR", ct);
        if (vr == null)
        {
            var bought = await db.Set<VendInvoiceTrans>().SingleAsync(x => x.DataAreaId == "dat" && x.PurchId == po.PurchId, ct);
            var created = await vendorReturns.CreateAsync(new(bought.InvoiceId, "FIFO115-VR", new() { new(bought.RecId, 5) }), ct);
            Check(created.StatusCode, created.Message);
            vr = await db.Set<PurchTable>().SingleAsync(x => x.DataAreaId == "dat" && x.ReturnItemNum == "FIFO115-VR", ct);
        }
        if (!await db.Set<VendInvoiceJour>().AnyAsync(x => x.DataAreaId == "dat" && x.InvoiceId == "FIFO115-VC", ct))
        {
            var vl = await db.Set<PurchLine>().SingleAsync(x => x.DataAreaId == "dat" && x.PurchId == vr.PurchId, ct);
            if (!await db.Set<VendPackingSlipJour>().AnyAsync(x => x.DataAreaId == "dat" && x.PurchId == vr.PurchId, ct))
            {
                var shipped = await vendorReturns.ShipAsync(vr.RecId, new("FIFO115-VS", date, new() { new(vl.RecId, 5) }), ct);
                Check(shipped.StatusCode, shipped.Message);
            }
            var credited = await vendorReturns.CreditAsync(vr.RecId, new("FIFO115-VC", date), ct);
            Check(credited.StatusCode, credited.Message);
        }
        var balances = await db.Set<InventSum>().Where(x => x.DataAreaId == "dat" && x.ItemId == Item).ToListAsync(ct);
        if (balances.Sum(x => x.PhysicalInvent) != 115 || balances.Where(x => x.InventLocationId == From).Sum(x => x.PhysicalInvent) != 55
            || balances.Where(x => x.InventLocationId == To).Sum(x => x.PhysicalInvent) != 60 || balances.Where(x => x.InventLocationId == Transit).Sum(x => x.PhysicalInvent) != 0)
            throw new InvalidOperationException("FIFO worked example must end at 55 + 60 + 0 = 115 units.");
        Require((await new InventCloseService(work, actor, actor, numbers).CloseItemAsync(Item, date, ct)).Error);
    }

    private static void Require(string? error) { if (error != null) throw new InvalidOperationException(error); }
    private static void Check(int status, string? message) { if (status >= 400) throw new InvalidOperationException(message); }

    private static async Task SetupAsync(ApplicationDbContext db, CancellationToken ct)
    {
        await using var tx = await db.Database.BeginTransactionAsync(ct);
        if (!await db.Set<InventModelGroup>().AnyAsync(x => x.DataAreaId == "dat" && x.ModelGroupId == Group, ct))
            db.Add(new InventModelGroup { DataAreaId = "dat", ModelGroupId = Group, Name = "Strict FIFO example", InventModel = InventModel.FIFO,
                StockedProduct = NoYes.Yes, PostOnHandPhysical = NoYes.Yes, PostOnHandFinancial = NoYes.Yes, MandatoryReceive = NoYes.Yes,
                NegativePhysical = NoYes.No, NegativeFinancial = NoYes.No });
        foreach (var id in new[] { From, To, Transit })
            if (!await db.Set<InventLocation>().AnyAsync(x => x.DataAreaId == "dat" && x.InventLocationId == id, ct))
                db.Add(new InventLocation { DataAreaId = "dat", InventLocationId = id, InventSiteId = "JED", Name = "FIFO example " + id,
                    InventLocationType = id == Transit ? InventLocationType.Transit : InventLocationType.Standard, DefaultStatusID = "AVAILABLE" });
        if (!await db.Set<IAX.IXApi.Modules.Finance.Entities.InventTable>().AnyAsync(x => x.DataAreaId == "dat" && x.ItemId == Item, ct))
        {
            var product = new EcoResProduct { DisplayProductNumber = Item, SearchName = "FIFO115 example", ProductType = ItemType.Item };
            db.Add(product);
            await db.SaveChangesAsync(ct);
            db.Add(new EcoResDistinctProduct { RecId = product.RecId });
            db.Add(new EcoResProductTranslation { Product = product.RecId, LanguageId = "en-us", Name = "FIFO worked example", Description = "Dedicated 115-unit inventory cycle" });
            db.Add(new IAX.IXApi.Modules.Finance.Entities.InventTable { DataAreaId = "dat", ItemId = Item, Product = product.RecId, Name = "FIFO worked example", BomUnitId = "PCS" });
            var group = await db.Set<InventItemGroupItem>().AsNoTracking().SingleAsync(x => x.ItemDataAreaId == "dat" && x.ItemId == "ITEM-001", ct);
            db.Add(new InventItemGroupItem { DataAreaId = "dat", ItemDataAreaId = "dat", ItemId = Item, ItemGroupId = group.ItemGroupId, ItemGroupDataAreaId = "dat" });
            db.Add(new InventModelGroupItem { DataAreaId = "dat", ItemDataAreaId = "dat", ItemId = Item, ModelGroupId = Group, ModelGroupDataAreaId = "dat" });
            var storage = await db.Set<EcoResStorageDimensionGroupItem>().AsNoTracking().SingleAsync(x => x.ItemDataAreaId == "dat" && x.ItemId == "ITEM-001", ct);
            var tracking = await db.Set<EcoResTrackingDimensionGroupItem>().AsNoTracking().SingleAsync(x => x.ItemDataAreaId == "dat" && x.ItemId == "ITEM-001", ct);
            db.Add(new EcoResStorageDimensionGroupItem { ItemDataAreaId = "dat", ItemId = Item, StorageDimensionGroup = storage.StorageDimensionGroup });
            db.Add(new EcoResTrackingDimensionGroupItem { ItemDataAreaId = "dat", ItemId = Item, TrackingDimensionGroup = tracking.TrackingDimensionGroup });
            foreach (var module in new[] { ModuleInventPurchSales.Inventory, ModuleInventPurchSales.Purchase, ModuleInventPurchSales.Sales })
                db.Add(new InventTableModule { DataAreaId = "dat", ItemId = Item, ModuleType = module, UnitId = "PCS", PriceUnit = 1, Price = module == ModuleInventPurchSales.Purchase ? 60 : module == ModuleInventPurchSales.Sales ? 100 : 50 });
        }
        await db.SaveChangesAsync(ct);
        var policy = await db.Set<InventModelGroup>().SingleAsync(x => x.DataAreaId == "dat" && x.ModelGroupId == Group, ct);
        if (policy.InventModel != InventModel.FIFO || policy.NegativePhysical != NoYes.No || policy.NegativeFinancial != NoYes.No)
            throw new InvalidOperationException("The dedicated FIFO example policy was customized; it was preserved.");
        await tx.CommitAsync(ct);
    }
}
