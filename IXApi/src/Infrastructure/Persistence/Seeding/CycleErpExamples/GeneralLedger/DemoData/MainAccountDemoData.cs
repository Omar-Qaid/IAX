using IAX.IXApi.Modules.Finance.Entities;
using IAX.IXApi.Modules.Finance.Common;
using IAX.IXApi.Modules.Finance.AccountsPayable;
using IAX.IXApi.Infrastructure.Persistence.Seeding.CycleErpExamples.Services;
using Microsoft.EntityFrameworkCore;

namespace IAX.IXApi.Infrastructure.Persistence.Seeding.CycleErpExamples.GeneralLedger;

internal static class MainAccountDemoData
{
    internal static MainAccount Create(string code, string name, MainAccountType type, long chart) => new()
    {
        DataAreaId = "dat",
        MainAccountId = code, Name = name, Type = type, LedgerChartOfAccounts = chart,
        CreatedBy = "sys", OwnerAccountId = "sys"
    };
}
