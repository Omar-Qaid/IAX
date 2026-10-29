using IAX.IXApi.Modules.Finance.Inventory;
using IAX.IXApi.Infrastructure.Identity;
using IAX.IXApi.Infrastructure.Persistence.Repositories;
using IAX.IXApi.Infrastructure.Persistence.Services;
using IAX.IXApi.Modules.Administration.NumberSequences;
using IAX.IXApi.Modules.Finance.Common;
using IAX.IXApi.Modules.Finance.Entities;
using IAX.IXApi.Shared.Application.Identity;
using FluentValidation;
using Microsoft.EntityFrameworkCore;

namespace IAX.IXApi.Modules.Finance.AccountsPayable;

/// <summary>
/// Product receipt (physical update). Splits each line's Ordered receipt into a Received portion,
/// increases physical on-hand and physical value, and records VendPackingSlipJour/Trans. Partial
/// receipts are supported; over-receipt and duplicate product receipt numbers are rejected.
/// Configured physical-purchase posting records receipt accruals for matched invoice reversal.
/// </summary>
public sealed class ProductReceiptService : BaseService<VendPackingSlipJour>, IProductReceiptService
{
    private readonly ICompanyExecutionContext _company;
    private readonly ISysNumberSequenceService _numbers;
    private readonly IValidator<ProductReceiptPostDto>? _validator;

    private DbContext Db => _unitOfWork.Context;

    public ProductReceiptService(
        IUnitOfWork unitOfWork,
        ICurrentUserService currentUser,
        ICompanyExecutionContext company,
        ISysNumberSequenceService numbers,
        IValidator<ProductReceiptPostDto>? validator = null)
        : base(unitOfWork, currentUser)
    {
        _company = company;
        _numbers = numbers;
        _validator = validator;
    }

    public ProductReceiptService(
        IUnitOfWork unitOfWork,
        ICompanyExecutionContext company,
        ISysNumberSequenceService numbers,
        IValidator<ProductReceiptPostDto>? validator = null)
        : this(unitOfWork, new NullCurrentUserService(company), company, numbers, validator)
    {
    }

    private sealed class NullCurrentUserService(ICompanyExecutionContext company) : ICurrentUserService
    {
        public string GetCurrentUserId() => string.Empty;
        public string GetOwnerAccountId() => string.Empty;
        public string GetDataAreaId() => company.GetDataAreaId();
    }

    public Task<PurchResult> PostAsync(long recId, ProductReceiptPostDto input, CancellationToken ct) =>
        Db.Database.CreateExecutionStrategy().ExecuteAsync(() => PostCoreAsync(recId, input, ct));

    private async Task<PurchResult> PostCoreAsync(long recId, ProductReceiptPostDto input, CancellationToken ct)
    {
        var area = _company.GetDataAreaId();
        var validation = await (_validator ?? new ProductReceiptPostDtoValidator()).ValidateAsync(input, ct);
        if (!validation.IsValid) return new(400, null, validation.Errors[0].ErrorMessage);
        var date = (input.DeliveryDate ?? DateTime.UtcNow).Date;
        await _unitOfWork.BeginTransactionAsync(ct);
        try
        {
            var order = await Db.Set<PurchTable>().FirstOrDefaultAsync(x => x.RecId == recId && x.DataAreaId == area, ct);
            if (order == null) return await Fail(404, "Purchase order was not found in the selected company.", ct);
            await new SubledgerPostingSupport(Db).LockAsync($"PurchDocument:{area}:{order.PurchId}", ct);
            if (order.PurchStatus == PurchStatus.Canceled) return await Fail(422, "The purchase order is canceled.", ct);
            if (order.PurchaseType == PurchaseType.ReturnItem)
                return await Fail(422, "Vendor return orders are shipped through the return order shipment.", ct);
            var slipId = input.PackingSlipId.Trim();
            if (await Db.Set<VendPackingSlipJour>().AnyAsync(x => x.DataAreaId == area && x.PurchId == order.PurchId
                && x.PackingSlipId == slipId, ct))
                return await Fail(422, $"Product receipt '{slipId}' has already been posted for this purchase order.", ct);

            var internalId = (await _numbers.NextAsync("VendPackingSlipJour", area, ct)).Code;
            var journal = new VendPackingSlipJour
            {
                DataAreaId = area, PackingSlipId = slipId, InternalPackingSlipId = internalId, PurchId = order.PurchId,
                DeliveryDate = date, OrderAccount = order.OrderAccount, InvoiceAccount = order.InvoiceAccount,
                LedgerVoucher = internalId
            };
            Db.Set<VendPackingSlipJour>().Add(journal);
            await _unitOfWork.CompleteAsync(ct);

            foreach (var request in input.Lines)
            {
                var line = await Db.Set<PurchLine>().FirstOrDefaultAsync(x => x.RecId == request.PurchLineRecId
                    && x.DataAreaId == area && x.PurchId == order.PurchId, ct);
                if (line == null) return await Fail(404, $"Purchase line {request.PurchLineRecId} was not found.", ct);
                if (request.Qty <= 0) return await Fail(422, "The receipt quantity must be greater than zero.", ct);
                if (request.Qty > line.RemainPurchPhysical)
                    return await Fail(422, $"Receipt quantity for item {line.ItemId} exceeds the remaining quantity {line.RemainPurchPhysical}.", ct);
                var inventQty = request.Qty == line.RemainPurchPhysical
                    ? line.RemainInventPhysical
                    : decimal.Round(request.Qty * line.QtyOrdered / line.PurchQty, 6);
                var unitCost = line.QtyOrdered == 0 ? 0m : line.LineAmount / line.QtyOrdered;
                var cost = inventQty * unitCost;

                var origin = await Db.Set<InventTransOrigin>().FirstOrDefaultAsync(x => x.DataAreaId == area
                    && x.InventTransId == line.InventTransId && x.ReferenceId == order.PurchId
                    && x.ReferenceCategory == InventRefType.PurchaseOrder, ct);
                if (origin == null) return await Fail(422, $"Inventory origin is missing for item {line.ItemId}.", ct);
                var ordered = (await Db.Set<InventTrans>().Where(x => x.DataAreaId == area && x.InventTransOrigin == origin.RecId
                    && x.StatusReceipt == StatusReceipt.Ordered).ToListAsync(ct)).OrderBy(x => x.RecId).ToList();
                if (ordered.Sum(x => x.Qty) < inventQty)
                    return await Fail(422, $"Ordered inventory does not cover the receipt for item {line.ItemId}.", ct);

                var left = inventQty;
                foreach (var source in ordered)
                {
                    if (left <= 0) break;
                    var take = Math.Min(source.Qty, left);
                    if (take < source.Qty)
                        Db.Set<InventTrans>().Add(CopyOrdered(source, source.Qty - take));
                    source.Qty = take;
                    source.StatusReceipt = StatusReceipt.Received;
                    source.DatePhysical = date;
                    source.DateStatus = date;
                    source.DateInvent = date;
                    source.PackingSlipId = internalId;
                    source.VoucherPhysical = internalId;
                    source.CostAmountPhysical = take * unitCost;
                    left -= take;
                }

                var countError = await InventoryCountProtection.CheckAsync(Db, area, line.ItemId, line.InventDimId, ct);
                if (countError != null) return await Fail(422, countError, ct);
                var summary = await Db.Set<InventSum>().SingleOrDefaultAsync(x => x.DataAreaId == area
                    && x.ItemId == line.ItemId && x.InventDimId == line.InventDimId, ct);
                if (summary == null) return await Fail(422, $"Inventory summary is missing for item {line.ItemId}.", ct);
                summary.Ordered -= inventQty;
                summary.Received += inventQty;
                summary.PhysicalInvent += inventQty;
                summary.PhysicalValue += cost;
                summary.AvailPhysical = summary.PhysicalInvent - summary.ReservPhysical - summary.Picked;
                summary.AvailOrdered = summary.AvailPhysical + summary.Ordered - summary.OnOrder - summary.ReservOrdered;
                summary.LastUpdDatePhysical = date;

                line.RemainPurchPhysical -= request.Qty;
                line.RemainInventPhysical = Math.Max(0m, line.RemainInventPhysical - inventQty);
                if (line.RemainPurchPhysical <= 0) line.PurchStatus = PurchStatus.Received;

                var receiptLine = new VendPackingSlipTrans
                {
                    DataAreaId = area, PackingSlipId = slipId, InternalPackingSlipId = internalId, OrigPurchId = order.PurchId,
                    VendPackingSlipJour = journal.RecId, DeliveryDate = date, LineNum = line.LineNumber, ItemId = line.ItemId,
                    Name = line.Name, Qty = request.Qty, InventQty = inventQty, Ordered = line.PurchQty,
                    Remain = line.RemainPurchPhysical, RemainInvent = line.RemainInventPhysical, ValueMst = cost,
                    PurchUnit = line.PurchUnit, InventTransId = line.InventTransId, InventDimId = line.InventDimId
                };
                Db.Set<VendPackingSlipTrans>().Add(receiptLine);
                await new PurchaseReceiptAccrualPosting(Db).PostAsync(order, receiptLine, ct);
                journal.Qty += request.Qty;
            }

            var lines = await Db.Set<PurchLine>().Where(x => x.DataAreaId == area && x.PurchId == order.PurchId
                && x.PurchStatus != PurchStatus.Canceled).ToListAsync(ct);
            if (lines.All(x => x.RemainPurchPhysical <= 0)) order.PurchStatus = PurchStatus.Received;
            order.DocumentStatus = DocumentStatus.PackingSlip;
            await _unitOfWork.CompleteAsync(ct);
            await _unitOfWork.CommitTransactionAsync(ct);
            return new(200, journal, "Product receipt posted.");
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

    private static InventTrans CopyOrdered(InventTrans source, decimal qty) => new()
    {
        DataAreaId = source.DataAreaId, InventTransOrigin = source.InventTransOrigin, ItemId = source.ItemId,
        InventDimId = source.InventDimId, Qty = qty, StatusReceipt = StatusReceipt.Ordered, StatusIssue = StatusIssue.None,
        ValueOpen = InventTransOpen.Yes, CurrencyCode = source.CurrencyCode, DateExpected = source.DateExpected
    };

    private async Task<PurchResult> Fail(int status, string message, CancellationToken ct)
    {
        await _unitOfWork.RollbackTransactionAsync(ct);
        Db.ChangeTracker.Clear();
        return new(status, null, message);
    }
}
