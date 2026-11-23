using IAX.IXApi.Infrastructure.Persistence.SeedingERPV2.Core.Models;
using IAX.IXApi.Modules.Finance.AccountsReceivable;
using IAX.IXApi.Modules.Finance.Entities;

namespace IAX.IXApi.Infrastructure.Persistence.SeedingERPV2.Modules.AccountsReceivable;

/// <summary>
/// The startup sales and receivables setup, read from <c>Data/baseline-accountsreceivable.json</c>. The rows are
/// flat reference data in their entity shape; the seeders only add the keys that are missing.
/// </summary>
public sealed record AccountsReceivableBaselineCatalog(
    CustGroup[] CustGroups,
    SalesPool[] SalesPools,
    PriceDiscAdmName[] PriceDiscJournalNames,
    PriceDiscGroup[] PriceDiscGroups,
    PriceDiscAdmTable[] PriceDiscJournals,
    DlvMode[] DeliveryModes,
    DlvTerm[] DeliveryTerms,
    PaymTerm[] PaymentTerms,
    PaymSched[] PaymentSchedules,
    PaymSchedLine[] PaymentScheduleLines,
    MarkupTable[] Charges,
    CustPaymModeTable[] PaymentModes,
    CustLedger[] PostingProfiles,
    CustLedgerAccounts[] PostingProfileAccounts)
{
    public static AccountsReceivableBaselineCatalog Load() =>
        BaselineManifest.Read<AccountsReceivableBaselineCatalog>("baseline-accountsreceivable.json");
}
