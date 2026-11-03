using IAX.IXApi.Infrastructure.Persistence.Seeding.Core;
using IAX.IXApi.Modules.Finance.AccountsReceivable;
using IAX.IXApi.Modules.Finance.AccountsPayable;
using IAX.IXApi.Modules.Finance.Common;
using IAX.IXApi.Modules.Finance.Entities;
using IAX.IXApi.Modules.Finance.Inventory.DimensionGroups.Entities;
using IAX.IXApi.Modules.Administration.NumberSequences;
using Microsoft.EntityFrameworkCore;

using IAX.IXApi.Infrastructure.Persistence.Seeding.CycleErpExamples.Services;

namespace IAX.IXApi.Infrastructure.Persistence.Seeding.CycleErpExamples.AccountsPayable;

internal static class VendLedgerAccountsDemoData
{
    internal static VendLedgerAccounts Create(long vendorDimension) =>
        new VendLedgerAccounts { DataAreaId = "dat", PostingProfile = "DEMO", AccountCode = AccountCode.All,
                SummaryLedgerDimension = vendorDimension, Num = "" };
}
