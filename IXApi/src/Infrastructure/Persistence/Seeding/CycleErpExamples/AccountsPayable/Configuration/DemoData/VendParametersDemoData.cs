using IAX.IXApi.Modules.Finance.Entities;
using IAX.IXApi.Modules.Finance.Common;
using IAX.IXApi.Modules.Finance.AccountsPayable;
using IAX.IXApi.Infrastructure.Persistence.Seeding.CycleErpExamples.Services;
using Microsoft.EntityFrameworkCore;

namespace IAX.IXApi.Infrastructure.Persistence.Seeding.CycleErpExamples.AccountsPayable;

internal static class VendParametersDemoData
{
    internal static VendParameters Create() => new()
    {
        DataAreaId = "dat",
        Key = 0, VendPostingProfile = "DEMO",
        CreatedBy = "sys", OwnerAccountId = "sys"
    };
}
