using IAX.IXApi.Infrastructure.Persistence.SeedingERPV2.CycleErpExamples.Inventory.Data;
using IAX.IXApi.Infrastructure.Persistence.SeedingERPV2.CycleErpExamples.Inventory.Reports;
using IAX.IXApi.Infrastructure.Persistence.SeedingERPV2.Modules.Inventory;
using IAX.IXApi.Modules.Finance.Entities;
using IAX.IXApi.Modules.Finance.Inventory.Transfer;
using Microsoft.EntityFrameworkCore;

namespace IAX.IXApi.Infrastructure.Persistence.SeedingERPV2.CycleErpExamples.Inventory;

public sealed partial class InventoryTransactionFlow
{
    private async Task<InventoryScenarioReport> TransferCycleAsync(InventoryTransactionOptions options, CancellationToken ct)
    {
        const string id = "06";
        const string transit = "APV2-TRN", destination = "APV2-WH2";
        var item = InventoryCycleMasterSeeder.Item(options.FiscalYear, InventoryCycleSeedExample.TransferSuffix);
        var key = ExampleIdentifiers.ScenarioKey(options.FiscalYear, id);
        var date = new DateTime(options.FiscalYear, 11, 1);
        await AdjustmentAsync(key + "-OPEN", item, 90m, 12m, date, ct);
        var lookup = await transfers.GetAsync(key, ct);
        Check(lookup.Error is null, lookup.Error ?? "Transfer lookup failed.");
        var order = lookup.Order;
        if (order is null)
        {
            var created = await transfers.CreateAsync(new(key, ExampleIdentifiers.Site, ExampleIdentifiers.Warehouse,
                transit, destination, date.AddDays(1), date.AddDays(2), [new(item, ExampleIdentifiers.Unit, 10m)]), ct);
            Check(created.Error is null && created.Order is not null, created.Error ?? "Transfer creation failed.");
            order = created.Order!;
        }
        Check(order.InventLocationIdFrom == ExampleIdentifiers.Warehouse && order.InventLocationIdTransit == transit
            && order.InventLocationIdTo == destination && order.Lines.Count == 1, "Existing transfer header differs.");
        var line = order.Lines[0];
        Check(line.ItemId == item && line.UnitId == ExampleIdentifiers.Unit && line.QtyTransfer == 10m, "Existing transfer line differs.");
        async Task<InventTransferOrderDto> Post(bool receive, string request, decimal qty, DateTime postingDate)
        {
            var input = new InventTransferPostDto(request, postingDate, new Dictionary<decimal, decimal> { [line.LineNum] = qty });
            var result = receive ? await transfers.ReceiveAsync(key, input, ct) : await transfers.ShipAsync(key, input, ct);
            Check(result.Error is null && result.Order is not null, result.Error ?? "Transfer posting failed.");
            return result.Order!;
        }
        if (line.QtyShipped == 0)
        {
            order = await Post(false, key + "-SHIP", 10m, date.AddDays(1));
            line = order.Lines[0];
            await VerifyTransferBalancesAsync(item, 80m, 10m, 0m, ct);
            stages.Add(new("TRF-02", "Ship 10 to transit", key, 10m, line.QtyShipped, 120m, 120m, [], "Passed"));
        }
        Check(line.QtyShipped == 10m, "Existing transfer shipped quantity differs.");
        if (line.QtyReceived == 0)
        {
            order = await Post(true, key + "-RECEIVE6", 6m, date.AddDays(2));
            line = order.Lines[0];
            await VerifyTransferBalancesAsync(item, 80m, 4m, 6m, ct);
            stages.Add(new("TRF-02", "Receive 6; retain 4 in transit", key, 6m, line.QtyReceived, 72m, 72m, [], "Passed"));
        }
        if (line.QtyReceived == 6m)
        {
            order = await Post(true, key + "-RECEIVE4", 4m, date.AddDays(3));
            line = order.Lines[0];
        }
        Check(line.QtyReceived == 10m && line.QtyRemainShip == 0m && line.QtyRemainReceive == 0m,
            "Transfer must finish fully shipped and received.");
        await VerifyTransferBalancesAsync(item, 80m, 0m, 10m, ct);
        var journalsPosted = await db.Set<InventTransferJour>().Where(x => x.DataAreaId == Area && x.TransferId == key).ToListAsync(ct);
        Check(journalsPosted.Count == 3, "Transfer must retain one shipment and two receipt journals.");
        stages.Add(new("TRF-02", "Final receipt; transit cleared", key, 10m, line.QtyReceived, 120m, 120m,
            journalsPosted.Select(x => x.Voucher).ToArray(), "Passed"));
        return await ReconcileAsync(id, item, ExampleIdentifiers.ServiceItem(options.FiscalYear), 90m, 1080m, 0m, 0m, 0m, ct);
    }

    private async Task VerifyTransferBalancesAsync(string item, decimal source, decimal transit, decimal destination, CancellationToken ct)
    {
        var balances = await (from sum in db.Set<InventSum>()
            join dim in db.Set<InventDim>() on new { sum.DataAreaId, sum.InventDimId } equals new { dim.DataAreaId, dim.InventDimId }
            where sum.DataAreaId == Area && sum.ItemId == item
            select new { dim.InventLocationId, sum.PhysicalInvent, sum.PostedValue }).ToListAsync(ct);
        foreach (var (warehouse, qty) in new[] { (ExampleIdentifiers.Warehouse, source), ("APV2-TRN", transit), ("APV2-WH2", destination) })
        {
            Check(balances.Where(x => x.InventLocationId == warehouse).Sum(x => x.PhysicalInvent) == qty,
                $"Transfer on-hand differs at {warehouse}.");
            Check(balances.Where(x => x.InventLocationId == warehouse).Sum(x => x.PostedValue) == qty * 12m,
                $"Transfer value differs at {warehouse}.");
        }
        Check(balances.Sum(x => x.PhysicalInvent) == 90m && balances.Sum(x => x.PostedValue) == 1080m,
            "Transfer must conserve site quantity and value.");
    }
}
