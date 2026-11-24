using IAX.IXApi.Infrastructure.Persistence.SeedingERPV2.Core.Models;
using IAX.IXApi.Modules.Finance.AccountsPayable;
using IAX.IXApi.Modules.Finance.Entities;

namespace IAX.IXApi.Infrastructure.Persistence.SeedingERPV2.Modules.AccountsPayable;

/// <summary>
/// The startup purchasing and payables setup, read from <c>Data/baseline-accountspayable.json</c>, the payable
/// mirror of the General ledger and Accounts receivable catalogs. The rows are flat reference data in their entity
/// shape; the seeders only add the keys that are missing. Posting profile accounts are left at 0 for the operator.
/// </summary>
public sealed record AccountsPayableBaselineCatalog(
    VendGroup[] VendGroups,
    VendLedger[] PostingProfiles,
    VendLedgerAccounts[] PostingProfileAccounts,
    VendPaymModeTable[] PaymentModes,
    LedgerJournalName[] JournalNames)
{
    public static AccountsPayableBaselineCatalog Load() =>
        BaselineManifest.Read<AccountsPayableBaselineCatalog>("baseline-accountspayable.json");
}
