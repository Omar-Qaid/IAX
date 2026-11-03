using IAX.IXApi.Modules.Finance.Entities;
using IAX.IXApi.Modules.Finance.Common;
using IAX.IXApi.Modules.Finance.AccountsPayable;
using IAX.IXApi.Infrastructure.Persistence.Seeding.CycleErpExamples.Services;
using Microsoft.EntityFrameworkCore;

namespace IAX.IXApi.Infrastructure.Persistence.Seeding.CycleErpExamples.GeneralLedger;

internal static class MainAccountLegalEntityDemoData
{
    internal static MainAccountLegalEntity Create(long account, long company) => new()
    {
        MainAccount = account, LegalEntity = company,
        CreatedBy = "sys", OwnerAccountId = "sys"
    };
}
