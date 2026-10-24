using IAX.IXApi.Infrastructure.Persistence.Repositories;
using IAX.IXApi.Modules.Finance.Common;
using IAX.IXApi.Modules.Finance.Entities;
using IAX.IXApi.Shared.Application.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;
using System.Security.Cryptography;
using System.Text;

namespace IAX.IXApi.Modules.Finance.Inventory;

/// <summary>
/// Manual physical reservation engine (basic scope). Reservation allocates physically available
/// stock to a source's open demand by splitting its issue transactions between the Ordered and
/// ReservPhysical states, and maintaining InventSum.ReservPhysical. It never changes physical
/// on-hand, inventory value or the ledger (per D365 — reservation affects availability only).
/// </summary>
public sealed class InventoryReservationService : IInventoryReservationService
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly ICompanyExecutionContext _company;
    private DbContext Db => _unitOfWork.Context;

    public InventoryReservationService(IUnitOfWork unitOfWork, ICompanyExecutionContext company)
    {
        _unitOfWork = unitOfWork;
        _company = company;
    }

    public Task<(ReservationResultDto? Result, string? Error)> ReserveAsync(
        string inventTransId, decimal targetQuantity, bool allowPartial, CancellationToken ct) =>
        Db.Database.CreateExecutionStrategy().ExecuteAsync(() => ApplyAsync(inventTransId, targetQuantity, allowPartial, false, ct));

    public Task<(ReservationResultDto? Result, string? Error)> ReleaseAsync(
        string inventTransId, decimal quantity, CancellationToken ct) =>
        Db.Database.CreateExecutionStrategy().ExecuteAsync(() => ApplyAsync(inventTransId, quantity, false, true, ct));

    private async Task<(ReservationResultDto? Result, string? Error)> ApplyAsync(
        string inventTransId, decimal quantity, bool allowPartial, bool isRelease, CancellationToken ct)
    {
        var companyId = _company.GetDataAreaId();
        if (string.IsNullOrWhiteSpace(companyId)) return (null, "A company must be selected.");
        if (string.IsNullOrWhiteSpace(inventTransId)) return (null, "An inventory source identifier is required.");
        if (quantity <= 0)
            return (null, isRelease ? "The release quantity must be greater than zero."
                : "The reservation quantity must be greater than zero. Use release to lower a reservation.");

        await _unitOfWork.BeginTransactionAsync(ct);
        try
        {
            await LockPostingKeyAsync($"InventReservation:{companyId}:{inventTransId}", ct);
            var origin = await Db.Set<InventTransOrigin>().FirstOrDefaultAsync(
                x => x.DataAreaId == companyId && x.InventTransId == inventTransId, ct);
            if (origin == null) return await Fail("The inventory source was not found in the selected company.", ct);

            var transactions = await Db.Set<InventTrans>()
                .Where(x => x.DataAreaId == companyId && x.InventTransOrigin == origin.RecId
                    && (x.StatusIssue == StatusIssue.Ordered || x.StatusIssue == StatusIssue.ReservPhysical))
                .ToListAsync(ct);
            if (transactions.Count == 0)
                return await Fail("The inventory source has no open demand to reserve.", ct);

            var ordered = transactions.Where(x => x.StatusIssue == StatusIssue.Ordered).ToList();
            var reservedRows = transactions.Where(x => x.StatusIssue == StatusIssue.ReservPhysical).ToList();
            var openDemand = ordered.Sum(x => -x.Qty);          // currently unreserved
            var currentReserved = reservedRows.Sum(x => -x.Qty); // already reserved
            var dimId = transactions[0].InventDimId;

            var summary = await Db.Set<InventSum>().FirstOrDefaultAsync(
                x => x.DataAreaId == companyId && x.ItemId == origin.ItemId && x.InventDimId == dimId, ct);

            decimal delta;
            if (!isRelease)
            {
                // Set-target reserve: targetQuantity is the desired total reservation.
                var target = quantity;
                if (target < currentReserved)
                    return await Fail("The target is below the current reservation; use release to lower it.", ct);
                if (target > currentReserved + openDemand)
                    return await Fail("The reservation cannot exceed the source's remaining demand.", ct);
                delta = target - currentReserved;
                if (delta == 0)
                {
                    await _unitOfWork.CommitTransactionAsync(ct);
                    return (Result(inventTransId, origin.ItemId, openDemand + currentReserved, currentReserved, summary), null);
                }
                var available = summary == null ? 0m : summary.PhysicalInvent - summary.ReservPhysical;
                if (delta > available)
                {
                    if (!allowPartial)
                        return await Fail("There is not enough available physical stock to reserve the requested quantity.", ct);
                    delta = available;
                }
                if (delta <= 0)
                    return await Fail("There is no available physical stock to reserve.", ct);
                MoveToReserved(companyId, origin, ordered, reservedRows, delta);
            }
            else
            {
                if (quantity > currentReserved)
                    return await Fail("The release quantity exceeds the current reservation.", ct);
                delta = quantity;
                MoveToOpen(companyId, origin, ordered, reservedRows, delta);
            }

            if (summary != null)
            {
                // Reservation moves demand between the OnOrder and ReservPhysical buckets, mirroring
                // the Ordered <-> ReservPhysical transaction split; physical stock is unchanged.
                summary.OnOrder += isRelease ? delta : -delta;
                summary.ReservPhysical += isRelease ? -delta : delta;
                summary.AvailPhysical = summary.PhysicalInvent - summary.ReservPhysical;
                summary.AvailOrdered = summary.AvailPhysical + summary.Ordered - summary.OnOrder - summary.ReservOrdered;
                summary.LastUpdDatePhysical = DateTime.UtcNow;
            }

            await _unitOfWork.CompleteAsync(ct);
            await _unitOfWork.CommitTransactionAsync(ct);
            var newReserved = currentReserved + (isRelease ? -delta : delta);
            return (Result(inventTransId, origin.ItemId, openDemand + currentReserved, newReserved, summary), null);
        }
        catch
        {
            await _unitOfWork.RollbackTransactionAsync(ct);
            Db.ChangeTracker.Clear();
            throw;
        }
    }

    // Move `delta` units from Ordered rows into a ReservPhysical row (quantities are negative issues).
    private void MoveToReserved(string companyId, InventTransOrigin origin,
        List<InventTrans> ordered, List<InventTrans> reservedRows, decimal delta)
    {
        var remaining = delta;
        foreach (var row in ordered)
        {
            if (remaining <= 0) break;
            var open = -row.Qty;
            var take = Math.Min(open, remaining);
            row.Qty += take; // toward zero
            remaining -= take;
        }
        var reserved = reservedRows.FirstOrDefault();
        if (reserved == null)
        {
            var template = ordered[0];
            reserved = new InventTrans
            {
                DataAreaId = companyId, ItemId = origin.ItemId, InventDimId = template.InventDimId,
                InventTransOrigin = origin.RecId, Qty = -delta,
                StatusIssue = StatusIssue.ReservPhysical, StatusReceipt = StatusReceipt.None,
                ValueOpen = InventTransOpen.Yes, CurrencyCode = template.CurrencyCode,
                DateExpected = template.DateExpected, ShippingDateRequested = template.ShippingDateRequested
            };
            Db.Add(reserved);
        }
        else
        {
            reserved.Qty -= delta;
        }
    }

    // Move `delta` units from ReservPhysical rows back to open Ordered demand.
    private void MoveToOpen(string companyId, InventTransOrigin origin,
        List<InventTrans> ordered, List<InventTrans> reservedRows, decimal delta)
    {
        var remaining = delta;
        foreach (var row in reservedRows)
        {
            if (remaining <= 0) break;
            var reserved = -row.Qty;
            var give = Math.Min(reserved, remaining);
            row.Qty += give; // reserved toward zero
            remaining -= give;
        }
        var open = ordered.FirstOrDefault();
        if (open != null)
        {
            open.Qty -= delta; // add back to open demand
        }
        else
        {
            var template = reservedRows[0];
            Db.Add(new InventTrans
            {
                DataAreaId = companyId, ItemId = origin.ItemId, InventDimId = template.InventDimId,
                InventTransOrigin = origin.RecId, Qty = -delta,
                StatusIssue = StatusIssue.Ordered, StatusReceipt = StatusReceipt.None,
                ValueOpen = InventTransOpen.Yes, CurrencyCode = template.CurrencyCode,
                DateExpected = template.DateExpected, ShippingDateRequested = template.ShippingDateRequested
            });
        }
    }

    private static ReservationResultDto Result(string inventTransId, string itemId,
        decimal demand, decimal reserved, InventSum? summary) =>
        new(inventTransId, itemId, demand, reserved, summary == null ? 0m : summary.PhysicalInvent - summary.ReservPhysical);

    private async Task<(ReservationResultDto? Result, string? Error)> Fail(string message, CancellationToken ct)
    {
        await _unitOfWork.RollbackTransactionAsync(ct);
        Db.ChangeTracker.Clear();
        return (null, message);
    }

    private async Task LockPostingKeyAsync(string key, CancellationToken ct)
    {
        if (Db.Database.ProviderName?.Contains("SqlServer", StringComparison.OrdinalIgnoreCase) != true) return;
        var resource = Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes(key)));
        await using var command = Db.Database.GetDbConnection().CreateCommand();
        command.Transaction = Db.Database.CurrentTransaction!.GetDbTransaction();
        command.CommandText = "DECLARE @result int; EXEC @result = sys.sp_getapplock @Resource = @resource, @LockMode = 'Exclusive', @LockOwner = 'Transaction', @LockTimeout = 10000; SELECT @result;";
        var parameter = command.CreateParameter();
        parameter.ParameterName = "@resource";
        parameter.Value = "InventReservation:" + resource;
        command.Parameters.Add(parameter);
        if (await command.ExecuteScalarAsync(ct) is not int code || code < 0)
            throw new InvalidOperationException("Inventory reservation could not acquire its concurrency lock.");
    }
}
