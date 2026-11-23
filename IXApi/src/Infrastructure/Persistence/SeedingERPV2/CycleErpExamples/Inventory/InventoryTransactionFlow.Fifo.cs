using IAX.IXApi.Infrastructure.Persistence.SeedingERPV2.CycleErpExamples.Inventory.Data;
using IAX.IXApi.Infrastructure.Persistence.SeedingERPV2.CycleErpExamples.Inventory.Reports;
using IAX.IXApi.Infrastructure.Persistence.SeedingERPV2.CycleErpExamples.Inventory.Validation;
using IAX.IXApi.Infrastructure.Persistence.SeedingERPV2.Modules.Inventory;
using IAX.IXApi.Modules.Finance.AccountsPayable;
using IAX.IXApi.Modules.Finance.AccountsReceivable.Invoice;
using IAX.IXApi.Modules.Finance.AccountsReceivable.PackingSlip.Dtos;
using IAX.IXApi.Modules.Finance.AccountsReceivable.PackingSlip;
using IAX.IXApi.Modules.Finance.AccountsReceivable.SalesOrder.Interfaces;
using IAX.IXApi.Modules.Finance.AccountsReceivable;
using IAX.IXApi.Modules.Finance.Common;
using IAX.IXApi.Modules.Finance.Entities;
using IAX.IXApi.Modules.Finance.Inventory;
using Microsoft.EntityFrameworkCore;
using System.Globalization;
namespace IAX.IXApi.Infrastructure.Persistence.SeedingERPV2.CycleErpExamples.Inventory;

public sealed partial class InventoryTransactionFlow
{
    /// <summary>
    /// INV-21 of the scenario catalog. Buy 100 at 60 and 100 at 66, sell 150 at the running average of 63
    /// (cost of goods sold 9,450), then close. FIFO costs the 150 at 100 × 60 + 50 × 66 = 9,300, so the close
    /// adjusts the issues by +150 (debit inventory, credit cost of goods sold), leaving 50 at 66 = 3,300.
    /// </summary>
    private async Task<InventoryScenarioReport> FifoCloseCycleAsync(InventoryTransactionOptions options, CancellationToken ct)
    {
        const string id = "10";
        var item = InventoryCycleMasterSeeder.Item(options.FiscalYear, InventoryCycleSeedExample.FifoSuffix);
        var service = ExampleIdentifiers.ServiceItem(options.FiscalYear);
        var key = ExampleIdentifiers.ScenarioKey(options.FiscalYear, id);
        var date = new DateTime(options.FiscalYear, 11, 1);
        var closeDate = date.AddDays(29);

        var alreadyClosed = await db.Set<InventClosing>().AnyAsync(x => x.DataAreaId == Area && x.ScopeItemId == item
            && x.TransDate == closeDate && x.Cancellation == NoYes.No && x.CancelClosing == NoYes.No
            && x.RunRecalculation == NoYes.No, ct);
        if (!alreadyClosed)
        {
            await BuyAsync(key + "-P1", item, 100m, 60m, date, ct);
            await BuyAsync(key + "-P2", item, 100m, 66m, date.AddDays(1), ct);

            var customer = ExampleIdentifiers.Customer(Area);
            var order = await db.Set<SalesTable>().SingleOrDefaultAsync(x => x.DataAreaId == Area && x.SalesName == key, ct);
            if (order is null)
            {
                var created = await sales.CreateAsync(new SalesOrderQuickCreateDto
                {
                    CustomerAccount = customer, CurrencyCode = "SAR", SalesName = key,
                    InventSiteId = ExampleIdentifiers.Site, InventLocationId = ExampleIdentifiers.Warehouse,
                    RequestedShipDate = date.AddDays(4), RequestedReceiptDate = date.AddDays(4)
                }, ct);
                Check(created.Error is null && created.Order is not null, created.Error ?? "Sales order creation failed.");
                order = await db.Set<SalesTable>().SingleAsync(x => x.DataAreaId == Area && x.RecId == created.Order!.RecId, ct);
            }
            Check(order.CustAccount == customer && order.CurrencyCode == "SAR", "Existing FIFO sales order differs.");
            var line = await EnsureSalesLineAsync(order, item, 150m, 90m, date, ct);
            await DeliverAsync(order, key + "-D1", line, 150m, date.AddDays(5), ct);
            await InvoiceAsync(order, key + "-I1", [new(line.RecId, 150m)], date.AddDays(6), ct);
            await CheckSoldAsync(item, 150m, ct);

            // Before the close the issue carries the running average of 63.
            var before = await db.Set<InventTrans>().Where(x => x.DataAreaId == Area && x.ItemId == item).ToListAsync(ct);
            var issued = before.Where(x => x.StatusIssue is StatusIssue.Deducted or StatusIssue.Sold)
                .Sum(x => x.CostAmountPosted + x.CostAmountAdjustment);
            Check(issued == -9450m, $"Pre-close issue cost is {issued} instead of -9,450.");
            stages.Add(new("CST-FIFO-01", "Pre-close running cost", item, 150m, 150m, -9450m, issued, [], "Passed"));

            var result = await closing.CloseItemAsync(item, closeDate, ct);
            Check(result.Error is null, result.Error ?? "FIFO close failed.");
        }

        var movements = await db.Set<InventTrans>().Where(x => x.DataAreaId == Area && x.ItemId == item).ToListAsync(ct);
        var closedIssue = movements.Where(x => x.StatusIssue is StatusIssue.Deducted or StatusIssue.Sold)
            .Sum(x => x.CostAmountPosted + x.CostAmountAdjustment);
        Check(closedIssue == -9300m, $"FIFO issue cost is {closedIssue} instead of -9,300.");
        stages.Add(new("CST-FIFO-02", "FIFO close adjustment", item, null, null, -9300m, closedIssue, [], "Passed"));
        return await ReconcileAsync(id, item, service, 50m, 3300m, 9300m, 150m, 0m, ct);
    }
}
