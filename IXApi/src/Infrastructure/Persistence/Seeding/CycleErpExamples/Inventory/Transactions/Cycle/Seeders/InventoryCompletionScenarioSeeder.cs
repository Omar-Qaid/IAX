using IAX.IXApi.Infrastructure.Persistence.Repositories;
using IAX.IXApi.Infrastructure.Persistence.Seeding.Core;
using IAX.IXApi.Modules.Administration.NumberSequences;
using IAX.IXApi.Modules.Finance.AccountsPayable;
using IAX.IXApi.Modules.Finance.AccountsReceivable.SalesOrder.Services;
using IAX.IXApi.Modules.Finance.Common;
using IAX.IXApi.Modules.Finance.Entities;
using IAX.IXApi.Modules.Finance.Inventory;
using IAX.IXApi.Modules.Identity.Users;
using Microsoft.EntityFrameworkCore;

namespace IAX.IXApi.Infrastructure.Persistence.Seeding.Modules.Finance;

/// <summary>Atomic inventory movement, same-site transfer and vendor-return examples.</summary>
public sealed class InventoryCompletionScenarioSeeder : BaseModuleSeeder
{
    public override string Id => "Finance.InventoryCompletionScenarios";
    public override int Version => 2;
    public override SeederCategory Category => SeederCategory.Demo;
    public override SeederExecutionPolicy Policy => SeederExecutionPolicy.RunOnce;
    public override string Scope => "dat";
    public override IReadOnlyCollection<string> DependsOn => new[] { "Finance.InventoryScenarios", "Finance.SalesCompletionScenarios" };

    public override async Task SeedAsync(SeedingContext context, CancellationToken ct)
    {
        var db = context.Db;
        // Earlier posting examples use independent contexts and advance shared sequences.
        // Start this atomic scenario with fresh snapshots, without discarding pending edits.
        if (db.ChangeTracker.HasChanges())
            throw new InvalidOperationException("Inventory examples require a clean change tracker.");
        db.ChangeTracker.Clear();
        var actor = new SalesScenarioSeeder.ExampleActor(await db.Set<AspNetUser>().IgnoreQueryFilters()
            .Where(x => x.UserName == "sys").Select(x => x.Id).SingleAsync(ct));
        var work = new UnitOfWork(db);
        var numbers = new SysNumberSequenceService(work, actor);
        var inventoryNumbers = new SalesInventoryNumberService(numbers);
        var journals = new InventJournalService(work, actor, actor, numbers, new InventDimensionResolver(work, inventoryNumbers));
        var posting = new InventJournalPostingService(work, actor, actor, numbers, inventoryNumbers);
        var date = DateTime.UtcNow.Date;
        var warehouse = await db.Set<InventLocation>().IgnoreQueryFilters()
            .SingleOrDefaultAsync(x => x.DataAreaId == "dat" && x.InventLocationId == "JED-DEMO", ct);
        if (warehouse == null)
        {
            db.Add(new InventLocation { DataAreaId = "dat", InventLocationId = "JED-DEMO", Name = "Example secondary Jeddah warehouse",
                InventSiteId = "JED", InventLocationType = InventLocationType.Standard, DefaultStatusID = "AVAILABLE" });
            await db.SaveChangesAsync(ct);
        }
        else if (warehouse.InventSiteId != "JED" || warehouse.InventLocationType != InventLocationType.Standard)
            throw new InvalidOperationException("The example destination warehouse has customized geography/type; it was preserved.");

        var offset = await db.Set<InventPosting>().IgnoreQueryFilters().Where(x => x.DataAreaId == "dat"
            && x.InventAccountType == InventAccountType.InventLoss && x.ItemCode == TableGroupAll.All
            && x.CustVendCode == TableGroupAll.All).Select(x => x.LedgerDimension).FirstAsync(ct);
        foreach (var (marker, journalName, qty) in new[] {
            ("DEMO-INV-MOVEMENT", "IJ-MOV", -1m), ("DEMO-INV-TRANSFER", "IJ-TRF", 2m) })
        {
            var journal = await db.Set<InventJournalTable>().SingleOrDefaultAsync(x => x.DataAreaId == "dat" && x.Description == marker, ct);
            if (journal?.Posted == NoYes.Yes) continue;
            if (journal != null)
            {
                // Recover only this seeder's unposted rows from the observed daily-reset collision.
                var collisions = await db.Set<InventJournalTable>().CountAsync(x => x.DataAreaId == "dat" && x.JournalId == journal.JournalId, ct);
                var recoveryLines = await db.Set<InventJournalTrans>().Where(x => x.DataAreaId == "dat" && x.JournalId == journal.JournalId
                    && x.JournalType == journal.JournalType && x.ItemId == "ITM-1002" && x.Qty == qty && x.Voucher == "" && x.InventTransId == "").ToListAsync(ct);
                if (collisions < 2 || recoveryLines.Count != 1)
                    throw new InvalidOperationException("An unposted journal already uses the example marker; it was preserved.");
                var repairedId = (await numbers.NextAsync(nameof(InventJournalTable), "dat", ct)).Code;
                journal.JournalId = repairedId;
                recoveryLines[0].JournalId = repairedId;
                await db.SaveChangesAsync(ct);
                var recoveryPost = await posting.PostAsync(repairedId, ct);
                if (recoveryPost.Error != null) throw new InvalidOperationException(recoveryPost.Error);
                continue;
            }
            var created = await journals.CreateAsync(new InventJournalCreateDto { JournalNameId = journalName, Description = marker,
                InventSiteId = "JED", InventLocationId = "JED-1" }, ct);
            if (created.Error != null) throw new InvalidOperationException(created.Error);
            var added = await journals.AddLineAsync(created.Journal!.JournalId, new InventJournalLineInputDto {
                TransDate = date, ItemId = "ITM-1002", Unit = "PCK", Qty = qty, InventSiteId = "JED", InventLocationId = "JED-1",
                ToInventSiteId = journalName == "IJ-TRF" ? "JED" : "", ToInventLocationId = journalName == "IJ-TRF" ? "JED-DEMO" : "",
                LedgerDimension = journalName == "IJ-MOV" ? offset : 0 }, ct);
            if (added.Error != null) throw new InvalidOperationException(added.Error);
            var posted = await posting.PostAsync(created.Journal.JournalId, ct);
            if (posted.Error != null) throw new InvalidOperationException(posted.Error);
        }

        const string returnMarker = "DEMO-VRET-INV";
        if (!await db.Set<VendInvoiceJour>().AnyAsync(x => x.DataAreaId == "dat" && x.InvoiceId == returnMarker + "-CN", ct))
        {
            if (await db.Set<PurchTable>().AnyAsync(x => x.DataAreaId == "dat" && x.ReturnItemNum == returnMarker, ct))
                throw new InvalidOperationException("An existing uncredited vendor return uses the example marker; it was preserved.");
            var invoiceLine = await db.Set<VendInvoiceTrans>().SingleAsync(x => x.DataAreaId == "dat"
                && x.InvoiceId == "DEMO-PO-VAT-6040-I" && x.ItemId == "ITEM-001", ct);
            var returns = new PurchReturnService(work, actor, numbers, inventoryNumbers);
            Require(await returns.CreateAsync(new PurchReturnCreateDto(invoiceLine.InvoiceId, returnMarker,
                new() { new PurchReturnLineDto(invoiceLine.RecId, 5m) }), ct));
            var order = await db.Set<PurchTable>().SingleAsync(x => x.DataAreaId == "dat" && x.ReturnItemNum == returnMarker, ct);
            var line = await db.Set<PurchLine>().SingleAsync(x => x.DataAreaId == "dat" && x.PurchId == order.PurchId, ct);
            Require(await returns.ShipAsync(order.RecId, new PurchReturnShipDto(returnMarker + "-SHIP", date,
                new() { new PurchPostLineDto(line.RecId, 5m) }), ct));
            Require(await returns.CreditAsync(order.RecId, new VendCreditNoteDto(returnMarker + "-CN", date), ct));
            var credit = await db.Set<VendInvoiceJour>().SingleAsync(x => x.DataAreaId == "dat" && x.InvoiceId == returnMarker + "-CN", ct);
            if (credit.SalesBalance != -250m || credit.SumTax != -37.5m || credit.InvoiceAmount != -287.5m)
                throw new InvalidOperationException("Vendor return must reverse five units at SAR 50 and their original 15% VAT.");
        }
    }

    private static void Require(PurchResult result)
    {
        if (result.StatusCode is < 200 or >= 300) throw new InvalidOperationException(result.Message);
    }
}
