using IAX.IXApi.Infrastructure.Persistence.Seeding.Core;
using IAX.IXApi.Modules.Finance.AccountsReceivable;
using IAX.IXApi.Modules.Finance.AccountsPayable;
using IAX.IXApi.Modules.Finance.Common;
using IAX.IXApi.Modules.Finance.Entities;
using IAX.IXApi.Modules.Finance.Inventory.DimensionGroups.Entities;
using IAX.IXApi.Modules.Administration.NumberSequences;
using Microsoft.EntityFrameworkCore;

using IAX.IXApi.Infrastructure.Persistence.Seeding.CycleErpExamples.Services;

namespace IAX.IXApi.Infrastructure.Persistence.Seeding.CycleErpExamples.GeneralLedger;

internal static class BankAccountTableDemoData
{
    internal static BankAccountTable Create(long bankDimension) =>
        new BankAccountTable { AccountId = "DEMO-SAR", AccountNum = "TRAINING-001", Name = "Example SAR operating account",
                BankGroupId = "DEMO", CurrencyCode = "SAR", LedgerDimension = bankDimension,
                BankAccountStatus = BankAccountStatus.Active, ActiveFrom = new DateTime(2026,1,1), ActiveTo = new DateTime(2099,12,31),
                CustPaymentJournalName = "DEMO-AR", VendPaymentJournalName = "DEMO-AP", DataAreaId = "dat" };
}
