using IAX.IXApi.Modules.Finance.Inventory;
using IAX.IXApi.Infrastructure.Persistence.Repositories;
using IAX.IXApi.Modules.Administration.NumberSequences;
using IAX.IXApi.Modules.Finance.AccountsReceivable.SalesOrder.Interfaces;
using IAX.IXApi.Modules.Finance.Common;
using IAX.IXApi.Modules.Finance.Entities;
using IAX.IXApi.Shared.Application.Identity;
using FluentValidation;
using Microsoft.EntityFrameworkCore;

using IAX.IXApi.Infrastructure.Identity;
using IAX.IXApi.Infrastructure.Persistence.Services;

namespace IAX.IXApi.Modules.Finance.AccountsPayable;

/// <summary>
/// Vendor returns (Purchorder.md Phase 8). A return is a purchase order with PurchaseType =
/// ReturnItem and negative lines linked to the original invoiced receipt (InventTransIdReturn).
/// Inventory follows the issue direction: created = Ordered (on order), shipped = Deducted
/// (physical stock decreases), vendor credit note = Sold. The return is valued at the original
/// receipt cost and the vendor is debited (VendTrans) Ã¢â‚¬â€ Dr Vendor summary / Cr inventory receipt.
/// Returned quantity cannot exceed the invoiced quantity not already returned, nor available stock.
/// </summary>
public sealed class PurchReturnService : BaseService<PurchTable>, IPurchReturnService
{
    private readonly ICompanyExecutionContext _company;
    private readonly ISysNumberSequenceService _numbers;
    private readonly ISalesInventoryNumberService _inventoryNumbers;
    private readonly IValidator<PurchReturnCreateDto>? _createValidator;
    private readonly IValidator<PurchReturnShipDto>? _shipValidator;
    private readonly IValidator<VendCreditNoteDto>? _creditValidator;

    public PurchReturnService(
        IUnitOfWork unitOfWork,
        ICurrentUserService currentUser,
        ICompanyExecutionContext company,
        ISysNumberSequenceService numbers,
        ISalesInventoryNumberService inventoryNumbers,
        IValidator<PurchReturnCreateDto>? createValidator = null,
        IValidator<PurchReturnShipDto>? shipValidator = null,
        IValidator<VendCreditNoteDto>? creditValidator = null)
        : base(unitOfWork, currentUser)
    {
        _company = company;
        _numbers = numbers;
        _inventoryNumbers = inventoryNumbers;
        _createValidator = createValidator;
        _shipValidator = shipValidator;
        _creditValidator = creditValidator;
    }

    public PurchReturnService(
        IUnitOfWork unitOfWork,
        ICompanyExecutionContext company,
        ISysNumberSequenceService numbers,
        ISalesInventoryNumberService inventoryNumbers,
        IValidator<PurchReturnCreateDto>? createValidator = null,
        IValidator<PurchReturnShipDto>? shipValidator = null,
        IValidator<VendCreditNoteDto>? creditValidator = null)
        : this(unitOfWork, new NullCurrentUserService(company), company, numbers, inventoryNumbers, createValidator, shipValidator, creditValidator)
    {
    }

    private sealed class NullCurrentUserService(ICompanyExecutionContext company) : ICurrentUserService
    {
        public string GetCurrentUserId() => string.Empty;
        public string GetOwnerAccountId() => string.Empty;
        public string GetDataAreaId() => company.GetDataAreaId();
    }

    private DbContext Db => _unitOfWork.Context;
    private IUnitOfWork unitOfWork => _unitOfWork;
    private ICompanyExecutionContext company => _company;
    private ISysNumberSequenceService numbers => _numbers;
    private ISalesInventoryNumberService inventoryNumbers => _inventoryNumbers;
    private IValidator<PurchReturnCreateDto>? createValidator => _createValidator;
    private IValidator<PurchReturnShipDto>? shipValidator => _shipValidator;
    private IValidator<VendCreditNoteDto>? creditValidator => _creditValidator;

    // ---------------------------------------------------------------- Create return order

    public Task<PurchResult> CreateAsync(PurchReturnCreateDto input, CancellationToken ct) =>
        Db.Database.CreateExecutionStrategy().ExecuteAsync(() => CreateCoreAsync(input, ct));

    private async Task<PurchResult> CreateCoreAsync(PurchReturnCreateDto input, CancellationToken ct)
    {
        var area = company.GetDataAreaId();
        var createValidation = await (createValidator ?? new PurchReturnCreateDtoValidator()).ValidateAsync(input, ct);
        if (!createValidation.IsValid) return new(400, null, createValidation.Errors[0].ErrorMessage);
        await unitOfWork.BeginTransactionAsync(ct);
        try
        {
            var requestedIds = input.Lines.Select(x => x.VendInvoiceTransRecId).Distinct().ToList();
            var selectedLines = await Db.Set<VendInvoiceTrans>().AsNoTracking()
                .Where(x => x.DataAreaId == area && x.InvoiceId == input.VendInvoiceId && requestedIds.Contains(x.RecId))
                .ToListAsync(ct);
            if (selectedLines.Count != requestedIds.Count)
                return await Fail(404, $"One or more invoice lines were not found on vendor invoice '{input.VendInvoiceId}'.", ct);
            var selectedInvoice = selectedLines[0];
            if (selectedLines.Any(x => x.InternalInvoiceId != selectedInvoice.InternalInvoiceId || x.PurchId != selectedInvoice.PurchId))
                return await Fail(422, "All return lines must belong to the same posted vendor invoice.", ct);
            var invoices = await Db.Set<VendInvoiceJour>().AsNoTracking()
                .Where(x => x.DataAreaId == area && x.InvoiceId == input.VendInvoiceId
                    && x.InternalInvoiceId == selectedInvoice.InternalInvoiceId && x.PurchId == selectedInvoice.PurchId)
                .ToListAsync(ct);
            if (invoices.Count != 1)
                return await Fail(422, "The selected invoice lines do not identify one posted vendor invoice.", ct);
            var invoice = invoices[0];
            var original = await Db.Set<PurchTable>().AsNoTracking()
                .FirstOrDefaultAsync(x => x.DataAreaId == area && x.PurchId == invoice.PurchId, ct);
            if (original == null) return await Fail(422, "The invoice's original purchase order was not found.", ct);

            var order = new PurchTable
            {
                DataAreaId = area, PurchId = (await numbers.NextAsync("PurchTable", area, ct)).Code,
                PurchaseType = PurchaseType.ReturnItem, PurchStatus = PurchStatus.Backorder,
                ReturnItemNum = string.IsNullOrWhiteSpace(input.ReturnItemNum)
                    ? (await numbers.NextAsync("ReturnItemNum", area, ct)).Code : input.ReturnItemNum.Trim(),
                OrderAccount = invoice.OrderAccount, InvoiceAccount = invoice.InvoiceAccount, PurchName = original.PurchName,
                CurrencyCode = invoice.CurrencyCode, Payment = original.Payment, OrderDate = DateTime.UtcNow.Date,
                DeliveryDate = DateTime.UtcNow.Date, InventSiteId = original.InventSiteId, InventLocationId = original.InventLocationId,
                DefaultDimension = original.DefaultDimension
            };
            Db.Set<PurchTable>().Add(order);
            await unitOfWork.CompleteAsync(ct);

            await new SubledgerPostingSupport(Db).LockAsync($"PurchReturnSource:{area}:{invoice.InternalInvoiceId}", ct);
            var lineNumber = 0m;
            foreach (var request in input.Lines)
            {
                var invoiced = selectedLines.Single(x => x.RecId == request.VendInvoiceTransRecId);
                if (request.Qty <= 0 || invoiced.Qty <= 0) return await Fail(422, "The return quantity must be greater than zero.", ct);
                var invoicedQty = (await Db.Set<VendInvoiceTrans>().AsNoTracking()
                    .Where(x => x.DataAreaId == area && x.InventTransId == invoiced.InventTransId && x.Qty > 0)
                    .Select(x => x.Qty).ToListAsync(ct)).Sum();
                var alreadyReturned = -(await Db.Set<PurchLine>().AsNoTracking()
                    .Where(x => x.DataAreaId == area && x.InventTransIdReturn == invoiced.InventTransId
                        && x.PurchaseType == PurchaseType.ReturnItem && x.PurchStatus != PurchStatus.Canceled)
                    .Select(x => x.PurchQty).ToListAsync(ct)).Sum();
                if (request.Qty > invoicedQty - alreadyReturned)
                    return await Fail(422, $"Return quantity for item {invoiced.ItemId} exceeds the returnable quantity {invoicedQty - alreadyReturned}.", ct);

                var returnedFromInvoiceLine = -(await Db.Set<PurchLine>().Where(x => x.DataAreaId == area
                    && x.OriginalInvoiceTransRecId == invoiced.RecId && x.PurchStatus != PurchStatus.Canceled)
                    .Select(x => x.PurchQty).ToListAsync(ct)).Sum();
                // Include unsaved lines in this request as well as previous return orders.
                returnedFromInvoiceLine -= Db.Set<PurchLine>().Local.Where(x => x.OriginalInvoiceTransRecId == invoiced.RecId
                    && Db.Entry(x).State == EntityState.Added).Sum(x => x.PurchQty);
                if (request.Qty > invoiced.Qty - returnedFromInvoiceLine)
                    return await Fail(422, "Return quantity exceeds the remaining quantity of the selected invoice line.", ct);

                var inventQty = decimal.Round(request.Qty * invoiced.InventQty / invoiced.Qty, 6);
                var line = new PurchLine
                {
                    DataAreaId = area, PurchId = order.PurchId, LineNumber = ++lineNumber, ItemId = invoiced.ItemId,
                    VendAccount = order.OrderAccount, CurrencyCode = order.CurrencyCode, PurchaseType = PurchaseType.ReturnItem,
                    PurchQty = -request.Qty, PurchUnit = invoiced.PurchUnit, QtyOrdered = -inventQty,
                    PurchPrice = invoiced.PurchPrice, PriceUnit = invoiced.PriceUnit > 0 ? invoiced.PriceUnit : 1m,
                    LineAmount = -request.Qty * invoiced.PurchPrice / (invoiced.PriceUnit > 0 ? invoiced.PriceUnit : 1m),
                    RemainPurchPhysical = -request.Qty, RemainPurchFinancial = -request.Qty,
                    RemainInventPhysical = -inventQty, RemainInventFinancial = -inventQty,
                    InventDimId = invoiced.InventDimId, InventTransIdReturn = invoiced.InventTransId,
                    OriginalInvoiceTransRecId = invoiced.RecId, TaxGroup = invoiced.TaxGroup, TaxItemGroup = invoiced.TaxItemGroup,
                    DeliveryDate = order.DeliveryDate
                };
                await CreateIssueDemandAsync(area, order, line, inventQty, ct);
                Db.Set<PurchLine>().Add(line);
            }

            await unitOfWork.CompleteAsync(ct);
            await unitOfWork.CommitTransactionAsync(ct);
            return new(200, new { recId = order.RecId, purchId = order.PurchId, returnItemNum = order.ReturnItemNum }, "Vendor return order created.");
        }
        catch (SubledgerPostingException ex)
        {
            return await Fail(422, ex.Message, ct);
        }
        catch
        {
            await unitOfWork.RollbackTransactionAsync(ct);
            Db.ChangeTracker.Clear();
            throw;
        }
    }

    // A vendor return line is outbound demand: an Ordered issue linked to the original receipt.
    private async Task CreateIssueDemandAsync(string area, PurchTable order, PurchLine line, decimal inventQty, CancellationToken ct)
    {
        var originalOrigin = await Db.Set<InventTransOrigin>().AsNoTracking()
            .FirstOrDefaultAsync(x => x.DataAreaId == area && x.InventTransId == line.InventTransIdReturn, ct)
            ?? throw new SubledgerPostingException($"The original receipt of item {line.ItemId} was not found.");
        var transId = await inventoryNumbers.NextInventTransIdAsync(ct);
        var origin = new InventTransOrigin
        {
            DataAreaId = area, InventTransId = transId, ItemId = line.ItemId, ItemInventDimId = line.InventDimId,
            ReferenceCategory = InventRefType.PurchaseOrder, ReferenceId = order.PurchId
        };
        Db.Set<InventTransOrigin>().Add(origin);
        await unitOfWork.CompleteAsync(ct);
        Db.Set<InventTransOriginPurchLine>().Add(new InventTransOriginPurchLine
        {
            InventTransOrigin = origin.RecId, PurchLineDataAreaId = area, PurchLineInventTransId = transId, DataAreaId = area
        });
        Db.Set<InventTrans>().Add(new InventTrans
        {
            DataAreaId = area, InventTransOrigin = origin.RecId, ItemId = line.ItemId, InventDimId = line.InventDimId,
            Qty = -inventQty, StatusIssue = StatusIssue.Ordered, StatusReceipt = StatusReceipt.None, ValueOpen = InventTransOpen.Yes,
            CurrencyCode = order.CurrencyCode, DateExpected = line.DeliveryDate, ReturnInventTransOrigin = originalOrigin.RecId
        });
        var summary = await Db.Set<InventSum>().SingleOrDefaultAsync(x => x.DataAreaId == area
            && x.ItemId == line.ItemId && x.InventDimId == line.InventDimId, ct)
            ?? throw new SubledgerPostingException($"Inventory summary is missing for item {line.ItemId}.");
        summary.OnOrder += inventQty;
        summary.AvailOrdered = summary.AvailPhysical + summary.Ordered - summary.OnOrder - summary.ReservOrdered;
        line.InventTransId = transId;
    }

    // ---------------------------------------------------------------- Ship back to vendor (physical issue)

    public Task<PurchResult> ShipAsync(long returnRecId, PurchReturnShipDto input, CancellationToken ct) =>
        Db.Database.CreateExecutionStrategy().ExecuteAsync(() => ShipCoreAsync(returnRecId, input, ct));

    private async Task<PurchResult> ShipCoreAsync(long returnRecId, PurchReturnShipDto input, CancellationToken ct)
    {
        var area = company.GetDataAreaId();
        var shipValidation = await (shipValidator ?? new PurchReturnShipDtoValidator()).ValidateAsync(input, ct);
        if (!shipValidation.IsValid) return new(400, null, shipValidation.Errors[0].ErrorMessage);
        var date = (input.ShipDate ?? DateTime.UtcNow).Date;
        var support = new SubledgerPostingSupport(Db);
        await unitOfWork.BeginTransactionAsync(ct);
        try
        {
            var order = await LoadReturnAsync(area, returnRecId, ct);
            if (order == null) return await Fail(404, "Vendor return order was not found in the selected company.", ct);
            await support.LockAsync($"PurchDocument:{area}:{order.PurchId}", ct);
            var slipId = input.PackingSlipId.Trim();
            if (await Db.Set<VendPackingSlipJour>().AnyAsync(x => x.DataAreaId == area && x.PurchId == order.PurchId && x.PackingSlipId == slipId, ct))
                return await Fail(422, $"Return shipment '{slipId}' has already been posted for this return order.", ct);

            var internalId = (await numbers.NextAsync("VendPackingSlipJour", area, ct)).Code;
            var journal = new VendPackingSlipJour
            {
                DataAreaId = area, PackingSlipId = slipId, InternalPackingSlipId = internalId, PurchId = order.PurchId,
                DeliveryDate = date, OrderAccount = order.OrderAccount, InvoiceAccount = order.InvoiceAccount, LedgerVoucher = internalId
            };
            Db.Set<VendPackingSlipJour>().Add(journal);
            await unitOfWork.CompleteAsync(ct);

            foreach (var request in input.Lines)
            {
                var line = await Db.Set<PurchLine>().FirstOrDefaultAsync(x => x.RecId == request.PurchLineRecId
                    && x.DataAreaId == area && x.PurchId == order.PurchId, ct);
                if (line == null) return await Fail(404, $"Return line {request.PurchLineRecId} was not found.", ct);
                if (request.Qty <= 0 || request.Qty > -line.RemainPurchPhysical)
                    return await Fail(422, $"Shipment quantity for item {line.ItemId} must be positive and at most {-line.RemainPurchPhysical}.", ct);
                var inventQty = request.Qty == -line.RemainPurchPhysical
                    ? -line.RemainInventPhysical
                    : decimal.Round(request.Qty * line.QtyOrdered / line.PurchQty, 6);
                var countError = await InventoryCountProtection.CheckAsync(Db, area, line.ItemId, line.InventDimId, ct);
                if (countError != null) return await Fail(422, countError, ct);
                var summary = await Db.Set<InventSum>().SingleAsync(x => x.DataAreaId == area
                    && x.ItemId == line.ItemId && x.InventDimId == line.InventDimId, ct);
                if (summary.PhysicalInvent - summary.ReservPhysical - summary.Picked < inventQty)
                    return await Fail(422, $"Available physical stock is insufficient to return item {line.ItemId}.", ct);
                var unitCost = await OriginalReceiptUnitCostAsync(area, line, ct);

                var origin = await Db.Set<InventTransOrigin>().FirstAsync(x => x.DataAreaId == area
                    && x.InventTransId == line.InventTransId && x.ReferenceId == order.PurchId, ct);
                var ordered = (await Db.Set<InventTrans>().Where(x => x.DataAreaId == area && x.InventTransOrigin == origin.RecId
                    && x.StatusIssue == StatusIssue.Ordered).ToListAsync(ct)).OrderBy(x => x.RecId).ToList();
                if (ordered.Sum(x => -x.Qty) < inventQty)
                    return await Fail(422, $"Return demand does not cover the shipment for item {line.ItemId}.", ct);
                var left = inventQty;
                foreach (var source in ordered)
                {
                    if (left <= 0) break;
                    var available = -source.Qty;
                    var take = Math.Min(available, left);
                    if (take < available)
                        Db.Set<InventTrans>().Add(new InventTrans
                        {
                            DataAreaId = area, InventTransOrigin = source.InventTransOrigin, ItemId = source.ItemId,
                            InventDimId = source.InventDimId, Qty = -(available - take), StatusIssue = StatusIssue.Ordered,
                            StatusReceipt = StatusReceipt.None, ValueOpen = InventTransOpen.Yes, CurrencyCode = source.CurrencyCode,
                            DateExpected = source.DateExpected, ReturnInventTransOrigin = source.ReturnInventTransOrigin
                        });
                    source.Qty = -take;
                    source.StatusIssue = StatusIssue.Deducted;
                    source.DatePhysical = date;
                    source.DateStatus = date;
                    source.DateInvent = date;
                    source.PackingSlipId = internalId;
                    source.VoucherPhysical = internalId;
                    source.CostAmountPhysical = -decimal.Round(take * unitCost, 2, MidpointRounding.AwayFromZero);
                    left -= take;
                }
                summary.OnOrder -= inventQty;
                summary.Deducted += inventQty;
                summary.PhysicalInvent -= inventQty;
                summary.AvailPhysical = summary.PhysicalInvent - summary.ReservPhysical - summary.Picked;
                summary.AvailOrdered = summary.AvailPhysical + summary.Ordered - summary.OnOrder - summary.ReservOrdered;
                summary.LastUpdDatePhysical = date;

                line.RemainPurchPhysical += request.Qty;
                line.RemainInventPhysical = Math.Min(0m, line.RemainInventPhysical + inventQty);
                if (line.RemainPurchPhysical >= 0) line.PurchStatus = PurchStatus.Received;
                Db.Set<VendPackingSlipTrans>().Add(new VendPackingSlipTrans
                {
                    DataAreaId = area, PackingSlipId = slipId, InternalPackingSlipId = internalId, OrigPurchId = order.PurchId,
                    VendPackingSlipJour = journal.RecId, DeliveryDate = date, LineNum = line.LineNumber, ItemId = line.ItemId,
                    Name = line.Name, Qty = -request.Qty, InventQty = -inventQty, Ordered = line.PurchQty,
                    Remain = line.RemainPurchPhysical, RemainInvent = line.RemainInventPhysical,
                    ValueMst = -decimal.Round(inventQty * unitCost, 2, MidpointRounding.AwayFromZero),
                    PurchUnit = line.PurchUnit, InventTransId = line.InventTransId, InventDimId = line.InventDimId
                });
                journal.Qty -= request.Qty;
            }
            var lines = await Db.Set<PurchLine>().Where(x => x.DataAreaId == area && x.PurchId == order.PurchId).ToListAsync(ct);
            if (lines.All(x => x.RemainPurchPhysical >= 0)) order.PurchStatus = PurchStatus.Received;
            order.DocumentStatus = DocumentStatus.PackingSlip;
            await unitOfWork.CompleteAsync(ct);
            await unitOfWork.CommitTransactionAsync(ct);
            return new(200, new { packingSlipId = slipId, internalId, qty = -journal.Qty }, "Vendor return shipment posted.");
        }
        catch (SubledgerPostingException ex)
        {
            return await Fail(422, ex.Message, ct);
        }
        catch
        {
            await unitOfWork.RollbackTransactionAsync(ct);
            Db.ChangeTracker.Clear();
            throw;
        }
    }

    // ---------------------------------------------------------------- Vendor credit note (financial issue)

    public Task<PurchResult> CreditAsync(long returnRecId, VendCreditNoteDto input, CancellationToken ct) =>
        Db.Database.CreateExecutionStrategy().ExecuteAsync(() => CreditCoreAsync(returnRecId, input, ct));

    private async Task<PurchResult> CreditCoreAsync(long returnRecId, VendCreditNoteDto input, CancellationToken ct)
    {
        var area = company.GetDataAreaId();
        var creditValidation = await (creditValidator ?? new VendCreditNoteDtoValidator()).ValidateAsync(input, ct);
        if (!creditValidation.IsValid) return new(400, null, creditValidation.Errors[0].ErrorMessage);
        var date = (input.InvoiceDate ?? DateTime.UtcNow).Date;
        var support = new SubledgerPostingSupport(Db);
        await unitOfWork.BeginTransactionAsync(ct);
        try
        {
            var order = await LoadReturnAsync(area, returnRecId, ct);
            if (order == null) return await Fail(404, "Vendor return order was not found in the selected company.", ct);
            await support.LockAsync($"PurchDocument:{area}:{order.PurchId}", ct);
            var creditNoteId = input.InvoiceId.Trim();
            await support.LockAsync($"VendInvoice:{area}:{order.InvoiceAccount}:{creditNoteId}".ToUpperInvariant(), ct);
            if (await Db.Set<VendInvoiceJour>().AnyAsync(x => x.DataAreaId == area
                && x.InvoiceAccount == order.InvoiceAccount && x.InvoiceId == creditNoteId, ct))
                return await Fail(422, $"Vendor document '{creditNoteId}' has already been posted for this vendor.", ct);
            var vendor = await Db.Set<VendTable>().AsNoTracking()
                .FirstOrDefaultAsync(x => x.DataAreaId == area && x.AccountNum == order.InvoiceAccount, ct);
            if (vendor == null) return await Fail(422, "The invoice vendor was not found.", ct);

            var lines = await Db.Set<PurchLine>().Where(x => x.DataAreaId == area && x.PurchId == order.PurchId
                && x.PurchStatus != PurchStatus.Canceled).ToListAsync(ct);
            var selection = lines.Select(x => (Line: x, Qty: x.RemainPurchPhysical - x.RemainPurchFinancial))
                .Where(x => x.Qty > 0).ToList();
            if (selection.Count == 0) return await Fail(422, "There is no shipped, uncredited return quantity.", ct);
            foreach (var originalLineId in selection.Where(x => x.Line.OriginalInvoiceTransRecId.HasValue)
                .Select(x => x.Line.OriginalInvoiceTransRecId!.Value).Distinct().OrderBy(x => x))
                await support.LockAsync($"PurchTaxCredit:{area}:{originalLineId}", ct);

            var context = await support.LoadLedgerContextAsync(area, date,
                LedgerPostingModule.Vendor, _currentUser.GetCurrentUserId(), ct);
            if (string.IsNullOrWhiteSpace(order.CurrencyCode))
                return await Fail(422, "The return order has no currency.", ct);
            // The credit is raised in the order's currency; the inventory it reverses is held in the
            // accounting currency. The two must still agree once translated, which is what the
            // price-variance check below enforces.
            SubledgerCurrencyTranslation translation;
            try
            {
                translation = await SubledgerCurrencyTranslation.LoadAsync(Db, area, context.Ledger,
                    order.CurrencyCode, date, ct);
            }
            catch (SubledgerPostingException ex) { return await Fail(422, ex.Message, ct); }
            var vendorAccount = await support.ResolveVendorSummaryAsync(area, vendor, ct);
            var internalId = (await numbers.NextAsync("VendInvoiceJour", area, ct)).Code;
            var entry = support.NewEntry(area, internalId, internalId, date, context);
            entry.DocumentNumber = creditNoteId;
            Db.Add(entry);
            await unitOfWork.CompleteAsync(ct);
            var currency = context.Ledger.AccountingCurrency;

            decimal total = 0m, totalQty = 0m, totalTax = 0m;
            foreach (var (line, qty) in selection)
            {
                var boundaryError = await InventoryFinancialBoundary.CheckAsync(Db, area, line.ItemId, date, ct);
                if (boundaryError != null) return await Fail(422, boundaryError, ct);
                var inventQty = decimal.Round(qty * line.QtyOrdered / line.PurchQty, 6);
                var amount = decimal.Round(qty * line.PurchPrice / line.PriceUnit, 2, MidpointRounding.AwayFromZero);
                var unitCost = await OriginalReceiptUnitCostAsync(area, line, ct);
                var cost = decimal.Round(inventQty * unitCost, 2, MidpointRounding.AwayFromZero);
                // The credit is in the order's currency and the original cost in the accounting
                // currency, so they are compared after translation.
                if (Math.Abs(translation.ToAccounting(amount) - cost) > 0.01m)
                    return await Fail(422, $"The credit amount for item {line.ItemId} differs from its original receipt cost; price variance posting is outside the basic scope.", ct);

                var origin = await Db.Set<InventTransOrigin>().FirstAsync(x => x.DataAreaId == area
                    && x.InventTransId == line.InventTransId && x.ReferenceId == order.PurchId, ct);
                var deducted = (await Db.Set<InventTrans>().Where(x => x.DataAreaId == area && x.InventTransOrigin == origin.RecId
                    && x.StatusIssue == StatusIssue.Deducted).ToListAsync(ct)).OrderBy(x => x.RecId).ToList();
                if (deducted.Sum(x => -x.Qty) < inventQty)
                    return await Fail(422, $"Shipped return quantity does not cover the credit for item {line.ItemId}.", ct);
                var left = inventQty;
                foreach (var source in deducted)
                {
                    if (left <= 0) break;
                    var available = -source.Qty;
                    var take = Math.Min(available, left);
                    if (take < available)
                        Db.Set<InventTrans>().Add(new InventTrans
                        {
                            DataAreaId = area, InventTransOrigin = source.InventTransOrigin, ItemId = source.ItemId,
                            InventDimId = source.InventDimId, Qty = -(available - take), StatusIssue = StatusIssue.Deducted,
                            StatusReceipt = StatusReceipt.None, ValueOpen = InventTransOpen.Yes, CurrencyCode = source.CurrencyCode,
                            DateExpected = source.DateExpected, DatePhysical = source.DatePhysical, DateStatus = source.DateStatus,
                            DateInvent = source.DateInvent, PackingSlipId = source.PackingSlipId, VoucherPhysical = source.VoucherPhysical,
                            CostAmountPhysical = -decimal.Round((available - take) * unitCost, 2, MidpointRounding.AwayFromZero),
                            ReturnInventTransOrigin = source.ReturnInventTransOrigin
                        });
                    source.Qty = -take;
                    source.StatusIssue = StatusIssue.Sold;
                    source.DateFinancial = date;
                    source.DateStatus = date;
                    source.InvoiceId = internalId;
                    source.Voucher = internalId;
                    source.CostAmountPosted = -decimal.Round(take * unitCost, 2, MidpointRounding.AwayFromZero);
                    left -= take;
                }
                var countError = await InventoryCountProtection.CheckAsync(Db, area, line.ItemId, line.InventDimId, ct);
                if (countError != null) return await Fail(422, countError, ct);
                var summary = await Db.Set<InventSum>().SingleAsync(x => x.DataAreaId == area
                    && x.ItemId == line.ItemId && x.InventDimId == line.InventDimId, ct);
                summary.Deducted -= inventQty;
                summary.PostedQty -= inventQty;
                summary.PostedValue -= cost;

                var vendorScope = await support.VendorScopeAsync(area, order.InvoiceAccount.Length > 0 ? order.InvoiceAccount : order.OrderAccount, ct);
                var inventoryAccount = await support.ResolveInventPostingAsync(area, line.ItemId, InventAccountType.PurchReceipt, ct,
                    custVendAccount: vendorScope.Account, custVendGroup: vendorScope.Group);
                // Posted on the credit's own amount so the voucher balances against the vendor
                // line; its accounting amount equals the original cost, which the check above
                // guarantees.
                Db.Add(translation.AccountEntry(area, entry.RecId, inventoryAccount, LedgerPostingType.PurchReceipt, -amount,
                    $"Return {order.ReturnItemNum} item {line.ItemId}", -inventQty));
                // The account pair a later cost adjustment of this purchase-related transaction must
                // use. Per Microsoft, the offset to "Cost of purchased materials invoiced" is
                // "Purchase expenditure for product", so inventory close adjusts that pair rather
                // than resolving whatever profile is current when it runs.
                var consumption = await support.ResolveInventPostingAsync(area, line.ItemId, InventAccountType.PurchConsump, ct,
                    custVendAccount: vendorScope.Account, custVendGroup: vendorScope.Group);
                Db.Add(IAX.IXApi.Modules.Finance.Inventory.InventTransPosting.InventTransPostingService.Record(
                    area, origin.RecId, line.ItemId, InventTransPostingType.Financial, date, internalId,
                    inventoryAccount, LedgerPostingType.PurchReceipt, consumption, LedgerPostingType.PurchConsump));
                var creditLine = new VendInvoiceTrans
                {
                    DataAreaId = area, InvoiceId = creditNoteId, InternalInvoiceId = internalId, InvoiceDate = date,
                    PurchId = order.PurchId, LineNum = line.LineNumber, ItemId = line.ItemId, Qty = -qty, InventQty = -inventQty,
                    PurchUnit = line.PurchUnit, PurchPrice = line.PurchPrice, PriceUnit = line.PriceUnit, LineAmount = -amount,
                    CurrencyCode = order.CurrencyCode, InventTransId = line.InventTransId, InventDimId = line.InventDimId,
                    TaxGroup = line.TaxGroup, TaxItemGroup = line.TaxItemGroup, OriginalInvoiceTransRecId = line.OriginalInvoiceTransRecId
                };
                Db.Add(creditLine);
                await unitOfWork.CompleteAsync(ct);
                if (line.OriginalInvoiceTransRecId.HasValue)
                    totalTax -= await new PurchaseTaxPosting(Db).PostAsync(area, creditLine, entry.RecId, translation, ct, line.OriginalInvoiceTransRecId);
                line.RemainPurchFinancial += qty;
                line.RemainInventFinancial = Math.Min(0m, line.RemainInventFinancial + inventQty);
                if (line.RemainPurchFinancial >= 0 && line.RemainPurchPhysical >= 0) line.PurchStatus = PurchStatus.Invoiced;
                total += amount;
                totalQty += qty;
            }

            // Derived from the lines already translated, so the accounting currency balances even
            // when each line rounded independently.
            var vendorEntry = translation.BalancingEntry(area, entry.RecId, vendorAccount,
                LedgerPostingType.VendBalance, total + totalTax, $"Vendor credit note {creditNoteId}", 0m);
            Db.Add(vendorEntry);
            var totalAccounting = vendorEntry.AccountingCurrencyAmount;
            Db.Add(new SubledgerVoucherGeneralJournalEntry
            {
                DataAreaId = area, GeneralJournalEntry = entry.RecId, Voucher = internalId, VoucherDataAreaId = area, AccountingDate = date
            });
            var vendorTransaction = new VendTrans
            {
                DataAreaId = area, AccountNum = order.InvoiceAccount, TransDate = date, Voucher = internalId,
                Invoice = creditNoteId, Txt = $"Credit note for return {order.ReturnItemNum}", CurrencyCode = order.CurrencyCode,
                AmountCur = total + totalTax, AmountMst = totalAccounting, DueDate = date, TransType = LedgerTransType.Purch
            };
            Db.Set<VendTrans>().Add(vendorTransaction);
            await unitOfWork.CompleteAsync(ct);
            VendorOpenBalance.Add(Db, vendorTransaction);
            Db.Set<VendInvoiceJour>().Add(new VendInvoiceJour
            {
                DataAreaId = area, InvoiceId = creditNoteId, InternalInvoiceId = internalId, InvoiceDate = date,
                PurchId = order.PurchId, OrderAccount = order.OrderAccount, InvoiceAccount = order.InvoiceAccount,
                CurrencyCode = order.CurrencyCode, LedgerVoucher = internalId, Qty = -totalQty,
                SalesBalance = -total, SumTax = -totalTax, InvoiceAmount = -(total + totalTax), DueDate = date
            });
            if (lines.All(x => x.PurchStatus == PurchStatus.Invoiced)) order.PurchStatus = PurchStatus.Invoiced;
            order.DocumentStatus = DocumentStatus.Invoice;
            await unitOfWork.CompleteAsync(ct);
            await unitOfWork.CommitTransactionAsync(ct);
            return new(200, new { invoiceId = creditNoteId, voucher = internalId, amount = total + totalTax }, "Vendor credit note posted.");
        }
        catch (SubledgerPostingException ex)
        {
            return await Fail(422, ex.Message, ct);
        }
        catch
        {
            await unitOfWork.RollbackTransactionAsync(ct);
            Db.ChangeTracker.Clear();
            throw;
        }
    }

    // Returned goods leave at the cost of the original receipt they reverse (D365 return marking).
    private async Task<decimal> OriginalReceiptUnitCostAsync(string area, PurchLine line, CancellationToken ct)
    {
        var originalOrigin = await Db.Set<InventTransOrigin>().AsNoTracking()
            .FirstOrDefaultAsync(x => x.DataAreaId == area && x.InventTransId == line.InventTransIdReturn, ct)
            ?? throw new SubledgerPostingException($"The original receipt of item {line.ItemId} was not found.");
        var originalInvoice = line.OriginalInvoiceTransRecId.HasValue
            ? await Db.Set<VendInvoiceTrans>().AsNoTracking().SingleOrDefaultAsync(x => x.DataAreaId == area
                && x.RecId == line.OriginalInvoiceTransRecId.Value && x.InventTransId == line.InventTransIdReturn && x.Qty > 0, ct)
            : null;
        if (line.OriginalInvoiceTransRecId.HasValue && (originalInvoice == null || string.IsNullOrWhiteSpace(originalInvoice.InternalInvoiceId)))
            throw new SubledgerPostingException("The selected original vendor invoice line cannot be identified for return costing.");
        if (originalInvoice != null && await Db.Set<VendInvoiceTrans>().AsNoTracking().CountAsync(x => x.DataAreaId == area
                && x.InternalInvoiceId == originalInvoice.InternalInvoiceId && x.InventTransId == originalInvoice.InventTransId && x.Qty > 0, ct) != 1)
            throw new SubledgerPostingException("The selected vendor invoice contains multiple lines for the same inventory origin; return costing cannot identify its movements.");
        var receipts = await Db.Set<InventTrans>().AsNoTracking().Where(x => x.DataAreaId == area
            && x.InventTransOrigin == originalOrigin.RecId && x.StatusReceipt == StatusReceipt.Purchased
            && (originalInvoice == null || x.InvoiceId == originalInvoice.InternalInvoiceId))
            .Select(x => new { x.InvoiceId, x.Qty, x.CostAmountPosted, x.CostAmountAdjustment }).ToListAsync(ct);
        var qty = receipts.Sum(x => x.Qty);
        if (qty <= 0) throw new SubledgerPostingException($"The original receipt of item {line.ItemId} has not been invoiced.");
        if (originalInvoice != null && qty != originalInvoice.InventQty)
            throw new SubledgerPostingException("The selected vendor invoice line does not match its financial inventory quantity.");
        if (originalInvoice == null && receipts.GroupBy(x => x.InvoiceId).Where(x => x.Sum(y => y.Qty) > 0m)
            .Select(x => x.Sum(y => y.CostAmountPosted + y.CostAmountAdjustment) / x.Sum(y => y.Qty)).Distinct().Count() > 1)
            throw new SubledgerPostingException("The original vendor invoices have different inventory costs and the selected invoice line is unavailable.");
        return receipts.Sum(x => x.CostAmountPosted + x.CostAmountAdjustment) / qty;
    }

    private Task<PurchTable?> LoadReturnAsync(string area, long recId, CancellationToken ct) =>
        Db.Set<PurchTable>().FirstOrDefaultAsync(x => x.RecId == recId && x.DataAreaId == area
            && x.PurchaseType == PurchaseType.ReturnItem, ct);

    private async Task<PurchResult> Fail(int status, string message, CancellationToken ct)
    {
        await unitOfWork.RollbackTransactionAsync(ct);
        Db.ChangeTracker.Clear();
        return new(status, null, message);
    }
}
