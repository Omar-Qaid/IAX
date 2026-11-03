using IAX.IXApi.Infrastructure.Identity;
using IAX.IXApi.Infrastructure.Persistence.Repositories;
using IAX.IXApi.Infrastructure.Persistence.Seeding.Core;
using IAX.IXApi.Modules.Administration.NumberSequences;
using IAX.IXApi.Modules.Finance.AccountsPayable;
using IAX.IXApi.Modules.Finance.AccountsReceivable;
using IAX.IXApi.Modules.Finance.AccountsReceivable.SalesOrder.Services;
using IAX.IXApi.Modules.Identity.Users;
using IAX.IXApi.Shared.Application.Identity;
using Microsoft.EntityFrameworkCore;

namespace IAX.IXApi.Infrastructure.Persistence.Seeding.Modules.Finance;

/// <summary>Exercises partial invoicing and two-stage completion through production services; amounts exclude unsupported purchase VAT.</summary>
public sealed class PurchasePartialInvoiceSeeder : BaseModuleSeeder
{
    public override string Id => "Finance.PurchasePartialInvoices";
    public override SeederCategory Category => SeederCategory.Demo;
    public override SeederExecutionPolicy Policy => SeederExecutionPolicy.RunOnce;
    public override string Scope => "dat";
    public override IReadOnlyCollection<string> DependsOn => new[] { "Finance.PurchaseScenarios" };

    public override async Task SeedAsync(SeedingContext context, CancellationToken ct)
    {
        var db = context.Db;
        var userId = await db.Set<AspNetUser>().IgnoreQueryFilters().Where(x => x.UserName == "sys").Select(x => x.Id).SingleAsync(ct);
        var actor = new ExampleActor(userId);
        // UnitOfWork participates in the seeder's transaction and does not own/dispose its DbContext here.
        var work = new UnitOfWork(db);
        var numbers = new SysNumberSequenceService(work, actor);
        var inventoryNumbers = new SalesInventoryNumberService(numbers);
        var dimensions = new InventDimensionResolver(work, inventoryNumbers);
        var supply = new PurchInventorySupplyService(work, inventoryNumbers, dimensions);
        var orders = new PurchOrderService(work, actor, actor, numbers, supply, new SalesUnitConversionService(work));
        var confirmation = new PurchConfirmationService(work, actor, actor, numbers);
        var receipts = new ProductReceiptService(work, actor, actor, numbers);
        var invoices = new VendorInvoiceService(work, actor, actor, numbers);
        var date = DateTime.UtcNow.Date;
        foreach (var (marker, quantity, received, invoiced) in new[] {
            ("DEMO-PO-60-OPEN", 100m, 60m, 60m), ("DEMO-PO-60-40", 100m, 60m, 60m) })
        {
            // A complete scenario is atomic with its execution log. An existing scenario is never reposted or overwritten.
            if (await db.Set<PurchTable>().IgnoreQueryFilters().AnyAsync(x => x.DataAreaId == "dat" && x.PurchName == marker, ct)) continue;
            var created = Require(await orders.CreateAsync(new PurchOrderCreateDto("VEND-100", null, "SAR", date, "JED", "JED-1", marker), ct));
            var order = (PurchTable)created.Data!;
            Require(await orders.AddLineAsync(order.RecId,
                new PurchLineInputDto("ITEM-001", quantity, "PCS", 50m, 1m, "JED", "JED-1", date, "Example office chairs"), ct));
            Require(await confirmation.ConfirmAsync(order.RecId, ct));
            var line = await db.Set<PurchLine>().SingleAsync(x => x.DataAreaId == "dat" && x.PurchId == order.PurchId, ct);
            if (received > 0)
                Require(await receipts.PostAsync(order.RecId, new ProductReceiptPostDto(marker + "-R", date,
                    new() { new PurchPostLineDto(line.RecId, received) }), ct));
            if (invoiced > 0)
                Require(await invoices.PostAsync(order.RecId, new VendorInvoicePostDto(marker + "-I", date,
                    new() { new PurchPostLineDto(line.RecId, invoiced) }), ct));
            if (line.RemainPurchPhysical != 40m || line.RemainPurchFinancial != 40m)
                throw new InvalidOperationException("Partial purchase invoice must leave 40 units physically and financially outstanding.");
            if (marker == "DEMO-PO-60-40")
            {
                Require(await receipts.PostAsync(order.RecId, new ProductReceiptPostDto(marker + "-R2", date,
                    new() { new PurchPostLineDto(line.RecId, 40m) }), ct));
                Require(await invoices.PostAsync(order.RecId, new VendorInvoicePostDto(marker + "-I2", date,
                    new() { new PurchPostLineDto(line.RecId, 40m) }), ct));
                if (line.RemainPurchPhysical != 0m || line.RemainPurchFinancial != 0m)
                    throw new InvalidOperationException("Completed purchase example must have no outstanding quantity.");
            }
        }
    }

    private static PurchResult Require(PurchResult result) => result.StatusCode is >= 200 and < 300
        ? result : throw new InvalidOperationException($"Purchase example failed ({result.StatusCode}): {result.Message}");

    private sealed class ExampleActor(string id) : ICurrentUserService, ICompanyExecutionContext
    {
        public string GetCurrentUserId() => id;
        public string GetOwnerAccountId() => id;
        public string GetDataAreaId() => "dat";
        public bool IsRequestedCompanyAuthorized() => true;
    }
}

