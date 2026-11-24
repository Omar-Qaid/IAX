using IAX.IXApi.Infrastructure.Persistence.SeedingERPV2.Core.Contracts;
using IAX.IXApi.Infrastructure.Persistence.SeedingERPV2.Core.Models;
using IAX.IXApi.Infrastructure.Persistence.SeedingERPV2.Modules.GeneralLedger;
using IAX.IXApi.Modules.Administration.NumberSequences;
using IAX.IXApi.Modules.Finance.AccountsPayable;
using IAX.IXApi.Modules.Finance.Common;
using IAX.IXApi.Modules.Finance.Entities;
using IAX.IXApi.Modules.Finance.Foundation.Dimensions.AccountStructure;
using IAX.IXApi.Modules.Finance.Foundation.Dimensions.LedgerDimension;
using Microsoft.EntityFrameworkCore;

namespace IAX.IXApi.Infrastructure.Persistence.SeedingERPV2.Modules.AccountsPayable;

/// <summary>
/// Setup for the D365 invoice register and invoice approval journals: the Arrival and Offset accounts (their own
/// account structure, since an activated structure cannot be widened), the two journal names (journal types
/// PurchaseLedger and Approval), and the Arrival/Offset accounts on the APV2 vendor posting profile line. An
/// operator's own Arrival or Offset choice on that line is kept; only empty fields are filled.
/// </summary>
public sealed class VendInvoiceRegisterSeeder : IDataSeeder
{
    public const string RegisterJournalName = "APV2-IREG";
    public const string ApprovalJournalName = "APV2-IAPP";

    public string Id => "ERPV2.VendInvoiceRegisterSeeder";
    public IReadOnlyCollection<string> Dependencies =>
        ["ERPV2.MainAccountSeeder", "ERPV2.LedgerSeeder", "ERPV2.VendPostingSeeder", "ERPV2.LedgerJournalNameSeeder"];

    public async Task SeedAsync(SeedContext c, CancellationToken ct)
    {
        var builder = new AccountStructureBuilder(c.Db);
        var structure = await c.Rows<DimensionHierarchy>().SingleOrDefaultAsync(x => x.Name == "APV2-IR", ct);
        if (structure == null)
        {
            var (defined, error) = await builder.DefineAsync(c.Company, new AccountStructureCreateDto("APV2-IR", "Invoice register example main accounts",
                [new AccountStructureSegmentDto(AccountStructureBuilder.MainAccountSegment,
                    string.Join(';', GeneralLedgerSeedExample.InvoiceRegisterAccounts))]), ct);
            if (defined == null || error != null) throw new InvalidOperationException(error);
            var (activated, activationError) = await builder.ActivateAsync(c.Company, "APV2-IR", ct);
            if (activated == null || activationError != null) throw new InvalidOperationException(activationError);
            structure = activated;
        }
        if (structure.IsDeleted || structure.IsDraft == NoYes.Yes) throw new InvalidOperationException("APV2-IR structure is unavailable.");
        await c.RequireAsync<Ledger>(x => x.Name == "APV2", ct);
        var (_, assignmentError) = await builder.AssignToLedgerAsync(c.Company, structure.Name, ct);
        if (assignmentError != null) throw new InvalidOperationException(assignmentError);
        var chart = await c.RequireAsync<LedgerChartOfAccounts>(x => x.Name == "APV2", ct);
        var codes = GeneralLedgerSeedExample.InvoiceRegisterAccounts;
        var combinations = new Dictionary<string, long>();
        foreach (var account in await c.Rows<MainAccount>().Where(x => x.LedgerChartOfAccounts == chart.RecId && codes.Contains(x.MainAccountId) && !x.IsDeleted).ToListAsync(ct))
        {
            var combination = await new LedgerDimensionResolver(c.Db).ResolveAsync(c.Company, account, [], structure.RecId, ct);
            if (combination.MainAccount != account.RecId || combination.AccountStructure != structure.RecId || combination.IsDeleted || !combination.IsActive)
                throw new InvalidOperationException("Invoice register example combination conflicts.");
            combinations[account.MainAccountId] = combination.RecId;
        }
        if (combinations.Count != codes.Count) throw new InvalidOperationException("Invoice register example main accounts are missing.");

        var sequence = await c.RequireAsync<SysNumberSequence>(x => x.NumberSequence == "APV2-VOUCH", ct);
        foreach (var (code, name, type) in new[]
        {
            (RegisterJournalName, "Vendor invoice register (demo)", LedgerJournalType.PurchaseLedger),
            (ApprovalJournalName, "Vendor invoice approval (demo)", LedgerJournalType.Approval)
        })
            await c.EnsureAsync<LedgerJournalName>(x => x.JournalName == code,
                new() { JournalName = code, Name = name, JournalType = type, NumberSequenceTable = sequence.RecId, CurrencyCode = "SAR" }, ct);

        var rule = await c.Rows<VendLedgerAccounts>()
            .SingleOrDefaultAsync(x => x.PostingProfile == "APV2" && x.AccountCode == AccountCode.Group && x.Num == "APV2", ct)
            ?? throw new InvalidOperationException("The APV2 vendor posting profile line is missing.");
        if (rule.PurchasingLedgerDimension == 0) rule.PurchasingLedgerDimension = combinations[GeneralLedgerSeedExample.InvoiceRegisterArrival];
        if (rule.PurchasingOffsetLedgerDimension == 0) rule.PurchasingOffsetLedgerDimension = combinations[GeneralLedgerSeedExample.InvoiceRegisterOffset];
        await c.Db.SaveChangesAsync(ct);
    }
}
