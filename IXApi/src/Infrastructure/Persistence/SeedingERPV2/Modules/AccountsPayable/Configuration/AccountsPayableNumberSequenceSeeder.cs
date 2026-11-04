using IAX.IXApi.Infrastructure.Persistence.SeedingERPV2.Core.Contracts;
using IAX.IXApi.Infrastructure.Persistence.SeedingERPV2.Core.Models;
using IAX.IXApi.Modules.Finance.Entities;
using IAX.IXApi.Modules.Finance.Common;
using IAX.IXApi.Modules.Finance.AccountsPayable;
using IAX.IXApi.Modules.Finance.AccountsReceivable;
using IAX.IXApi.Modules.Administration.NumberSequences;
using IAX.IXApi.Modules.Finance.Inventory.InventTable;
using IAX.IXApi.Modules.Finance.Inventory.ProductCategories.Entities;
using IAX.IXApi.Modules.Finance.Foundation.Dimensions.LedgerDimension;
using IAX.IXApi.Modules.Finance.Foundation.Dimensions.AccountStructure;
using Microsoft.EntityFrameworkCore;
namespace IAX.IXApi.Infrastructure.Persistence.SeedingERPV2.Modules.AccountsPayable;
public sealed class AccountsPayableNumberSequenceSeeder : IDataSeeder
{
    public string Id => "ERPV2.AccountsPayableNumberSequenceSeeder";
    public IReadOnlyCollection<string> Dependencies => [];
    public async Task SeedAsync(SeedContext c, CancellationToken ct)
    {

        foreach (var key in new[] { "APV2-VEND", "APV2-PURCH", "PurchTable", "VendPurchOrderJour", "VendPackingSlipJour", "VendInvoiceInfoTable", "VendInvoiceJour", "LedgerJournalTable", "Voucher", "InventDim", "InventTransId", "ReturnItemNum" })
        {
            var prefix = key switch { "PurchTable" => "AP2PO", "VendPurchOrderJour" => "AP2CF", "VendPackingSlipJour" => "AP2RC",
                "VendInvoiceInfoTable" => "AP2PI", "VendInvoiceJour" => "AP2IV", "LedgerJournalTable" => "AP2JN",
                "Voucher" => "AP2V", "InventDim" => "AP2DM", "InventTransId" => "AP2IT", "ReturnItemNum" => "AP2RT", _ => key };
            var sequence = await c.EnsureAsync<SysNumberSequence>(x => x.NumberSequence == key,
                new() { NumberSequence = key, Txt = "AP example vouchers", Format = prefix + "-######", AnnotatedFormat = prefix + "-{SEQ}",
                    Lowest = 1, Highest = 999999, NextRec = 1, InUse = 1, Blocked = 0, Manual = 0, NoIncrement = 0 }, ct,
                nameof(SysNumberSequence.Format), nameof(SysNumberSequence.AnnotatedFormat), nameof(SysNumberSequence.Lowest), nameof(SysNumberSequence.Highest), nameof(SysNumberSequence.InUse), nameof(SysNumberSequence.Blocked), nameof(SysNumberSequence.Manual), nameof(SysNumberSequence.NoIncrement));
            if ((sequence.NextRec ?? 0) < 1 || sequence.NextRec > sequence.Highest) throw new InvalidOperationException("AP voucher sequence is invalid or exhausted.");
        }
    }
}
