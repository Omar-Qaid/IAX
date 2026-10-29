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
using IAX.IXApi.Modules.Finance.Common;
using IAX.IXApi.Modules.Finance.AccountsReceivable.Return;
using IAX.IXApi.Modules.Identity.Users;
using IAX.IXApi.Shared.Application.Identity;
using Microsoft.EntityFrameworkCore;

namespace IAX.IXApi.Infrastructure.Persistence.Seeding.Modules.Finance;

/// <summary>Resumable sales examples. Each posting service owns its normal transaction.</summary>
public sealed class SalesCompletionScenarioSeeder : BaseModuleSeeder
{
    public override string Id => "Finance.SalesCompletionScenarios";
    public override SeederCategory Category => SeederCategory.Demo;
    public override SeederExecutionPolicy Policy => SeederExecutionPolicy.RunOnce;
    public override string Scope => "dat";
    public override IReadOnlyCollection<string> DependsOn => new[] { "Finance.CommercialScenarios", "Finance.PurchaseVatInvoices" };

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
            ("DEMO-SO-COMPLETE", 10m, 6m, 4m) })
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
                    Unit = "PCS", UnitPrice = 100m, PriceUnit = 1m, InventSiteId = "JED", InventLocationId = "JED-1",
                    Description = "Example office chairs", DeliveryDate = date, TaxGroup = "DOM" }, ct);
                if (added.Status != SalesLineWriteStatus.Success) throw new InvalidOperationException(added.Error);
                line = await db.Set<SalesLine>().SingleAsync(x => x.DataAreaId == "dat" && x.SalesId == order.SalesId, ct);
            }
            if (!await db.Set<CustPackingSlipJour>().AnyAsync(x => x.DataAreaId == "dat" && x.SalesId == order.SalesId, ct))
            {
                var reservation = new IAX.IXApi.Modules.Finance.Inventory.InventoryReservationService(work, actor);
                var reserved = await reservation.ReserveAsync(line.InventTransId, quantity, false, ct);
                if (reserved.Error != null) throw new InvalidOperationException(reserved.Error);
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
            var secondDelivery = await deliveries.PostAsync(order.RecId, new PostPackingSlipRequest(date, "All", true, marker + "-R2",
                new[] { new PackingSlipPostingLineRequest(line.RecId.ToString(), 4m) }), ct);
            Require(secondDelivery.StatusCode, secondDelivery.Message);
            var finalInvoice = await invoices.PostAsync(order.RecId, new SalesInvoicePostDto(date, marker + "-I2",
                new() { new SalesInvoiceLineDto(line.RecId, 6m) }), ct);
            Require(finalInvoice.StatusCode, finalInvoice.Message);
            var postedInvoices = await db.Set<CustInvoiceJour>().Where(x => x.DataAreaId == "dat" && x.SalesId == order.SalesId).ToListAsync(ct);
            if (postedInvoices.Sum(x => x.SalesBalance) != 1000m || postedInvoices.Sum(x => x.SumTax) != 150m)
                throw new InvalidOperationException("Sales completion example requires net 1000 and VAT 150; customized tax setup was preserved.");
            var payments = new IAX.IXApi.Modules.Finance.AccountsReceivable.Payment.CustPaymentService(work, actor, actor, numbers);
            foreach (var posted in postedInvoices)
            {
                var paid = await payments.PostAsync(new IAX.IXApi.Modules.Finance.AccountsReceivable.Payment.CustPaymentPostDto(
                    order.CustAccount, "DEMO-SAR", posted.InvoiceAmount, date, "Transfer", posted.InvoiceId, "SC-PAY-" + posted.RecId), ct);
                Require(paid.StatusCode, paid.Message);
            }
            if (line.RemainSalesPhysical != 0 || line.RemainSalesFinancial != 0)
                throw new InvalidOperationException("Completed sales example still has outstanding quantities.");
            var invoiceIds = postedInvoices.Select(x => x.InvoiceId).ToList();
            if (await (from open in db.Set<CustTransOpen>() join trans in db.Set<CustTrans>() on open.RefRecId equals trans.RecId
                where open.DataAreaId == "dat" && trans.DataAreaId == "dat" && invoiceIds.Contains(trans.Invoice) select open).AnyAsync(ct))
                throw new InvalidOperationException("Completed sales example still has an open invoice balance.");

            // The source inventory transaction is a durable retry key for this dedicated example.
            var returns = new CustReturnService(work, actor, numbers, inventoryNumbers);
            var returned = await db.Set<SalesLine>().SingleOrDefaultAsync(x => x.DataAreaId == "dat"
                && x.InventTransIdReturn == line.InventTransId && x.SalesType == SalesType.ReturnItem, ct);
            if (returned == null)
            {
                var sold = await db.Set<CustInvoiceTrans>().Where(x => x.DataAreaId == "dat" && x.SalesId == order.SalesId)
                    .OrderBy(x => x.RecId).FirstAsync(ct);
                var createdReturn = await returns.CreateAsync(new CustReturnCreateDto(sold.InvoiceId, "UNWANTED",
                    new() { new CustReturnLineDto(sold.RecId, 1m, "RESTOCK") }), ct);
                Require(createdReturn.StatusCode, createdReturn.Message);
                returned = await db.Set<SalesLine>().SingleAsync(x => x.DataAreaId == "dat"
                    && x.InventTransIdReturn == line.InventTransId && x.SalesType == SalesType.ReturnItem, ct);
            }
            var returnOrder = await db.Set<SalesTable>().SingleAsync(x => x.DataAreaId == "dat" && x.SalesId == returned.SalesId, ct);
            var receivedReturn = await returns.ReceiveAsync(returnOrder.RecId, new CustReturnReceiptDto(marker + "-RET", date,
                new() { new CustReturnReceiptLineDto(returned.RecId, 1m) }), ct);
            Require(receivedReturn.StatusCode, receivedReturn.Message);
            var credited = await returns.CreditAsync(returnOrder.RecId, new CustCreditNoteDto(date, marker + "-CN"), ct);
            Require(credited.StatusCode, credited.Message);
            var credit = await db.Set<CustInvoiceJour>().SingleAsync(x => x.DataAreaId == "dat" && x.SalesId == returnOrder.SalesId, ct);
            if (credit.InvoiceAmount != -115m) throw new InvalidOperationException("Example return must credit SAR 100 plus 15 VAT.");
        }

        const string cancelMarker = "DEMO-SO-CANCEL-PRICE";
        var cancelOrder = await db.Set<SalesTable>().SingleOrDefaultAsync(x => x.DataAreaId == "dat" && x.CustomerRef == cancelMarker, ct);
        if (cancelOrder == null)
        {
            var created = await orders.CreateAsync(new SalesOrderQuickCreateDto { CustomerAccount = "CUST-100",
                CustomerReference = cancelMarker, SalesName = cancelMarker, CurrencyCode = "SAR", InventSiteId = "JED", InventLocationId = "JED-1",
                RequestedReceiptDate = date, RequestedShipDate = date, TaxGroupId = "DOM" }, ct);
            if (created.Error != null) throw new InvalidOperationException(created.Error);
            cancelOrder = await db.Set<SalesTable>().SingleAsync(x => x.DataAreaId == "dat" && x.CustomerRef == cancelMarker, ct);
        }
        if (cancelOrder.SalesStatus != SalesStatus.Canceled)
        {
            var cancelLine = await db.Set<SalesLine>().SingleOrDefaultAsync(x => x.DataAreaId == "dat" && x.SalesId == cancelOrder.SalesId, ct);
            if (cancelLine == null)
            {
                var added = await lines.AddAsync(cancelOrder.RecId, new SalesLineInputDto { ItemNumber = "ITEM-001", Quantity = 10,
                    Unit = "PCS", UsePriceAgreement = true, InventSiteId = "JED", InventLocationId = "JED-1", TaxGroup = "DOM", DeliveryDate = date }, ct);
                if (added.Status != SalesLineWriteStatus.Success) throw new InvalidOperationException(added.Error);
                cancelLine = await db.Set<SalesLine>().SingleAsync(x => x.DataAreaId == "dat" && x.SalesId == cancelOrder.SalesId, ct);
            }
            if (cancelLine.SalesPrice != 140m && cancelLine.ManualEntryChangePolicy == 0
                && cancelLine.SalesQty == 10 && cancelLine.RemainSalesPhysical == 10 && cancelLine.RemainSalesFinancial == 10)
            {
                var repriced = await lines.ChangeAsync(cancelOrder.RecId, cancelLine.RecId, new SalesLineInputDto {
                    ItemNumber = "ITEM-001", Quantity = 10, Unit = "PCS", UnitPrice = cancelLine.SalesPrice,
                    UsePriceAgreement = true, InventSiteId = "JED", InventLocationId = "JED-1", TaxGroup = "DOM",
                    TaxItemGroup = cancelLine.TaxItemGroup, DeliveryDate = date }, ct);
                if (repriced.Status != SalesLineWriteStatus.Success) throw new InvalidOperationException(repriced.Error);
            }
            if (cancelLine.SalesPrice != 140m) throw new InvalidOperationException("Quantity-tier example requires its valid SAR 140 agreement; existing agreement was preserved.");
            var confirmed = await confirmations.PostAsync(cancelOrder.RecId, new PostConfirmationRequest(date, true, cancelMarker), ct);
            Require(confirmed.StatusCode, confirmed.Message);
            var reservation = new IAX.IXApi.Modules.Finance.Inventory.InventoryReservationService(work, actor);
            var reserved = await reservation.ReserveAsync(cancelLine.InventTransId, 10m, false, ct);
            if (reserved.Error != null) throw new InvalidOperationException(reserved.Error);
            var released = await reservation.ReleaseAsync(cancelLine.InventTransId, 4m, ct);
            if (released.Error != null) throw new InvalidOperationException(released.Error);
            var cancelled = await new SalesOrderCancellationService(work, demand, actor).CancelAsync(cancelOrder.RecId, ct);
            if (cancelled != SalesOrderCancelStatus.Cancelled) throw new InvalidOperationException("Example sales cancellation failed: " + cancelled);
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
