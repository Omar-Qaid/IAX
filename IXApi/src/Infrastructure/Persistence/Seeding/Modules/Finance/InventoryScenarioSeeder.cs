using IAX.IXApi.Infrastructure.Persistence.Repositories;
using IAX.IXApi.Infrastructure.Persistence.Seeding.Core;
using IAX.IXApi.Modules.Administration.NumberSequences;
using IAX.IXApi.Modules.Finance.AccountsReceivable.SalesOrder.Services;
using IAX.IXApi.Modules.Finance.Common;
using IAX.IXApi.Modules.Finance.Entities;
using IAX.IXApi.Modules.Finance.Inventory;
using IAX.IXApi.Modules.Identity.Users;
using Microsoft.EntityFrameworkCore;

namespace IAX.IXApi.Infrastructure.Persistence.Seeding.Modules.Finance;

public sealed class InventoryScenarioSeeder : BaseModuleSeeder
{
    public override string Id => "Finance.InventoryScenarios";
    public override SeederCategory Category => SeederCategory.Demo;
    public override SeederExecutionPolicy Policy => SeederExecutionPolicy.RunOnce;
    public override string Scope => "dat";
    public override IReadOnlyCollection<string> DependsOn => new[] { "Finance.BusinessSetupExamples", "Administration.NumberSequences", "Identity.MasterData" };

    public override async Task SeedAsync(SeedingContext context, CancellationToken ct)
    {
        var db = context.Db;
        var userId = await db.Set<AspNetUser>().IgnoreQueryFilters().Where(x => x.UserName == "sys").Select(x => x.Id).SingleAsync(ct);
        var actor = new SalesScenarioSeeder.ExampleActor(userId);
        var work = new UnitOfWork(db);
        var numbers = new SysNumberSequenceService(work, actor);
        var inventoryNumbers = new SalesInventoryNumberService(numbers);
        var journals = new InventJournalService(work, actor, actor, numbers, new InventDimensionResolver(work, inventoryNumbers));
        var posting = new InventJournalPostingService(work, actor, actor, numbers, inventoryNumbers);
        var cutoff = new DateTime(2026, 10, 2);
        // Closing is company-wide: refuse to settle any pre-existing business history.
        if (await db.Set<InventTrans>().IgnoreQueryFilters().AnyAsync(x => x.DataAreaId == "dat" && x.DateFinancial < cutoff.AddDays(1)
            && (x.StatusReceipt == StatusReceipt.Purchased || x.StatusIssue == StatusIssue.Sold), ct))
            throw new InvalidOperationException("The inventory close example requires no existing financial inventory history through 2026-10-02; existing transactions were preserved.");
        foreach (var (marker, qty, date) in new[] { ("DEMO-INV-RECEIPT", 10m, cutoff.AddDays(-1)), ("DEMO-INV-ISSUE", -2m, cutoff) })
        {
            var created = await journals.CreateAsync(new InventJournalCreateDto { JournalNameId = "IJ-ADJ", Description = marker,
                InventSiteId = "JED", InventLocationId = "JED-1" }, ct);
            if (created.Error != null) throw new InvalidOperationException(created.Error);
            var added = await journals.AddLineAsync(created.Journal!.JournalId, new InventJournalLineInputDto {
                TransDate = date, ItemId = "ITM-1002", Unit = "PCK", Qty = qty, CostPrice = 20,
                InventSiteId = "JED", InventLocationId = "JED-1" }, ct);
            if (added.Error != null) throw new InvalidOperationException(added.Error);
            var posted = await posting.PostAsync(created.Journal.JournalId, ct);
            if (posted.Error != null) throw new InvalidOperationException(posted.Error);
        }
        var closed = await new InventCloseService(work, actor, actor, numbers).CloseAsync(cutoff, ct);
        if (closed.Error != null) throw new InvalidOperationException(closed.Error);

        var count = await journals.CreateAsync(new InventJournalCreateDto { JournalNameId = "IJ-CNT", Description = "DEMO-INV-COUNT",
            InventSiteId = "JED", InventLocationId = "JED-1" }, ct);
        if (count.Error != null) throw new InvalidOperationException(count.Error);
        var counted = await journals.AddLineAsync(count.Journal!.JournalId, new InventJournalLineInputDto { TransDate = DateTime.UtcNow.Date,
            ItemId = "ITM-1002", Unit = "PCK", Counted = 9, CostPrice = 20, InventSiteId = "JED", InventLocationId = "JED-1" }, ct);
        if (counted.Error != null) throw new InvalidOperationException(counted.Error);
        var countLine = await db.Set<InventJournalTrans>().SingleAsync(x => x.DataAreaId == "dat" && x.JournalId == count.Journal.JournalId, ct);
        db.Add(new InventCountJour { ItemId = countLine.ItemId, InventDimId = countLine.InventDimId, JournalId = countLine.JournalId,
            CountDate = countLine.TransDate, CountedQty = countLine.Counted, InventOnHand = countLine.InventOnHand, Ok = NoYes.No });
        await db.SaveChangesAsync(ct);
    }
}
