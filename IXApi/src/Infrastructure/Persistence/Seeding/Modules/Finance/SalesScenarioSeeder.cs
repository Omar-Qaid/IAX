using IAX.IXApi.Infrastructure.Identity;
using IAX.IXApi.Infrastructure.Persistence.Repositories;
using IAX.IXApi.Infrastructure.Persistence.Seeding.Core;
using IAX.IXApi.Modules.Administration.NumberSequences;
using IAX.IXApi.Modules.Finance.AccountsReceivable;
using IAX.IXApi.Modules.Finance.AccountsReceivable.Confirm;
using IAX.IXApi.Modules.Finance.AccountsReceivable.Invoice;
using IAX.IXApi.Modules.Finance.AccountsReceivable.PackingSlip;
using IAX.IXApi.Modules.Finance.AccountsReceivable.PackingSlip.Dtos;
using IAX.IXApi.Modules.Finance.AccountsReceivable.SalesOrder.Services;
using IAX.IXApi.Modules.Finance.Entities;
using IAX.IXApi.Modules.Identity.Users;
using IAX.IXApi.Shared.Application.Identity;
using Microsoft.EntityFrameworkCore;

namespace IAX.IXApi.Infrastructure.Persistence.Seeding.Modules.Finance;

/// <summary>Resumable sales examples. Each posting service owns its normal transaction.</summary>
public sealed class SalesScenarioSeeder : BaseModuleSeeder
{
    public override string Id => "Finance.SalesScenarios";
    public override SeederCategory Category => SeederCategory.Demo;
    public override SeederExecutionPolicy Policy => SeederExecutionPolicy.RunOnce;
    public override string Scope => "dat";
    public override IReadOnlyCollection<string> DependsOn => new[] { "Finance.PurchaseScenarios" };

    public override async Task SeedAsync(SeedingContext context, CancellationToken ct)
    {
        var userId = await context.Db.Set<AspNetUser>().IgnoreQueryFilters().Where(x => x.UserName == "sys").Select(x => x.Id).SingleAsync(ct);
        var actor = new ExampleActor(userId);
        // Sales posting services explicitly own transactions. Use a separate connection so
        // they retain their existing locking/commit behavior, and resume by durable business keys.
        await using var db = new ApplicationDbContext(new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseSqlServer(context.Db.Database.GetConnectionString()).Options, companyContext: actor);
        var work = new UnitOfWork(db);
        var numbers = new SysNumberSequenceService(work, actor);
        var inventoryNumbers = new SalesInventoryNumberService(numbers);
        var validation = new SalesOrderValidationService(work, actor);
        var demand = new SalesInventoryDemandService(work, inventoryNumbers, new InventDimensionResolver(work, inventoryNumbers));
        var orders = new SalesOrderQuickCreateService(work, actor, numbers, validation);
        var lines = new SalesOrderLineService(work, actor, validation, demand, new SalesUnitConversionService(work));
        var confirmations = new SalesOrderConfirmationService(work, actor, numbers, actor, new SalesOrderConfirmationValidator());
        var deliveries = new PackingSlipJournalService(work, actor, actor, numbers);
        var invoices = new SalesInvoiceService(work, actor, actor, numbers);
        var date = DateTime.UtcNow.Date;
        foreach (var (marker, quantity, delivered, invoiced) in new[] {
            ("DEMO-SO-OPEN", 2m, 0m, 0m), ("DEMO-SO-PARTIAL", 3m, 2m, 0m), ("DEMO-SO-INVOICED", 4m, 4m, 4m) })
        {
            var order = await db.Set<SalesTable>().IgnoreQueryFilters().SingleOrDefaultAsync(x => x.DataAreaId == "dat" && x.CustomerRef == marker, ct);
            if (order == null)
            {
                var created = await orders.CreateAsync(new SalesOrderQuickCreateDto { CustomerAccount = "CUST-100",
                    CustomerReference = marker, SalesName = marker, CurrencyCode = "SAR", InventSiteId = "JED", InventLocationId = "JED-1",
                    RequestedReceiptDate = date, RequestedShipDate = date, TaxGroupId = "DOM" }, ct);
                if (created.Error != null) throw new InvalidOperationException(created.Error);
                order = await db.Set<SalesTable>().SingleAsync(x => x.DataAreaId == "dat" && x.CustomerRef == marker, ct);
            }
            var line = await db.Set<SalesLine>().SingleOrDefaultAsync(x => x.DataAreaId == "dat" && x.SalesId == order.SalesId, ct);
            if (line == null)
            {
                var added = await lines.AddAsync(order.RecId, new SalesLineInputDto { ItemNumber = "ITEM-001", Quantity = quantity,
                    Unit = "PCS", UnitPrice = 150m, PriceUnit = 1m, InventSiteId = "JED", InventLocationId = "JED-1",
                    Description = "Example office chairs", DeliveryDate = date, TaxGroup = "DOM" }, ct);
                if (added.Status != SalesLineWriteStatus.Success) throw new InvalidOperationException(added.Error);
                line = await db.Set<SalesLine>().SingleAsync(x => x.DataAreaId == "dat" && x.SalesId == order.SalesId, ct);
            }
            if (!await db.Set<CustConfirmJour>().AnyAsync(x => x.DataAreaId == "dat" && x.SalesId == order.SalesId && x.ParmId == marker, ct))
            {
                var result = await confirmations.PostAsync(order.RecId, new PostConfirmationRequest(date, true, marker), ct);
                Require(result.StatusCode, result.Message);
            }
            if (delivered > 0 && !await db.Set<CustPackingSlipJour>().AnyAsync(x => x.DataAreaId == "dat" && x.SalesId == order.SalesId && x.ParmId == marker, ct))
            {
                var result = await deliveries.PostAsync(order.RecId, new PostPackingSlipRequest(date, "All", true, marker,
                    new[] { new PackingSlipPostingLineRequest(line.RecId.ToString(), delivered) }), ct);
                Require(result.StatusCode, result.Message);
            }
            if (invoiced > 0 && !await db.Set<CustInvoiceJour>().AnyAsync(x => x.DataAreaId == "dat" && x.SalesId == order.SalesId && x.ParmId == marker, ct))
            {
                var result = await invoices.PostAsync(order.RecId, new SalesInvoicePostDto(date, marker,
                    new() { new SalesInvoiceLineDto(line.RecId, invoiced) }), ct);
                Require(result.StatusCode, result.Message);
            }
        }
    }

    private static void Require(int status, string? message)
    {
        if (status is < 200 or >= 300) throw new InvalidOperationException($"Sales example failed ({status}): {message}");
    }

    internal sealed class ExampleActor(string id) : ICurrentUserService, ICompanyExecutionContext
    {
        public string GetCurrentUserId() => id;
        public string GetOwnerAccountId() => id;
        public string GetDataAreaId() => "dat";
        public bool IsRequestedCompanyAuthorized() => true;
    }
}
