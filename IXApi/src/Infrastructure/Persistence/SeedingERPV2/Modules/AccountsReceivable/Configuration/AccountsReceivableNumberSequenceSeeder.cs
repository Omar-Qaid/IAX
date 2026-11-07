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
namespace IAX.IXApi.Infrastructure.Persistence.SeedingERPV2.Modules.AccountsReceivable;
public sealed class AccountsReceivableNumberSequenceSeeder : IDataSeeder
{
    public string Id => "ERPV2.AccountsReceivableNumberSequenceSeeder";
    public IReadOnlyCollection<string> Dependencies => [];
    public async Task SeedAsync(SeedContext c, CancellationToken ct)
    {
        // Voucher, LedgerJournalTable, InventDim and InventTransId are shared with the purchase
        // example and stay owned by its sequence seeder; only the sales documents are added here.
        foreach (var key in new[] { "ARV2-CUST", "ARV2-VOUCH", "ARV2-PAY", "SalesTable", "CustConfirmJour",
            "CustPackingSlipJour", "CustInvoiceTable", "CustInvoiceJour", "CustSettlement" })
        {
            var prefix = key switch { "SalesTable" => "AR2SO", "CustConfirmJour" => "AR2CF", "CustPackingSlipJour" => "AR2PS",
                "CustInvoiceTable" => "AR2FT", "CustInvoiceJour" => "AR2IV", "CustSettlement" => "AR2ST", _ => key };
            var sequence = await c.EnsureAsync<SysNumberSequence>(x => x.NumberSequence == key,
                new() { NumberSequence = key, Txt = "AR example vouchers", Format = prefix + "-######", AnnotatedFormat = prefix + "-{SEQ}",
                    Lowest = 1, Highest = 999999, NextRec = 1, InUse = 1, Blocked = 0, Manual = 0, NoIncrement = 0 }, ct,
                nameof(SysNumberSequence.Format), nameof(SysNumberSequence.AnnotatedFormat), nameof(SysNumberSequence.Lowest), nameof(SysNumberSequence.Highest), nameof(SysNumberSequence.InUse), nameof(SysNumberSequence.Blocked), nameof(SysNumberSequence.Manual), nameof(SysNumberSequence.NoIncrement));
            if ((sequence.NextRec ?? 0) < 1 || sequence.NextRec > sequence.Highest) throw new InvalidOperationException("AR voucher sequence is invalid or exhausted.");
        }
    }
}
