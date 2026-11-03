using IAX.IXApi.Infrastructure.Persistence.Seeding.Core;
using IAX.IXApi.Modules.Finance.AccountsReceivable;
using IAX.IXApi.Modules.Finance.AccountsPayable;
using IAX.IXApi.Modules.Finance.Common;
using IAX.IXApi.Modules.Finance.Entities;
using IAX.IXApi.Modules.Finance.Inventory.DimensionGroups.Entities;
using IAX.IXApi.Modules.Administration.NumberSequences;
using Microsoft.EntityFrameworkCore;

namespace IAX.IXApi.Infrastructure.Persistence.Seeding.Modules.Finance;

public sealed partial class FinanceOperationalExamplesSeeder
{
    private static async Task SeedReturnSetupAsync(DbContext db, CancellationToken ct)
    {
        foreach (var (code, text) in new[] { ("DAMAGED", "Goods damaged in transit"), ("WRONG-ITEM", "Incorrect item supplied"), ("UNWANTED", "Customer changed their mind") })
            if (!await db.Set<ReturnReasonCode>().IgnoreQueryFilters().AnyAsync(x => x.DataAreaId == "dat" && x.ReasonCodeId == code, ct))
                db.Add(new ReturnReasonCode { ReasonCodeId = code, Description = text, DataAreaId = "dat" });
        foreach (var (code, action) in new[] { ("RESTOCK", DispositionAction.Credit), ("CREDIT-ONLY", DispositionAction.CreditOnly), ("SCRAP", DispositionAction.Scrap) })
            if (!await db.Set<ReturnDispositionCode>().IgnoreQueryFilters().AnyAsync(x => x.DataAreaId == "dat" && x.DispositionCodeId == code, ct))
                db.Add(new ReturnDispositionCode { DispositionCodeId = code, Description = "Example return: " + code, DispositionAction = action, DataAreaId = "dat" });
    }
}
