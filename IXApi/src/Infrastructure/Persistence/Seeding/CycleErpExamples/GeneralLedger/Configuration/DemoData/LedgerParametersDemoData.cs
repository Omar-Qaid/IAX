using IAX.IXApi.Modules.Finance.Entities;
using IAX.IXApi.Modules.Finance.Common;
using IAX.IXApi.Modules.Finance.AccountsPayable;
using IAX.IXApi.Infrastructure.Persistence.Seeding.CycleErpExamples.Services;
using Microsoft.EntityFrameworkCore;

namespace IAX.IXApi.Infrastructure.Persistence.Seeding.CycleErpExamples.GeneralLedger;

internal static class LedgerParametersDemoData
{
    internal static LedgerParameters Create() => new()
    {
        DataAreaId = "dat",
        Key_ = 0,
        CreatedBy = "sys", OwnerAccountId = "sys"
    };
}
