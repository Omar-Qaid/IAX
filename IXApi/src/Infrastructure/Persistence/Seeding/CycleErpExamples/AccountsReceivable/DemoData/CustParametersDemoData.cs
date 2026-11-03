using IAX.IXApi.Infrastructure.Persistence.Seeding.Core;
using IAX.IXApi.Modules.Finance.AccountsReceivable;
using IAX.IXApi.Modules.Finance.AccountsPayable;
using IAX.IXApi.Modules.Finance.Common;
using IAX.IXApi.Modules.Finance.Entities;
using IAX.IXApi.Modules.Finance.Inventory.DimensionGroups.Entities;
using IAX.IXApi.Modules.Administration.NumberSequences;
using Microsoft.EntityFrameworkCore;

using IAX.IXApi.Infrastructure.Persistence.Seeding.CycleErpExamples.Services;

namespace IAX.IXApi.Infrastructure.Persistence.Seeding.CycleErpExamples.AccountsReceivable;

internal static class CustParametersDemoData
{
    internal static CustParameters Create() =>
        new CustParameters { DataAreaId = "dat", CurrencyCode = "SAR", CustPostingProfile = "GEN",
                PaymTermId = "Monthly", PaymMode = "Transfer", DlvMode = "DHL", TaxGroup = "DOM",
                PriceDiscJournalNamePrice = "SALEPRICE", PriceDiscJournalNameLineDisc = "LINEDISC", PriceDiscJournalNameTotalDisc = "TOTALDISC" };
}
