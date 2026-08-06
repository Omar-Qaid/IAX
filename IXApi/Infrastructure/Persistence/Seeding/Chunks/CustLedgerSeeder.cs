using IAX.IXApi.Infrastructure.Persistence;
using IAX.IXApi.Modules.Identity.Authentication;
using IAX.IXApi.Modules.Identity.Users;
using IAX.IXApi.Modules.Identity.Roles;
using IAX.IXApi.Modules.ERP.AccountsReceivable;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using IAX.IXApi.Modules.ERP.Common;

namespace IAX.IXApi.Infrastructure.Persistence.Seeding.Chunks
{
    /// <summary>Seeds CustLedger posting profiles and their account lines.</summary>
    public class CustLedgerSeeder : ISeeder
    {
        public async Task SeedAsync(ApplicationDbContext db, RoleManager<AspNetRole> roles, UserManager<AspNetUser> users, CancellationToken ct)
        {
            // ── CustLedger profiles ─────────────────────────────────────────────
            var profileSeeds = new[]
            {
                new CustLedger
                {
                    DataAreaId          = "dat",
                    PostingProfile      = "GEN",
                    Name                = "General Customers",
                    Settlement          = NoYes.Yes,
                    Interest            = NoYes.Yes,
                    CollectionLetter    = NoYes.Yes,
                },
                new CustLedger
                {
                    DataAreaId          = "dat",
                    PostingProfile      = "PrePayment",
                    Name                = "Customers Advanced Payment",
                    Settlement          = NoYes.No,
                    Interest            = NoYes.No,
                    CollectionLetter    = NoYes.No,
                },
                new CustLedger
                {
                    DataAreaId          = "dat",
                    PostingProfile      = "EXPORT",
                    Name                = "Export Customers",
                    Settlement          = NoYes.Yes,
                    Interest            = NoYes.Yes,
                    CollectionLetter    = NoYes.Yes,
                },
                new CustLedger
                {
                    DataAreaId          = "dat",
                    PostingProfile      = "GOV",
                    Name                = "Government Customers",
                    Settlement          = NoYes.Yes,
                    Interest            = NoYes.No,
                    CollectionLetter    = NoYes.No,
                },
            };

            var existingProfiles = await db.CustLedgers
                .IgnoreQueryFilters()
                .Select(p => p.PostingProfile)
                .ToListAsync(ct);

            var profilesToAdd = profileSeeds
                .Where(p => !existingProfiles.Contains(p.PostingProfile))
                .ToList();

            if (profilesToAdd.Any())
            {
                await db.CustLedgers.AddRangeAsync(profilesToAdd, ct);
                await db.SaveChangesAsync(ct);
            }

            // ── CustLedgerAccounts lines ────────────────────────────────────────
            // AccountCode: 0 = Table (specific customer), 1 = Group, 2 = All
            // SummaryLedgerDimension stores the GL main account ID.
            // Priority in D365 engine: Table > Group > All

            var accountSeeds = new[]
            {
                // ── CASE 1: GROUP (Customer Group Rules - Priority 2) ───────────────
                new CustLedgerAccounts
                {
                    DataAreaId = "dat",
                    PostingProfile = "GEN",
                    AccountCode = AccountCode.Group,
                    Num = "Consultant",
                    SummaryLedgerDimension = 0,
                    ClearingLedgerDimension = 0,
                    VatPrepaymentsLedgerDimension = 0,
                    LiabilitiesForDiscountLedgerDimension = 0,
                    WriteOffLedgerDimension = 0,
                    CollectionLetterCourse = "Collection",
                    CustInterest = 1
                },
                new CustLedgerAccounts
                {
                    DataAreaId = "dat",
                    PostingProfile = "GEN",
                    AccountCode = AccountCode.Group,
                    Num = "Contractor",
                    SummaryLedgerDimension = 0,
                    ClearingLedgerDimension = 0,
                    VatPrepaymentsLedgerDimension = 0,
                    LiabilitiesForDiscountLedgerDimension = 0,
                    WriteOffLedgerDimension = 0,
                    CollectionLetterCourse = "Collection",
                    CustInterest = 1
                },
                new CustLedgerAccounts
                {
                    DataAreaId = "dat",
                    PostingProfile = "GEN",
                    AccountCode = AccountCode.Group,
                    Num = "Government",
                    SummaryLedgerDimension = 0,
                    ClearingLedgerDimension = 0,
                    VatPrepaymentsLedgerDimension = 0,
                    LiabilitiesForDiscountLedgerDimension = 0,
                    WriteOffLedgerDimension = 0,
                    CollectionLetterCourse = "Collection",
                    CustInterest = 1
                },
                new CustLedgerAccounts
                {
                    DataAreaId = "dat",
                    PostingProfile = "GEN",
                    AccountCode = AccountCode.Group,
                    Num = "IKKAff",
                    SummaryLedgerDimension = 0,
                    ClearingLedgerDimension = 0,
                    VatPrepaymentsLedgerDimension = 0,
                    LiabilitiesForDiscountLedgerDimension = 0,
                    WriteOffLedgerDimension = 0,
                    CollectionLetterCourse = "Collection",
                    CustInterest = 1
                },
                new CustLedgerAccounts
                {
                    DataAreaId = "dat",
                    PostingProfile = "GEN",
                    AccountCode = AccountCode.Group,
                    Num = "SisComp",
                    SummaryLedgerDimension = 0,
                    ClearingLedgerDimension = 0,
                    VatPrepaymentsLedgerDimension = 0,
                    LiabilitiesForDiscountLedgerDimension = 0,
                    WriteOffLedgerDimension = 0,
                    CollectionLetterCourse = "Collection",
                    CustInterest = 1
                },
                new CustLedgerAccounts
                {
                    DataAreaId = "dat",
                    PostingProfile = "GEN",
                    AccountCode = AccountCode.Group,
                    Num = "Traders",
                    SummaryLedgerDimension = 0,
                    ClearingLedgerDimension = 0,
                    VatPrepaymentsLedgerDimension = 0,
                    LiabilitiesForDiscountLedgerDimension = 0,
                    WriteOffLedgerDimension = 0,
                    CollectionLetterCourse = "Collection",
                    CustInterest = 1
                },
                new CustLedgerAccounts
                {
                    DataAreaId = "dat",
                    PostingProfile = "GEN",
                    AccountCode = AccountCode.Group,
                    Num = "CUST-RTL",
                    SummaryLedgerDimension = 0,
                    ClearingLedgerDimension = 0,
                    VatPrepaymentsLedgerDimension = 0,
                    LiabilitiesForDiscountLedgerDimension = 0,
                    WriteOffLedgerDimension = 0,
                    CollectionLetterCourse = "Collection",
                    CustInterest = 1
                },

                // ── CASE 2: TABLE (Specific Customer Overrides - Priority 1 Highest) ─
                new CustLedgerAccounts
                {
                    DataAreaId = "dat",
                    PostingProfile = "GEN",
                    AccountCode = AccountCode.Table,
                    Num = "C00015",
                    SummaryLedgerDimension = 0,
                    ClearingLedgerDimension = 0,
                    VatPrepaymentsLedgerDimension = 0,
                    LiabilitiesForDiscountLedgerDimension = 0,
                    WriteOffLedgerDimension = 0,
                    CollectionLetterCourse = "Collection",
                    CustInterest = 1
                },
                new CustLedgerAccounts
                {
                    DataAreaId = "dat",
                    PostingProfile = "GEN",
                    AccountCode = AccountCode.Table,
                    Num = "CUST-300",
                    SummaryLedgerDimension = 0,
                    ClearingLedgerDimension = 0,
                    VatPrepaymentsLedgerDimension = 0,
                    LiabilitiesForDiscountLedgerDimension = 0,
                    WriteOffLedgerDimension = 0,
                    CollectionLetterCourse = "Collection",
                    CustInterest = 1
                },

                // ── CASE 3: ALL (Catch-All Global Fallback - Priority 3 Lowest) ────
                new CustLedgerAccounts
                {
                    DataAreaId = "dat",
                    PostingProfile = "GEN",
                    AccountCode = AccountCode.All,
                    Num = "",
                    SummaryLedgerDimension = 0,
                    ClearingLedgerDimension = 0,
                    VatPrepaymentsLedgerDimension = 0,
                    LiabilitiesForDiscountLedgerDimension = 0,
                    WriteOffLedgerDimension = 0,
                    CollectionLetterCourse = "Collection",
                    CustInterest = 1
                },

                // ── PREPAYMENT Profile ──────────────────────────────────────────────
                new CustLedgerAccounts
                {
                    DataAreaId = "dat",
                    PostingProfile = "PrePayment",
                    AccountCode = AccountCode.All,
                    Num = "",
                    SummaryLedgerDimension = 0,
                    ClearingLedgerDimension = 0,
                    VatPrepaymentsLedgerDimension = 0,
                    LiabilitiesForDiscountLedgerDimension = 0,
                    WriteOffLedgerDimension = 0
                },

                // ── EXPORT Profile ──────────────────────────────────────────────────
                new CustLedgerAccounts
                {
                    DataAreaId = "dat",
                    PostingProfile = "EXPORT",
                    AccountCode = AccountCode.Group,
                    Num = "INT",
                    SummaryLedgerDimension = 0,
                    ClearingLedgerDimension = 0,
                    VatPrepaymentsLedgerDimension = 0,
                    LiabilitiesForDiscountLedgerDimension = 0,
                    WriteOffLedgerDimension = 0,
                    CollectionLetterCourse = "Collection",
                    CustInterest = 1
                },
                new CustLedgerAccounts
                {
                    DataAreaId = "dat",
                    PostingProfile = "EXPORT",
                    AccountCode = AccountCode.All,
                    Num = "",
                    SummaryLedgerDimension = 0,
                    ClearingLedgerDimension = 0,
                    VatPrepaymentsLedgerDimension = 0,
                    LiabilitiesForDiscountLedgerDimension = 0,
                    WriteOffLedgerDimension = 0,
                    CollectionLetterCourse = "Collection",
                    CustInterest = 1
                },

                // ── GOV Profile ─────────────────────────────────────────────────────
                new CustLedgerAccounts
                {
                    DataAreaId = "dat",
                    PostingProfile = "GOV",
                    AccountCode = AccountCode.Group,
                    Num = "Government",
                    SummaryLedgerDimension = 0,
                    ClearingLedgerDimension = 0,
                    VatPrepaymentsLedgerDimension = 0,
                    LiabilitiesForDiscountLedgerDimension = 0,
                    WriteOffLedgerDimension = 0,
                    CollectionLetterCourse = "Collection"
                },
                new CustLedgerAccounts
                {
                    DataAreaId = "dat",
                    PostingProfile = "GOV",
                    AccountCode = AccountCode.All,
                    Num = "",
                    SummaryLedgerDimension = 0,
                    ClearingLedgerDimension = 0,
                    VatPrepaymentsLedgerDimension = 0,
                    LiabilitiesForDiscountLedgerDimension = 0,
                    WriteOffLedgerDimension = 0,
                    CollectionLetterCourse = "Collection"
                },
            };

            var existingKeys = await db.CustLedgerAccounts
                .IgnoreQueryFilters()
                .Select(a => new { a.PostingProfile, a.AccountCode, a.Num })
                .ToListAsync(ct);

            var accountsToAdd = accountSeeds
                .Where(a => !existingKeys.Any(k =>
                    k.PostingProfile == a.PostingProfile &&
                    k.AccountCode    == a.AccountCode    &&
                    k.Num            == a.Num))
                .ToList();

            if (accountsToAdd.Any())
            {
                await db.CustLedgerAccounts.AddRangeAsync(accountsToAdd, ct);
                await db.SaveChangesAsync(ct);
            }
        }
    }
}
