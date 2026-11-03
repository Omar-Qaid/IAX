using IAX.IXApi.Modules.Finance.Entities;
using IAX.IXApi.Modules.Finance.Common;
using IAX.IXApi.Modules.Finance.AccountsPayable;
using IAX.IXApi.Infrastructure.Persistence.Seeding.CycleErpExamples.Services;
using Microsoft.EntityFrameworkCore;

namespace IAX.IXApi.Infrastructure.Persistence.Seeding.CycleErpExamples.GeneralLedger;

internal static class CurrencyLedgerGainLossAccountDemoData
{
    internal static CurrencyLedgerGainLossAccount Create(long ledger, CurrencyGainLossAccountType type, long dimension) => new()
    {
        DataAreaId = "dat",
        Ledger = ledger, CurrencyCode = "USD", AccountType = type, LedgerDimension = dimension,
        CreatedBy = "sys", OwnerAccountId = "sys"
    };
}
