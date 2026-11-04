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
/// Pending vendor invoice (entry and hold). A vendor invoice is entered against a purchase order,
/// defaulted to the received-but-uninvoiced quantities, and held in VendInvoiceInfoTable/Line until
/// it is posted or discarded.
/// </summary>
/// <remarks>
/// This service does not post. <see cref="PostAsync"/> hands the held invoice to
/// <see cref="IVendorInvoiceService"/>, which remains the single vendor-invoice posting path, so
/// quantity, period, profile and dimension rules are applied in exactly one place. Entry therefore
/// re-validates nothing that posting owns: an order that changes after entry is refused at posting,
/// not silently adjusted here.
/// </remarks>
public sealed class VendInvoiceInfoService : BaseService<VendInvoiceInfoTable>, IVendInvoiceInfoService
{
    private readonly ICompanyExecutionContext _company;
    private readonly ISysNumberSequenceService _numbers;
    private readonly IVendorInvoiceService _invoices;
    private readonly IValidator<VendInvoiceInfoCreateDto>? _validator;

    private DbContext Db => _unitOfWork.Context;

    public VendInvoiceInfoService(
        IUnitOfWork unitOfWork,
        ICurrentUserService currentUser,
        ICompanyExecutionContext company,
        ISysNumberSequenceService numbers,
        IVendorInvoiceService invoices,
        IValidator<VendInvoiceInfoCreateDto>? validator = null)
        : base(unitOfWork, currentUser)
    {
        _company = company;
        _numbers = numbers;
        _invoices = invoices;
        _validator = validator;
    }

    public VendInvoiceInfoService(
        IUnitOfWork unitOfWork,
        ICompanyExecutionContext company,
        ISysNumberSequenceService numbers,
        IVendorInvoiceService invoices,
        IValidator<VendInvoiceInfoCreateDto>? validator = null)
        : this(unitOfWork, new NullCurrentUserService(company), company, numbers, invoices, validator)
    {
    }

    private sealed class NullCurrentUserService(ICompanyExecutionContext company) : ICurrentUserService
    {
        public string GetCurrentUserId() => string.Empty;
        public string GetOwnerAccountId() => string.Empty;
        public string GetDataAreaId() => company.GetDataAreaId();
    }

    public Task<PurchResult> CreateFromOrderAsync(long purchTableRecId, VendInvoiceInfoCreateDto input, CancellationToken ct) =>
        Db.Database.CreateExecutionStrategy().ExecuteAsync(() => CreateCoreAsync(purchTableRecId, input, ct));

    private async Task<PurchResult> CreateCoreAsync(long purchTableRecId, VendInvoiceInfoCreateDto input, CancellationToken ct)
    {
        var area = _company.GetDataAreaId();
        var validation = await (_validator ?? new VendInvoiceInfoCreateDtoValidator()).ValidateAsync(input, ct);
        if (!validation.IsValid) return new(400, null, validation.Errors[0].ErrorMessage);

        var support = new SubledgerPostingSupport(Db);
        await _unitOfWork.BeginTransactionAsync(ct);
        try
        {
            var order = await Db.Set<PurchTable>().AsNoTracking()
                .FirstOrDefaultAsync(x => x.RecId == purchTableRecId && x.DataAreaId == area, ct);
            if (order == null) return await Fail(404, "Purchase order was not found in the selected company.", ct);
            if (order.PurchaseType == PurchaseType.ReturnItem)
                return await Fail(422, "Vendor return orders are credited through the return order credit note.", ct);

            var number = input.Num.Trim();
            await support.LockAsync($"PurchDocument:{area}:{order.PurchId}", ct);
            await support.LockAsync($"VendInvoice:{area}:{order.InvoiceAccount}:{number}".ToUpperInvariant(), ct);

            var vendor = await Db.Set<VendTable>().AsNoTracking()
                .FirstOrDefaultAsync(x => x.DataAreaId == area && x.AccountNum == order.InvoiceAccount, ct);
            if (vendor == null) return await Fail(422, "The invoice vendor was not found.", ct);
            if (vendor.Blocked is CustVendBlocked.All or CustVendBlocked.Invoicing)
                return await Fail(422, "The vendor is blocked for invoicing.", ct);

            // Duplicate invoice numbers are checked per vendor, not globally, as APTables.md
            // section 8.1 requires. A configurable duplicate-invoice policy would live on
            // VendParameters, which does not exist in this module.
            if (await Db.Set<VendInvoiceJour>().AnyAsync(x => x.DataAreaId == area
                && x.InvoiceAccount == order.InvoiceAccount && x.InvoiceId == number, ct))
                return await Fail(422, $"Vendor invoice '{number}' has already been posted for this vendor.", ct);
            if (await Db.Set<VendInvoiceInfoTable>().AnyAsync(x => x.DataAreaId == area
                && x.InvoiceAccount == order.InvoiceAccount && x.Num == number, ct))
                return await Fail(422, $"A pending vendor invoice '{number}' is already held for this vendor.", ct);

            var orderLines = await Db.Set<PurchLine>().AsNoTracking()
                .Where(x => x.DataAreaId == area && x.PurchId == order.PurchId && x.PurchStatus != PurchStatus.Canceled)
                .ToListAsync(ct);

            var selections = new List<(PurchLine Line, decimal Qty, decimal UnitPrice)>();
            if (input.Lines is { Count: > 0 })
            {
                foreach (var request in input.Lines)
                {
                    var line = orderLines.FirstOrDefault(x => x.RecId == request.PurchLineRecId);
                    if (line == null) return await Fail(404, $"Purchase line {request.PurchLineRecId} was not found.", ct);
                    var available = line.RemainPurchFinancial - line.RemainPurchPhysical;
                    if (request.Qty > available)
                        return await Fail(422, $"Invoice quantity for item {line.ItemId} exceeds the received, uninvoiced quantity {available}.", ct);
                    var price = request.UnitPrice ?? line.PurchPrice;
                    if (price < 0) return await Fail(422, "The invoice price cannot be negative.", ct);
                    selections.Add((line, request.Qty, price));
                }
            }
            else
            {
                // D365's default quantity selection for an invoice: the received, uninvoiced amount.
                foreach (var line in orderLines.OrderBy(x => x.LineNumber))
                {
                    var available = line.RemainPurchFinancial - line.RemainPurchPhysical;
                    if (available > 0) selections.Add((line, available, line.PurchPrice));
                }
            }
            if (selections.Count == 0)
                return await Fail(422, "The purchase order has no received, uninvoiced quantity to invoice.", ct);

            var parmId = (await _numbers.NextAsync("VendInvoiceInfoTable", area, ct)).Code;
            var documentDate = (input.DocumentDate ?? DateTime.UtcNow).Date;
            var header = new VendInvoiceInfoTable
            {
                DataAreaId = area, ParmId = parmId, Num = number,
                InvoiceAccount = order.InvoiceAccount, OrderAccount = order.OrderAccount,
                DocumentDate = documentDate, TransDate = (input.TransDate ?? documentDate).Date,
                CurrencyCode = order.CurrencyCode, Description = (input.Description ?? string.Empty).Trim()
            };
            header.SourceDocumentHeader = await new PurchaseSourceAccounting(Db).HeaderAsync(area, FinanceTableNumbers.VendInvoiceInfoTable, ct);
            Db.Set<VendInvoiceInfoTable>().Add(header);

            var lines = new List<VendInvoiceInfoLine>();
            foreach (var (line, qty, price) in selections)
            {
                var amount = decimal.Round(qty * price / (line.PriceUnit > 0 ? line.PriceUnit : 1m), 2, MidpointRounding.AwayFromZero);
                var pending = new VendInvoiceInfoLine
                {
                    DataAreaId = area, ParmId = parmId, OrigPurchId = order.PurchId, LineNum = line.LineNumber,
                    ItemId = line.ItemId, InventTransId = line.InventTransId, InventDimId = line.InventDimId,
                    PurchUnit = line.PurchUnit, ReceiveNow = qty, PurchPrice = price, LineAmount = amount
                };
                pending.SourceDocumentLine = await new PurchaseSourceAccounting(Db).LineAsync(area, header.SourceDocumentHeader!.Value,
                    FinanceTableNumbers.VendInvoiceInfoLine, qty, header.TransDate, ct);
                lines.Add(pending);
                Db.Set<VendInvoiceInfoLine>().Add(pending);
                await _unitOfWork.CompleteAsync(ct);
                var receipts = await Db.Set<VendPackingSlipTrans>().Where(x => x.DataAreaId == area && x.OrigPurchId == order.PurchId
                    && x.InventTransId == line.InventTransId).OrderBy(x => x.RecId).ToListAsync(ct);
                var left = qty;
                foreach (var receipt in receipts)
                {
                    if (left <= 0) break;
                    var matched = await Db.Set<VendInvoicePackingSlipQuantityMatch>().Where(x => x.DataAreaId == area && x.PackingSlipTransRecId == receipt.RecId)
                        .Select(x => x.Quantity).ToListAsync(ct);
                    var take = Math.Min(left, receipt.Qty - matched.Sum());
                    if (take <= 0) continue;
                    Db.Add(new VendInvoiceInfoSubLine { DataAreaId = area, DocumentId = receipt.PackingSlipId,
                        ParmId = parmId, LineRefRecId = pending.RecId, JournalRefTableId = FinanceTableNumbers.VendPackingSlipTrans,
                        JournalRefRecId = receipt.RecId, ReceiveNow = take, InventNow = receipt.Qty == 0 ? 0 : take * receipt.InventQty / receipt.Qty });
                    left -= take;
                }
                // Legacy held records may have quantities without receipt snapshots. Posting still
                // refuses them; entry never manufactures a receipt to repair historical data.
                if (receipts.Count > 0 && left > 0) return await Fail(422, "Receipt snapshots do not cover the pending invoice quantity.", ct);
            }

            await _unitOfWork.CompleteAsync(ct);
            await _unitOfWork.CommitTransactionAsync(ct);
            return new(200, Read(header, lines), "Pending vendor invoice held.");
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

    public async Task<PurchResult> GetByParmIdAsync(string parmId, CancellationToken ct)
    {
        var area = _company.GetDataAreaId();
        var header = await Db.Set<VendInvoiceInfoTable>().AsNoTracking()
            .FirstOrDefaultAsync(x => x.DataAreaId == area && x.ParmId == parmId, ct);
        if (header == null) return new(404, null, "Pending vendor invoice was not found in the selected company.");
        var lines = await Db.Set<VendInvoiceInfoLine>().AsNoTracking()
            .Where(x => x.DataAreaId == area && x.ParmId == parmId).ToListAsync(ct);
        return new(200, Read(header, lines), null);
    }

    public Task<PurchResult> DiscardAsync(string parmId, CancellationToken ct) =>
        Db.Database.CreateExecutionStrategy().ExecuteAsync(() => DiscardCoreAsync(parmId, ct));

    private async Task<PurchResult> DiscardCoreAsync(string parmId, CancellationToken ct)
    {
        var area = _company.GetDataAreaId();
        await _unitOfWork.BeginTransactionAsync(ct);
        try
        {
            var header = await Db.Set<VendInvoiceInfoTable>()
                .FirstOrDefaultAsync(x => x.DataAreaId == area && x.ParmId == parmId, ct);
            if (header == null) return await Fail(404, "Pending vendor invoice was not found in the selected company.", ct);
            var lines = await Db.Set<VendInvoiceInfoLine>()
                .Where(x => x.DataAreaId == area && x.ParmId == parmId).ToListAsync(ct);
            var lineIds = lines.Select(x => x.RecId).ToArray();
            var connections = await Db.Set<VendInvoiceInfoSubLine>().Where(x => x.DataAreaId == area && x.LineRefRecId != null && lineIds.Contains(x.LineRefRecId.Value)).ToListAsync(ct);
            Db.Set<VendInvoiceInfoSubLine>().RemoveRange(connections);
            Db.Set<VendInvoiceInfoLine>().RemoveRange(lines);
            Db.Set<VendInvoiceInfoTable>().Remove(header);
            await _unitOfWork.CompleteAsync(ct);
            await _unitOfWork.CommitTransactionAsync(ct);
            return new(200, new { parmId, num = header.Num }, "Pending vendor invoice discarded.");
        }
        catch
        {
            await _unitOfWork.RollbackTransactionAsync(ct);
            Db.ChangeTracker.Clear();
            throw;
        }
    }

    public async Task<PurchResult> PostAsync(string parmId, CancellationToken ct)
    {
        var area = _company.GetDataAreaId();
        var header = await Db.Set<VendInvoiceInfoTable>().AsNoTracking()
            .FirstOrDefaultAsync(x => x.DataAreaId == area && x.ParmId == parmId, ct);
        if (header == null) return new(404, null, "Pending vendor invoice was not found in the selected company.");
        var pendingLines = await Db.Set<VendInvoiceInfoLine>().AsNoTracking()
            .Where(x => x.DataAreaId == area && x.ParmId == parmId).ToListAsync(ct);
        if (pendingLines.Count == 0) return new(422, null, "The pending vendor invoice has no lines.");

        var purchIds = pendingLines.Select(x => x.OrigPurchId).Distinct().ToList();
        if (purchIds.Count != 1)
            return new(422, null, "A pending vendor invoice covering several purchase orders is not supported in the basic scope.");
        var order = await Db.Set<PurchTable>().AsNoTracking()
            .FirstOrDefaultAsync(x => x.DataAreaId == area && x.PurchId == purchIds[0], ct);
        if (order == null) return new(422, null, $"Purchase order '{purchIds[0]}' was not found in the selected company.");

        // The pending line keeps its source link as OrigPurchId + InventTransId, which is how the
        // PurchLine the posting path updates is resolved. A line whose order line has since gone
        // is refused here rather than posted against a different line.
        var requests = new List<PurchPostLineDto>();
        foreach (var pending in pendingLines.OrderBy(x => x.LineNum))
        {
            var line = await Db.Set<PurchLine>().AsNoTracking().FirstOrDefaultAsync(x => x.DataAreaId == area
                && x.PurchId == pending.OrigPurchId && x.InventTransId == pending.InventTransId, ct);
            if (line == null)
                return new(422, null, $"The purchase line for item {pending.ItemId} no longer exists, so the pending invoice cannot be posted.");
            var receiptLinks = await Db.Set<VendInvoiceInfoSubLine>().Where(x => x.DataAreaId == area && x.LineRefRecId == pending.RecId).ToListAsync(ct);
            foreach (var link in receiptLinks)
            {
                var receipt = await Db.Set<VendPackingSlipTrans>().AsNoTracking().SingleOrDefaultAsync(x => x.DataAreaId == area && x.RecId == link.JournalRefRecId, ct);
                if (receipt == null) return new(422, null, "The pending invoice's selected receipt no longer exists.");
                var consumed = await Db.Set<VendInvoicePackingSlipQuantityMatch>().Where(x => x.DataAreaId == area && x.PackingSlipTransRecId == receipt.RecId).Select(x => x.Quantity).ToListAsync(ct);
                if (receipt.Qty - consumed.Sum() < link.ReceiveNow.GetValueOrDefault()) return new(422, null, "A selected receipt quantity has already been invoiced.");
            }
            requests.Add(new PurchPostLineDto(line.RecId, pending.ReceiveNow, pending.PurchPrice));
        }

        var result = await _invoices.PostAsync(order.RecId,
            new VendorInvoicePostDto(header.Num, header.TransDate, requests), ct);
        if (result.StatusCode != 200) return result;

        // Posting owns its own transaction, so the hold is released after it commits. If this
        // release fails the invoice stays posted and the hold remains: re-posting it is then
        // refused by the already-posted duplicate check, so no invoice can post twice.
        var discard = await DiscardAsync(parmId, ct);
        if (discard.StatusCode != 200) return discard;
        return result;
    }

    private static VendInvoiceInfoReadDto Read(VendInvoiceInfoTable header, List<VendInvoiceInfoLine> lines) =>
        new(header.RecId, header.ParmId, header.Num, header.InvoiceAccount, header.OrderAccount,
            header.DocumentDate, header.TransDate, header.CurrencyCode, header.Description,
            lines.Sum(x => x.LineAmount),
            lines.OrderBy(x => x.LineNum).Select(x => new VendInvoiceInfoLineReadDto(
                x.RecId, x.OrigPurchId, x.LineNum, x.ItemId, x.InventTransId, x.InventDimId,
                x.PurchUnit, x.ReceiveNow, x.PurchPrice, x.LineAmount)).ToList());

    private async Task<PurchResult> Fail(int status, string message, CancellationToken ct)
    {
        await _unitOfWork.RollbackTransactionAsync(ct);
        Db.ChangeTracker.Clear();
        return new(status, null, message);
    }
}
