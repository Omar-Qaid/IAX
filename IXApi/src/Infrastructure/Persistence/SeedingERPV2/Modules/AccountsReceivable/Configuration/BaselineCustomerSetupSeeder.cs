using IAX.IXApi.Infrastructure.Persistence.SeedingERPV2.Core.Contracts;
using IAX.IXApi.Infrastructure.Persistence.SeedingERPV2.Core.Models;
using IAX.IXApi.Infrastructure.Persistence.SeedingERPV2.Modules.Foundation;
using IAX.IXApi.Modules.Finance.AccountsReceivable;

namespace IAX.IXApi.Infrastructure.Persistence.SeedingERPV2.Modules.AccountsReceivable;

/// <summary>Customer groups.</summary>
public sealed class BaselineCustGroupSeeder : IBaselineSeeder
{
    public const string SeederId = "Finance.AccountsReceivable.CustGroups";
    public string Id => SeederId;
    public IReadOnlyCollection<string> Dependencies => [BaselineCompanySeeder.SeederId];

    public Task SeedAsync(SeedContext c, CancellationToken ct) =>
        c.AddMissingAsync(AccountsReceivableBaselineCatalog.Load().CustGroups, row => x => x.CustGroupId == row.CustGroupId, ct);
}

/// <summary>
/// Customer posting profiles and their account rows. The rows name the group, customer or all-customers scope a
/// profile applies to; the ledger accounts are left for the operator, as they were before.
/// </summary>
public sealed class BaselineCustPostingProfileSeeder : IBaselineSeeder
{
    public const string SeederId = "Finance.AccountsReceivable.CustLedger";
    public string Id => SeederId;
    public IReadOnlyCollection<string> Dependencies => [BaselineCustGroupSeeder.SeederId];

    public async Task SeedAsync(SeedContext c, CancellationToken ct)
    {
        var catalog = AccountsReceivableBaselineCatalog.Load();
        await c.AddMissingAsync(catalog.PostingProfiles, row => x => x.PostingProfile == row.PostingProfile, ct);
        await c.AddMissingAsync(catalog.PostingProfileAccounts,
            row => x => x.PostingProfile == row.PostingProfile && x.AccountCode == row.AccountCode && x.Num == row.Num, ct);
    }
}

/// <summary>Customer methods of payment.</summary>
public sealed class BaselineCustPaymModeSeeder : IBaselineSeeder
{
    public const string SeederId = "Finance.AccountsReceivable.PaymModes";
    public string Id => SeederId;
    public IReadOnlyCollection<string> Dependencies => [BaselineCompanySeeder.SeederId];

    public Task SeedAsync(SeedContext c, CancellationToken ct) =>
        c.AddMissingAsync(AccountsReceivableBaselineCatalog.Load().PaymentModes, row => x => x.PaymMode == row.PaymMode, ct);
}

/// <summary>Sales pools.</summary>
public sealed class BaselineSalesPoolSeeder : IBaselineSeeder
{
    public const string SeederId = "Finance.AccountsReceivable.SalesPools";
    public string Id => SeederId;
    public IReadOnlyCollection<string> Dependencies => [BaselineCompanySeeder.SeederId];

    public Task SeedAsync(SeedContext c, CancellationToken ct) =>
        c.AddMissingAsync(AccountsReceivableBaselineCatalog.Load().SalesPools, row => x => x.SalesPoolId == row.SalesPoolId, ct);
}

/// <summary>Trade agreement journal names, price and discount groups, and one draft sales price agreement journal.</summary>
public sealed class BaselinePriceDiscSeeder : IBaselineSeeder
{
    public const string SeederId = "Finance.AccountsReceivable.PriceDisc";
    public string Id => SeederId;
    public IReadOnlyCollection<string> Dependencies => [BaselineCustGroupSeeder.SeederId];

    public async Task SeedAsync(SeedContext c, CancellationToken ct)
    {
        var catalog = AccountsReceivableBaselineCatalog.Load();
        await c.AddMissingAsync(catalog.PriceDiscJournalNames, row => x => x.JournalName == row.JournalName, ct);
        await c.AddMissingAsync(catalog.PriceDiscGroups, row => x => x.GroupId == row.GroupId && x.Module == row.Module && x.Type == row.Type, ct);
        await c.AddMissingAsync(catalog.PriceDiscJournals, row => x => x.JournalNum == row.JournalNum, ct);
    }
}

/// <summary>Modes and terms of delivery, terms of payment, and an equal three-instalment payment schedule.</summary>
public sealed class BaselineDeliverySetupSeeder : IBaselineSeeder
{
    public const string SeederId = "Finance.AccountsReceivable.DeliverySetup";
    public string Id => SeederId;
    public IReadOnlyCollection<string> Dependencies => [BaselineCompanySeeder.SeederId];

    public async Task SeedAsync(SeedContext c, CancellationToken ct)
    {
        var catalog = AccountsReceivableBaselineCatalog.Load();
        await c.AddMissingAsync(catalog.DeliveryModes, row => x => x.Code == row.Code, ct);
        await c.AddMissingAsync(catalog.DeliveryTerms, row => x => x.Code == row.Code, ct);
        await c.AddMissingAsync(catalog.PaymentTerms, row => x => x.PaymTermId == row.PaymTermId, ct);
        await c.AddMissingAsync(catalog.PaymentSchedules, row => x => x.Name == row.Name, ct);
        await c.AddMissingAsync(catalog.PaymentScheduleLines, row => x => x.Name == row.Name && x.LineNum == row.LineNum, ct);
    }
}

/// <summary>Sales and purchase charge codes.</summary>
public sealed class BaselineChargeSeeder : IBaselineSeeder
{
    public const string SeederId = "Finance.AccountsReceivable.Markup";
    public string Id => SeederId;
    public IReadOnlyCollection<string> Dependencies => [BaselineCompanySeeder.SeederId];

    public Task SeedAsync(SeedContext c, CancellationToken ct) =>
        c.AddMissingAsync(AccountsReceivableBaselineCatalog.Load().Charges, row => x => x.MarkupCode == row.MarkupCode, ct);
}
