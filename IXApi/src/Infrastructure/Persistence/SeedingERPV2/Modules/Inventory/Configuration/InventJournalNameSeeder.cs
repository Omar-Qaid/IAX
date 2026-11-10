using IAX.IXApi.Infrastructure.Persistence.SeedingERPV2.Core.Contracts;
using IAX.IXApi.Infrastructure.Persistence.SeedingERPV2.Core.Models;
using IAX.IXApi.Modules.Administration.NumberSequences;
using IAX.IXApi.Modules.Finance.Common;
using IAX.IXApi.Modules.Finance.Entities;
using Microsoft.EntityFrameworkCore;

namespace IAX.IXApi.Infrastructure.Persistence.SeedingERPV2.Modules.Inventory;

/// <summary>
/// The four inventory journal names and the parameter selections that point at them. Journals cannot be created
/// without a name, and <c>InventJournalPostingService</c> refuses to post one whose voucher sequence cannot
/// allocate, so the name and its sequence are seeded together.
/// </summary>
public sealed class InventJournalNameSeeder : IDataSeeder
{
    public string Id => "ERPV2.InventJournalNameSeeder";
    public IReadOnlyCollection<string> Dependencies => ["ERPV2.InventoryNumberSequenceSeeder"];
    public async Task SeedAsync(SeedContext c, CancellationToken ct)
    {
        var voucher = await c.RequireAsync<SysNumberSequence>(x => x.NumberSequence == InventJournalSeedExample.VoucherSequence, ct);
        foreach (var (journalNameId, description, journalType) in InventJournalSeedExample.Names)
            await c.EnsureAsync<InventJournalName>(x => x.JournalNameId == journalNameId,
                new()
                {
                    JournalNameId = journalNameId,
                    Description = description,
                    JournalType = journalType,
                    VoucherNumberSequenceTable = voucher.RecId,
                    VoucherChange = JournalVoucherChange.None,
                    VoucherDraw = JournalVoucherDraw.Draw,
                    DetailSummary = DetailSummary.Detail,
                    DeletePostedLines = NoYes.No,
                    // A counting journal reads its own on-hand, so an automatic reservation on the
                    // journal line would double-count the quantity it is correcting.
                    Reservation = ItemReservation.None,
                    CountingStatusRegistrationPolicy = CountingStatusRegistrationPolicy.Standard,
                    WorkflowApproval = NoYes.No
                }, ct);

        await c.EnsureAsync<InventCountGroup>(x => x.CountGroupId == InventJournalSeedExample.CountGroup,
            new()
            {
                CountGroupId = InventJournalSeedExample.CountGroup,
                Name = "Inventory counting group (demo)",
                CountCode = InventCountCode.Manual,
                CountPeriod = 0
            }, ct);
    }
}
