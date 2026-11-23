using IAX.IXApi.Infrastructure.Persistence.SeedingERPV2.Core.Contracts;
using IAX.IXApi.Infrastructure.Persistence.SeedingERPV2.Core.Models;
using IAX.IXApi.Infrastructure.Persistence.SeedingERPV2.Modules.GeneralLedger;
using IAX.IXApi.Modules.Finance.AccountsPayable;
using IAX.IXApi.Modules.Finance.Common;
using IAX.IXApi.Modules.Finance.Entities;
using IAX.IXApi.Modules.Finance.Inventory.ProductCategories.Entities;
using Microsoft.EntityFrameworkCore;

namespace IAX.IXApi.Infrastructure.Persistence.SeedingERPV2.Modules.AccountsPayable;

/// <summary>
/// Setup for purchase order prepayment (example AP-10), kept apart from the other AP masters so it only ever adds
/// rows: a vendor with no sales tax group (the prepayment invoice path refuses taxed purchases), an advance posting
/// profile whose vendor summary is the payable account, a procurement category whose <c>PurchAdvance</c> posting is
/// the supplier-advance asset (181005), and the company's advance profile on <see cref="VendParameters"/>, set only
/// when the company has none so an operator's own profile is kept.
/// </summary>
public sealed class VendPrepaymentSetupSeeder : IDataSeeder
{
    public const string Vendor = "PREPAY";
    public const string PostingProfile = "APV2-ADV";
    public const string CategoryName = "APV2 purchase prepayment";

    public static string VendorAccount(string company) => $"APV2-{company}-{Vendor}";

    public string Id => "ERPV2.VendPrepaymentSetupSeeder";
    public IReadOnlyCollection<string> Dependencies => ["ERPV2.VendParametersSeeder", "ERPV2.VendGroupSeeder", "ERPV2.VendPaymModeTableSeeder",
        "ERPV2.PaymTermSeeder", "ERPV2.CurrencySeeder", "ERPV2.InventLocationSeeder", "ERPV2.DimensionAttributeValueCombinationSeeder"];

    public async Task SeedAsync(SeedContext c, CancellationToken ct)
    {
        var account = VendorAccount(c.Company);
        var party = await c.EnsureAsync<DirPartyTable>(x => x.PartyNumber == account, new()
        {
            PartyNumber = account, Name = "Prepayment demo supplier", NameAlias = "Prepayment demo supplier", LanguageId = "en-us", IsActive = NoYes.Yes
        }, ct, nameof(DirPartyTable.Name), nameof(DirPartyTable.NameAlias), nameof(DirPartyTable.LanguageId), nameof(DirPartyTable.IsActive));
        await c.EnsureAsync<VendTable>(x => x.AccountNum == account, new()
        {
            AccountNum = account, Party = party.RecId, VendGroup = "APV2", InvoiceAccount = account, PaymTermId = "APV2-N30", PaymMode = "APV2",
            Currency = "SAR", TaxGroup = string.Empty, InventSiteId = "APV2-SITE", InventLocation = "APV2-WH", MatchingPolicy = MatchingPolicy.ThreeWay
        }, ct, nameof(VendTable.Party), nameof(VendTable.VendGroup), nameof(VendTable.InvoiceAccount), nameof(VendTable.Currency), nameof(VendTable.TaxGroup));

        var payable = await c.RequireAsync<DimensionAttributeValueCombination>(x => x.MainAccountValue == GeneralLedgerSeedExample.VendorControl, ct);
        var advance = await c.RequireAsync<DimensionAttributeValueCombination>(x => x.MainAccountValue == GeneralLedgerSeedExample.VendorAdvance, ct);
        await c.EnsureAsync<VendLedgerAccounts>(x => x.PostingProfile == PostingProfile && x.AccountCode == AccountCode.All,
            new() { PostingProfile = PostingProfile, AccountCode = AccountCode.All, Num = string.Empty, SummaryLedgerDimension = payable.RecId },
            ct, nameof(VendLedgerAccounts.SummaryLedgerDimension));

        var hierarchy = await c.EnsureAsync<EcoResCategoryHierarchy>(x => x.Name == CategoryName, new() { Name = CategoryName }, ct, nameof(EcoResCategoryHierarchy.Name));
        var category = await c.EnsureAsync<EcoResCategory>(x => x.Name == CategoryName && x.CategoryHierarchy == hierarchy.RecId,
            new() { Name = CategoryName, CategoryHierarchy = hierarchy.RecId }, ct, nameof(EcoResCategory.Name), nameof(EcoResCategory.CategoryHierarchy));
        await c.EnsureAsync<InventPosting>(x => x.InventAccountType == InventAccountType.PurchAdvance && x.CategoryRelation == category.RecId,
            new() { InventAccountType = InventAccountType.PurchAdvance, ItemCode = TableGroupAll.All, CustVendCode = TableGroupAll.All,
                CategoryRelation = category.RecId, LedgerDimension = advance.RecId }, ct, nameof(InventPosting.LedgerDimension));

        // The parameters row is owned by VendParametersSeeder; only a missing advance profile is filled in.
        var parameters = await c.Rows<VendParameters>().SingleAsync(x => x.Key == 0, ct);
        if (string.IsNullOrWhiteSpace(parameters.AdvancePostingProfile))
        {
            parameters.AdvancePostingProfile = PostingProfile;
            await c.Db.SaveChangesAsync(ct);
        }
    }
}
