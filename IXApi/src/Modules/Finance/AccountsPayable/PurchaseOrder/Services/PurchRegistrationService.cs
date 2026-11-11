using IAX.IXApi.Infrastructure.Identity;
using IAX.IXApi.Infrastructure.Persistence.Repositories;
using IAX.IXApi.Modules.Finance.Common;
using IAX.IXApi.Modules.Finance.Entities;
using IAX.IXApi.Shared.Application.Identity;
using Microsoft.EntityFrameworkCore;

namespace IAX.IXApi.Modules.Finance.AccountsPayable;

/// <summary>
/// Arrival registration: the first step of a two-step receipt, from <c>Logic_Readonly.md</c> §10.
/// </summary>
/// <remarks>
/// <para>
/// <b>Nothing is posted.</b> The specification is explicit that registration is not the equivalent
/// of the product receipt entry: there is no ledger voucher, no vendor transaction and no physical
/// value. Ordered inventory becomes <see cref="StatusReceipt.Registered"/> and the quantity shows in
/// <c>InventSum.Registered</c>, its own bucket, rather than in physical on-hand - §10 puts the
/// registered quantity "within the registration quantities", and §9 notes availability of a
/// registered quantity depends on status, dimensions and setup, so it is deliberately not treated as
/// physically available here.
/// </para>
/// <para>
/// <b>The line's remaining quantity to receive is not reduced.</b> Registration is a status step on
/// inventory, not a receipt, so <c>PurchLine.RemainPurchPhysical</c> is untouched and the product
/// receipt still posts the whole quantity once. <c>ProductReceiptService</c> consumes registered
/// transactions in preference to merely ordered ones and decrements the matching
/// <c>InventSum</c> bucket, which is what keeps §10's rule - a registration and the receipt that
/// follows it are never counted as two receipts of the same goods.
/// </para>
/// <para>
/// <b>No arrival journal document is modelled.</b> D365 records registration through an arrival
/// journal; that table does not exist here and inventing one is outside this scope, so the audit
/// trail is <c>InventTrans.DateStatus</c> and the registered status itself. A nonstocked line has no
/// inventory to register and is refused rather than silently ignored.
/// </para>
/// </remarks>
public sealed class PurchRegistrationService : IPurchRegistrationService
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly ICompanyExecutionContext _company;

    private DbContext Db => _unitOfWork.Context;

    public PurchRegistrationService(IUnitOfWork unitOfWork, ICompanyExecutionContext company)
    {
        _unitOfWork = unitOfWork;
        _company = company;
    }

    public Task<PurchResult> RegisterAsync(long purchTableRecId, PurchRegistrationPostDto input, CancellationToken ct) =>
        Db.Database.CreateExecutionStrategy().ExecuteAsync(() => RegisterCoreAsync(purchTableRecId, input, ct));

    private async Task<PurchResult> RegisterCoreAsync(long recId, PurchRegistrationPostDto input, CancellationToken ct)
    {
        var area = _company.GetDataAreaId();
        if (input.Lines is not { Count: > 0 })
            return new(400, null, "At least one line quantity is required to register an arrival.");
        if (input.Lines.Any(x => x.Qty <= 0m))
            return new(400, null, "A registered quantity must be greater than zero.");
        if (input.Lines.Select(x => x.PurchLineRecId).Distinct().Count() != input.Lines.Count)
            return new(400, null, "Each purchase line may appear only once in a registration.");

        var date = (input.RegistrationDate ?? DateTime.UtcNow).Date;
        var support = new SubledgerPostingSupport(Db);
        await _unitOfWork.BeginTransactionAsync(ct);
        try
        {
            var order = await Db.Set<PurchTable>().FirstOrDefaultAsync(x => x.RecId == recId && x.DataAreaId == area, ct);
            if (order == null) return await Fail(404, "Purchase order was not found in the selected company.", ct);
            if (order.PurchStatus == PurchStatus.Canceled)
                return await Fail(422, "The purchase order is canceled.", ct);
            if (order.PurchaseType == PurchaseType.ReturnItem)
                return await Fail(422, "A vendor return order ships goods out and is not registered on arrival.", ct);
            await support.LockAsync($"PurchDocument:{area}:{order.PurchId}", ct);

            var registered = new List<object>();
            foreach (var request in input.Lines)
            {
                var line = await Db.Set<PurchLine>().FirstOrDefaultAsync(x => x.DataAreaId == area
                    && x.RecId == request.PurchLineRecId && x.PurchId == order.PurchId, ct);
                if (line == null)
                    return await Fail(404, $"Purchase line {request.PurchLineRecId} was not found on this order.", ct);
                if (line.PurchStatus == PurchStatus.Canceled)
                    return await Fail(422, $"Purchase line {line.LineNumber} is canceled.", ct);
                if (request.Qty > line.RemainPurchPhysical)
                    return await Fail(422, $"Registered quantity for item {line.ItemId} exceeds the remaining "
                        + $"quantity {line.RemainPurchPhysical}.", ct);

                var model = await support.ResolveItemModelPolicyAsync(area, line.ItemId, ct);
                if (model == null) return await Fail(422, "The item model group is required.", ct);
                if (model.StockedProduct != NoYes.Yes)
                    return await Fail(422, $"Item {line.ItemId} is not stocked, so it has no arrival to register.", ct);

                // The quantity is converted to the inventory unit exactly as the product receipt
                // converts it, so the two steps cannot disagree about how much arrived.
                var inventQty = request.Qty == line.RemainPurchPhysical
                    ? line.RemainInventPhysical
                    : decimal.Round(request.Qty * line.QtyOrdered / line.PurchQty, 6);

                var origin = await Db.Set<InventTransOrigin>().FirstOrDefaultAsync(x => x.DataAreaId == area
                    && x.InventTransId == line.InventTransId && x.ReferenceId == order.PurchId
                    && x.ReferenceCategory == InventRefType.PurchaseOrder, ct);
                if (origin == null) return await Fail(422, $"Inventory origin is missing for item {line.ItemId}.", ct);

                var ordered = (await Db.Set<InventTrans>().Where(x => x.DataAreaId == area
                    && x.InventTransOrigin == origin.RecId && x.StatusReceipt == StatusReceipt.Ordered)
                    .ToListAsync(ct)).OrderBy(x => x.RecId).ToList();
                // Already-registered quantity is not ordered any more, so a second registration of
                // the same goods runs out of ordered supply and is refused here rather than doubling.
                if (ordered.Sum(x => x.Qty) < inventQty)
                    return await Fail(422, $"Ordered inventory does not cover the registration for item "
                        + $"{line.ItemId}; the quantity may already be registered or received.", ct);

                var left = inventQty;
                foreach (var source in ordered)
                {
                    if (left <= 0m) break;
                    var take = Math.Min(source.Qty, left);
                    if (take < source.Qty) Db.Set<InventTrans>().Add(Split(source, source.Qty - take));
                    source.Qty = take;
                    source.StatusReceipt = StatusReceipt.Registered;
                    source.DateStatus = date;
                    left -= take;
                }

                var summary = await Db.Set<InventSum>().SingleOrDefaultAsync(x => x.DataAreaId == area
                    && x.ItemId == line.ItemId && x.InventDimId == line.InventDimId, ct);
                if (summary == null) return await Fail(422, $"Inventory summary is missing for item {line.ItemId}.", ct);
                summary.Ordered -= inventQty;
                summary.Registered += inventQty;
                // Physical on-hand and physical value are untouched: those are the product receipt's
                // effect. Only the ordered side of availability moves.
                summary.AvailOrdered = summary.AvailPhysical + summary.Ordered - summary.OnOrder - summary.ReservOrdered;
                summary.LastUpdDatePhysical = date;

                registered.Add(new
                {
                    lineNumber = line.LineNumber, itemId = line.ItemId, purchQty = request.Qty, inventQty,
                    remainToReceive = line.RemainPurchPhysical
                });
            }

            await _unitOfWork.CompleteAsync(ct);
            await _unitOfWork.CommitTransactionAsync(ct);
            return new(200, new { purchId = order.PurchId, registrationDate = date, lines = registered },
                "Arrival registered. No ledger or vendor posting arises from a registration.");
        }
        catch (SubledgerPostingException ex)
        {
            return await Fail(422, ex.Message, ct);
        }
        catch
        {
            await _unitOfWork.RollbackTransactionAsync(ct);
            Db.ChangeTracker.Clear();
            throw;
        }
    }

    /// <summary>
    /// The part of an ordered transaction that is not arriving yet, which stays ordered.
    /// Mirrors <c>ProductReceiptService</c>'s split so the two steps leave the same shape behind.
    /// </summary>
    private static InventTrans Split(InventTrans source, decimal qty) => new()
    {
        DataAreaId = source.DataAreaId, InventTransOrigin = source.InventTransOrigin, ItemId = source.ItemId,
        InventDimId = source.InventDimId, Qty = qty, StatusReceipt = StatusReceipt.Ordered,
        StatusIssue = StatusIssue.None, ValueOpen = InventTransOpen.Yes,
        CurrencyCode = source.CurrencyCode, DateExpected = source.DateExpected
    };

    private async Task<PurchResult> Fail(int status, string message, CancellationToken ct)
    {
        await _unitOfWork.RollbackTransactionAsync(ct);
        Db.ChangeTracker.Clear();
        return new(status, null, message);
    }
}
