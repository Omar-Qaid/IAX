using IAX.IXApi.Modules.Finance.Entities;
using IAX.IXApi.Modules.Finance.Common;
using IAX.IXApi.Modules.Finance.AccountsPayable;
using IAX.IXApi.Infrastructure.Persistence.Seeding.CycleErpExamples.Services;
using Microsoft.EntityFrameworkCore;

namespace IAX.IXApi.Infrastructure.Persistence.Seeding.CycleErpExamples.GeneralLedger;

internal static class CashDiscDemoData
{
    internal static CashDisc Create(long dimension) => new()
    {
        DataAreaId = "dat",
        CashDiscCode = "ERP2P10", Description = "ERP example: 2 percent within 10 days", NumOfDays = 10, DiscPct = 2m, CashDiscLedgerDimension = dimension,
        CreatedBy = "sys", OwnerAccountId = "sys"
    };
}
