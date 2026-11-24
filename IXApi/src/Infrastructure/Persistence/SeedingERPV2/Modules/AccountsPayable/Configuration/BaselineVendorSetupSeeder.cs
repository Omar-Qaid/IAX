using IAX.IXApi.Infrastructure.Persistence.SeedingERPV2.Core.Contracts;
using IAX.IXApi.Infrastructure.Persistence.SeedingERPV2.Core.Models;
using IAX.IXApi.Infrastructure.Persistence.SeedingERPV2.Modules.Foundation;

namespace IAX.IXApi.Infrastructure.Persistence.SeedingERPV2.Modules.AccountsPayable;

/// <summary>Vendor groups.</summary>
public sealed class BaselineVendGroupSeeder : IBaselineSeeder
{
    public const string SeederId = "Finance.AccountsPayable.VendGroups";
    public string Id => SeederId;
    // A vendor group's terms of payment must exist first (VendGroup.PaymTermId is a foreign key).
    public IReadOnlyCollection<string> Dependencies => [AccountsReceivable.BaselineDeliverySetupSeeder.SeederId];

    public Task SeedAsync(SeedContext c, CancellationToken ct) =>
        c.AddMissingAsync(AccountsPayableBaselineCatalog.Load().VendGroups, row => x => x.VendGroupCode == row.VendGroupCode, ct);
}

/// <summary>
/// Vendor posting profiles (D365 VendLedger) and their account rows (VendLedgerAccounts). The rows name the group,
/// vendor or all-vendors scope a profile applies to; the summary, Arrival and Offset accounts are left for the operator.
/// </summary>
public sealed class BaselineVendPostingProfileSeeder : IBaselineSeeder
{
    public const string SeederId = "Finance.AccountsPayable.VendLedger";
    public string Id => SeederId;
    public IReadOnlyCollection<string> Dependencies => [BaselineVendGroupSeeder.SeederId];

    public async Task SeedAsync(SeedContext c, CancellationToken ct)
    {
        var catalog = AccountsPayableBaselineCatalog.Load();
        await c.AddMissingAsync(catalog.PostingProfiles, row => x => x.PostingProfile == row.PostingProfile, ct);
        await c.AddMissingAsync(catalog.PostingProfileAccounts,
            row => x => x.PostingProfile == row.PostingProfile && x.AccountCode == row.AccountCode && x.Num == row.Num, ct);
    }
}

/// <summary>Vendor methods of payment.</summary>
public sealed class BaselineVendPaymModeSeeder : IBaselineSeeder
{
    public const string SeederId = "Finance.AccountsPayable.PaymModes";
    public string Id => SeederId;
    public IReadOnlyCollection<string> Dependencies => [BaselineVendJournalNameSeeder.SeederId];

    public Task SeedAsync(SeedContext c, CancellationToken ct) =>
        c.AddMissingAsync(AccountsPayableBaselineCatalog.Load().PaymentModes, row => x => x.PaymMode == row.PaymMode, ct);
}

/// <summary>Vendor journal names: invoice register (PurchaseLedger), invoice approval (Approval) and vendor payments.</summary>
public sealed class BaselineVendJournalNameSeeder : IBaselineSeeder
{
    public const string SeederId = "Finance.AccountsPayable.JournalNames";
    public string Id => SeederId;
    public IReadOnlyCollection<string> Dependencies => [BaselineCompanySeeder.SeederId];

    public Task SeedAsync(SeedContext c, CancellationToken ct) =>
        c.AddMissingAsync(AccountsPayableBaselineCatalog.Load().JournalNames, row => x => x.JournalName == row.JournalName, ct);
}
