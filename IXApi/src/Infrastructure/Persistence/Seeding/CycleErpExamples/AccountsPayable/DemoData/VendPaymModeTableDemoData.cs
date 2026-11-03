using IAX.IXApi.Modules.Finance.Entities;
using IAX.IXApi.Modules.Finance.Common;
using IAX.IXApi.Modules.Finance.AccountsPayable;
using IAX.IXApi.Infrastructure.Persistence.Seeding.CycleErpExamples.Services;
using Microsoft.EntityFrameworkCore;

namespace IAX.IXApi.Infrastructure.Persistence.Seeding.CycleErpExamples.AccountsPayable;

internal static class VendPaymModeTableDemoData
{
    internal static VendPaymModeTable Create(string code, string name) => new()
    {
        DataAreaId = "dat",
        PaymMode = code, Name = name, AccountType = LedgerJournalACType.Bank, PaymJournalNameId = "DEMO-AP",
        CreatedBy = "sys", OwnerAccountId = "sys"
    };
}
