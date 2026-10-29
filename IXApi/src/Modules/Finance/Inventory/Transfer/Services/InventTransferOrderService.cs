using IAX.IXApi.Infrastructure.Persistence.Repositories;
using IAX.IXApi.Modules.Finance.Common;
using IAX.IXApi.Modules.Finance.Entities;
using IAX.IXApi.Shared.Application.Identity;
using Microsoft.EntityFrameworkCore;

namespace IAX.IXApi.Modules.Finance.Inventory;

public sealed record TransferOrderInput(string TransferId, string ItemId, string Unit, decimal Quantity,
    string SiteId, string FromWarehouse, string TransitWarehouse, string ToWarehouse, string JournalNameId);
public sealed record TransferPostingInput(string RequestId, decimal Quantity, DateTime Date);
public interface IInventTransferOrderService
{
    Task<(InventTransferOrder? Order, string? Error)> CreateAsync(TransferOrderInput input, CancellationToken ct);
    Task<(InventTransferOrder? Order, string? Error)> PostAsync(string transferId, TransferPostingInput input, bool receive, CancellationToken ct);
    Task<InventTransferOrder?> GetAsync(string transferId, CancellationToken ct);
}

public sealed class InventTransferOrderService(IUnitOfWork work, ICompanyExecutionContext company,
    IInventJournalService journals, IInventJournalPostingService posting) : IInventTransferOrderService
{
    private DbContext Db => work.Context;
    public Task<InventTransferOrder?> GetAsync(string transferId, CancellationToken ct) => Db.Set<InventTransferOrder>().AsNoTracking()
        .SingleOrDefaultAsync(x => x.DataAreaId == company.GetDataAreaId() && x.TransferId == transferId, ct);

    public Task<(InventTransferOrder? Order, string? Error)> CreateAsync(TransferOrderInput input, CancellationToken ct) => ExecuteAsync(async area =>
    {
        if (string.IsNullOrWhiteSpace(input.TransferId) || input.TransferId.Length > 40 || input.Quantity <= 0 || decimal.Round(input.Quantity, 4) != input.Quantity)
            throw new InvalidOperationException("A transfer identifier (maximum 40 characters) and positive inventory quantity are required.");
        await LockAsync(area, input.TransferId, ct);
        var prior = await Db.Set<InventTransferOrder>().SingleOrDefaultAsync(x => x.DataAreaId == area && x.TransferId == input.TransferId, ct);
        if (prior != null)
        {
            if (prior.ItemId != input.ItemId || prior.Unit != input.Unit || prior.Quantity != input.Quantity || prior.SiteId != input.SiteId
                || prior.FromWarehouse != input.FromWarehouse || prior.TransitWarehouse != input.TransitWarehouse || prior.ToWarehouse != input.ToWarehouse
                || prior.JournalNameId != input.JournalNameId)
                throw new InvalidOperationException("The transfer identifier already belongs to a different request.");
            return prior;
        }
        if (new[] { input.FromWarehouse, input.TransitWarehouse, input.ToWarehouse }.Distinct().Count() != 3)
            throw new InvalidOperationException("Source, transit and destination must be distinct warehouses.");
        foreach (var id in new[] { input.FromWarehouse, input.TransitWarehouse, input.ToWarehouse })
        {
            var warehouse = await Db.Set<InventLocation>().SingleOrDefaultAsync(x => x.DataAreaId == area && x.InventLocationId == id && x.InventSiteId == input.SiteId, ct);
            if (warehouse == null || warehouse.InventLocationType != (id == input.TransitWarehouse ? InventLocationType.Transit : InventLocationType.Standard))
                throw new InvalidOperationException("A basic transfer requires standard source/destination warehouses and a transit warehouse in the same site.");
        }
        if (!await Db.Set<IAX.IXApi.Modules.Finance.Entities.InventTable>().AnyAsync(x => x.DataAreaId == area && x.ItemId == input.ItemId, ct)
            || !await Db.Set<InventTableModule>().AnyAsync(x => x.DataAreaId == area && x.ItemId == input.ItemId && x.ModuleType == ModuleInventPurchSales.Inventory && x.UnitId == input.Unit, ct))
            throw new InvalidOperationException("Select a released item and its inventory unit.");
        if (!await Db.Set<InventJournalName>().AnyAsync(x => x.DataAreaId == area && x.JournalNameId == input.JournalNameId && x.JournalType == InventJournalType.Transfer, ct))
            throw new InvalidOperationException("A transfer journal name is required.");
        var order = new InventTransferOrder { DataAreaId = area, TransferId = input.TransferId, ItemId = input.ItemId, Unit = input.Unit,
            Quantity = input.Quantity, SiteId = input.SiteId, FromWarehouse = input.FromWarehouse, TransitWarehouse = input.TransitWarehouse,
            ToWarehouse = input.ToWarehouse, JournalNameId = input.JournalNameId };
        Db.Add(order);
        return order;
    }, ct);

    public Task<(InventTransferOrder? Order, string? Error)> PostAsync(string transferId, TransferPostingInput input, bool receive, CancellationToken ct) => ExecuteAsync(async area =>
    {
        if (string.IsNullOrWhiteSpace(input.RequestId) || input.RequestId.Length > 40 || input.Quantity <= 0 || decimal.Round(input.Quantity, 4) != input.Quantity || input.Date == default)
            throw new InvalidOperationException("A posting request identifier, date and positive inventory quantity are required.");
        await LockAsync(area, transferId, ct);
        var order = await Db.Set<InventTransferOrder>().SingleOrDefaultAsync(x => x.DataAreaId == area && x.TransferId == transferId, ct)
            ?? throw new InvalidOperationException("Transfer order was not found in the selected company.");
        var prior = await Db.Set<InventTransferPosting>().SingleOrDefaultAsync(x => x.DataAreaId == area && x.TransferOrderRecId == order.RecId && x.RequestId == input.RequestId, ct);
        if (prior != null)
        {
            if (prior.IsReceipt != receive || prior.Quantity != input.Quantity || prior.PostingDate != input.Date.Date)
                throw new InvalidOperationException("The posting request identifier already belongs to a different operation.");
            return order;
        }
        if (input.Quantity > (receive ? order.Shipped - order.Received : order.Quantity - order.Shipped))
            throw new InvalidOperationException(receive ? "Receipt exceeds this order's stock in transit." : "Shipment exceeds the remaining transfer quantity.");
        var latest = await Db.Set<InventTransferPosting>().Where(x => x.DataAreaId == area && x.TransferOrderRecId == order.RecId)
            .Select(x => (DateTime?)x.PostingDate).MaxAsync(ct);
        if (latest.HasValue && input.Date.Date < latest.Value)
            throw new InvalidOperationException("Transfer postings must follow their existing posting dates.");
        var from = receive ? order.TransitWarehouse : order.FromWarehouse;
        var to = receive ? order.ToWarehouse : order.TransitWarehouse;
        var created = await journals.CreateAsync(new InventJournalCreateDto { JournalNameId = order.JournalNameId,
            Description = $"Transfer {transferId}: {input.RequestId}", InventSiteId = order.SiteId, InventLocationId = from }, ct);
        if (created.Error != null) throw new InvalidOperationException(created.Error);
        var added = await journals.AddLineAsync(created.Journal!.JournalId, new InventJournalLineInputDto { ItemId = order.ItemId, Unit = order.Unit,
            Qty = input.Quantity, TransDate = input.Date.Date, InventSiteId = order.SiteId, InventLocationId = from,
            ToInventSiteId = order.SiteId, ToInventLocationId = to }, ct);
        if (added.Error != null) throw new InvalidOperationException(added.Error);
        var operation = new InventTransferPosting { DataAreaId = area, TransferOrderRecId = order.RecId, RequestId = input.RequestId,
            JournalId = created.Journal.JournalId, IsReceipt = receive, Quantity = input.Quantity, PostingDate = input.Date.Date };
        if (receive)
        {
            var history = await Db.Set<InventTransferPosting>().Where(x => x.DataAreaId == area && x.TransferOrderRecId == order.RecId).ToListAsync(ct);
            var outstandingCost = history.Sum(x => x.IsReceipt ? -x.CostAmount : x.CostAmount);
            operation.CostAmount = decimal.Round(outstandingCost * input.Quantity / (order.Shipped - order.Received), 4);
            (await Db.Set<InventJournalTable>().SingleAsync(x => x.DataAreaId == area && x.JournalId == operation.JournalId, ct)).Source = "TransferReceipt";
            Db.Add(operation);
            await Db.SaveChangesAsync(ct);
        }
        var posted = await posting.PostAsync(created.Journal.JournalId, ct);
        if (posted.Error != null) throw new InvalidOperationException(posted.Error);
        if (receive) order.Received += input.Quantity; else order.Shipped += input.Quantity;
        if (!receive)
        {
            operation.CostAmount = await Db.Set<InventJournalTrans>().Where(x => x.DataAreaId == area && x.JournalId == operation.JournalId).SumAsync(x => x.CostAmount, ct);
            Db.Add(operation);
        }
        return order;
    }, ct);

    private async Task LockAsync(string area, string id, CancellationToken ct)
    {
        // Uses the shared transactional lock implementation with a separate document key space.
        var error = await InventoryCountProtection.CheckAsync(Db, area, "TransferOrder:" + id, "", ct);
        if (error != null) throw new InvalidOperationException(error);
    }

    private Task<(InventTransferOrder? Order, string? Error)> ExecuteAsync(Func<string, Task<InventTransferOrder>> action, CancellationToken ct) =>
        Db.Database.CreateExecutionStrategy().ExecuteAsync(async () =>
        {
            var area = company.GetDataAreaId();
            if (string.IsNullOrWhiteSpace(area)) return ((InventTransferOrder?)null, (string?)"A company must be selected.");
            // Own the outer transaction; journal services must not commit an incomplete transfer operation.
            await using var tx = await Db.Database.BeginTransactionAsync(ct);
            try
            {
                var order = await action(area);
                await Db.SaveChangesAsync(ct);
                await tx.CommitAsync(ct);
                return ((InventTransferOrder?)order, (string?)null);
            }
            catch (InvalidOperationException ex)
            {
                await tx.RollbackAsync(ct);
                Db.ChangeTracker.Clear();
                return ((InventTransferOrder?)null, (string?)ex.Message);
            }
            catch
            {
                await tx.RollbackAsync(ct);
                Db.ChangeTracker.Clear();
                throw;
            }
        });
}
