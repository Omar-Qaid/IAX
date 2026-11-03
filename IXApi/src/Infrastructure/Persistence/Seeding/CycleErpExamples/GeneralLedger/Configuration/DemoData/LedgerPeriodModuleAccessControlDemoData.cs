using IAX.IXApi.Modules.Finance.Entities;
using IAX.IXApi.Modules.Finance.Common;
using IAX.IXApi.Modules.Finance.AccountsPayable;
using IAX.IXApi.Infrastructure.Persistence.Seeding.CycleErpExamples.Services;
using Microsoft.EntityFrameworkCore;

namespace IAX.IXApi.Infrastructure.Persistence.Seeding.CycleErpExamples.GeneralLedger;

internal static class LedgerPeriodModuleAccessControlDemoData
{
    internal static LedgerPeriodModuleAccessControl Create(long period) => new()
    {
        DataAreaId = "dat",
        LedgerFiscalCalendarPeriod = period, LedgerAccessLevel = LedgerPeriodAccessLevel.All, TaxAccessLevel = LedgerPeriodAccessLevel.All, BankAccessLevel = LedgerPeriodAccessLevel.All, CustAccessLevel = LedgerPeriodAccessLevel.All, VendAccessLevel = LedgerPeriodAccessLevel.All, InventAccessLevel = LedgerPeriodAccessLevel.All,
        CreatedBy = "sys", OwnerAccountId = "sys"
    };
}
