using IAX.IXApi.Infrastructure.Identity;
using IAX.IXApi.Infrastructure.Persistence.Repositories;
using IAX.IXApi.Infrastructure.Persistence.Services;
using IAX.IXApi.Modules.Administration.NumberSequences;
using IAX.IXApi.Modules.Finance.AccountsReceivable;
using IAX.IXApi.Modules.Finance.Common;
using IAX.IXApi.Modules.Finance.Entities;
using IAX.IXApi.Modules.Finance.Inventory.ProductLifecycle;
using IAX.IXApi.Shared.Application.Identity;
using FluentValidation;
using Microsoft.EntityFrameworkCore;

namespace IAX.IXApi.Modules.Finance.AccountsPayable;

/// <summary>
/// Purchase order header/line maintenance. Adding a line for a stocked item creates its Ordered
/// inventory receipt (D365: approved PO creates ordered inbound quantity, no physical stock).
/// </summary>
public sealed class PurchOrderService : BaseService<PurchTable>, IPurchOrderService
{
    private readonly ICompanyExecutionContext _company;
    private readonly ISysNumberSequenceService _numbers;
    private readonly IPurchInventorySupplyService _supply;
    private readonly SalesUnitConversionService _units;
    private readonly IValidator<PurchOrderCreateDto>? _createValidator;
    private readonly IValidator<PurchLineInputDto>? _lineValidator;

    private DbContext Db => _unitOfWork.Context;

    public PurchOrderService(
        IUnitOfWork unitOfWork,
        ICurrentUserService currentUser,
        ICompanyExecutionContext company,
        ISysNumberSequenceService numbers,
        IPurchInventorySupplyService supply,
        SalesUnitConversionService units,
        IValidator<PurchOrderCreateDto>? createValidator = null,
        IValidator<PurchLineInputDto>? lineValidator = null)
        : base(unitOfWork, currentUser)
    {
        _company = company;
        _numbers = numbers;
        _supply = supply;
        _units = units;
        _createValidator = createValidator;
        _lineValidator = lineValidator;
    }

    public PurchOrderService(
        IUnitOfWork unitOfWork,
        ICompanyExecutionContext company,
        ISysNumberSequenceService numbers,
        IPurchInventorySupplyService supply,
        SalesUnitConversionService units,
        IValidator<PurchOrderCreateDto>? createValidator = null,
        IValidator<PurchLineInputDto>? lineValidator = null)
        : this(unitOfWork, new NullCurrentUserService(company), company, numbers, supply, units, createValidator, lineValidator)
    {
    }

    private sealed class NullCurrentUserService(ICompanyExecutionContext company) : ICurrentUserService
    {
        public string GetCurrentUserId() => string.Empty;
        public string GetOwnerAccountId() => string.Empty;
        public string GetDataAreaId() => company.GetDataAreaId();
    }

    public async Task<PurchResult> CreateAsync(PurchOrderCreateDto input, CancellationToken ct)
    {
        var area = _company.GetDataAreaId();
        if (string.IsNullOrWhiteSpace(area)) return new(400, null, "A company must be selected.");
        var vendor = await Db.Set<VendTable>().AsNoTracking()
            .FirstOrDefaultAsync(x => x.DataAreaId == area && x.AccountNum == input.OrderAccount, ct);
        if (vendor == null) return new(422, null, "The vendor was not found in the selected company.");
        if (vendor.Blocked == CustVendBlocked.All)
            return new(422, null, "The vendor is blocked for purchasing.");
        var createValidation = await (_createValidator ?? new PurchOrderCreateDtoValidator()).ValidateAsync(input, ct);
        if (!createValidation.IsValid) return new(422, null, createValidation.Errors[0].ErrorMessage);
        var siteError = await ValidateSiteWarehouseAsync(area, input.InventSiteId, input.InventLocationId, ct);
        if (siteError != null) return new(422, null, siteError);

        var order = new PurchTable
        {
            DataAreaId = area,
            PurchId = (await _numbers.NextAsync("PurchTable", area, ct)).Code,
            OrderAccount = vendor.AccountNum,
            InvoiceAccount = string.IsNullOrWhiteSpace(input.InvoiceAccount)
                ? (string.IsNullOrWhiteSpace(vendor.InvoiceAccount) ? vendor.AccountNum : vendor.InvoiceAccount)
                : input.InvoiceAccount.Trim(),
            PurchName = string.IsNullOrWhiteSpace(input.PurchName) ? vendor.AccountNum : input.PurchName.Trim(),
            CurrencyCode = string.IsNullOrWhiteSpace(input.CurrencyCode) ? vendor.Currency : input.CurrencyCode.Trim(),
            Payment = vendor.PaymTermId, TaxGroup = vendor.TaxGroup, DefaultDimension = vendor.DefaultDimension ?? 0,
            OrderDate = DateTime.UtcNow.Date,
            DeliveryDate = (input.DeliveryDate ?? DateTime.UtcNow).Date,
            InventSiteId = input.InventSiteId.Trim(),
            InventLocationId = input.InventLocationId.Trim(),
            PurchStatus = PurchStatus.Backorder,
            DocumentStatus = DocumentStatus.None
        };
        if (string.IsNullOrWhiteSpace(order.CurrencyCode))
            return new(422, null, "A currency is required for the purchase order.");
        Db.Set<PurchTable>().Add(order);
        await _unitOfWork.CompleteAsync(ct);
        return new(200, order, "Purchase order created.");
    }

    public Task<PurchResult> AddLineAsync(long recId, PurchLineInputDto input, CancellationToken ct) =>
        Db.Database.CreateExecutionStrategy().ExecuteAsync(() => AddLineCoreAsync(recId, input, ct));

    private async Task<PurchResult> AddLineCoreAsync(long recId, PurchLineInputDto input, CancellationToken ct)
    {
        var area = _company.GetDataAreaId();
        await _unitOfWork.BeginTransactionAsync(ct);
        try
        {
            var order = await Db.Set<PurchTable>().FirstOrDefaultAsync(x => x.RecId == recId && x.DataAreaId == area, ct);
            if (order == null) return await Fail(404, "Purchase order was not found in the selected company.", ct);
            if (order.PurchStatus != PurchStatus.Backorder)
                return await Fail(422, "Lines can only be added to an open purchase order.", ct);
            if (order.PurchaseType == PurchaseType.ReturnItem)
                return await Fail(422, "Return order lines are created from the original vendor invoice.", ct);
            var lineValidation = await (_lineValidator ?? new PurchLineInputDtoValidator()).ValidateAsync(input, ct);
            if (!lineValidation.IsValid) return await Fail(422, lineValidation.Errors[0].ErrorMessage, ct);

            var item = await Db.Set<InventTable>().AsNoTracking()
                .FirstOrDefaultAsync(x => x.DataAreaId == area && x.ItemId == input.ItemId, ct);
            if (item == null) return await Fail(422, "The item is not released to the selected company.", ct);
            var lifecycleError = await ProductLifecyclePolicy.ValidateAsync(Db, area, item.ItemId,
                ProductLifecycleProcess.Purchase, ct);
            if (lifecycleError != null) return await Fail(422, lifecycleError, ct);
            var assignment = await Db.Set<InventModelGroupItem>().AsNoTracking()
                .FirstOrDefaultAsync(x => x.ItemDataAreaId == area && x.ItemId == item.ItemId, ct);
            var model = assignment == null ? null : await Db.Set<InventModelGroup>().AsNoTracking()
                .FirstOrDefaultAsync(x => x.DataAreaId == area && x.ModelGroupId == assignment.ModelGroupId, ct);
            if (model == null || model.StockedProduct != NoYes.Yes)
                return await Fail(422, $"Item '{item.ItemId}' must have a stocked item model group.", ct);

            var purchModule = await Db.Set<InventTableModule>().AsNoTracking().FirstOrDefaultAsync(x => x.DataAreaId == area
                && x.ItemId == item.ItemId && x.ModuleType == ModuleInventPurchSales.Purchase, ct);
            var unit = string.IsNullOrWhiteSpace(input.PurchUnit) ? purchModule?.UnitId ?? string.Empty : input.PurchUnit.Trim();
            if (string.IsNullOrWhiteSpace(unit)) return await Fail(422, $"A purchase unit is required for item '{item.ItemId}'.", ct);
            var inventQty = await _units.ToInventoryQuantityAsync(item, unit, input.PurchQty, ct);
            if (inventQty == null || inventQty <= 0)
                return await Fail(422, "A conversion from the purchase unit to the item's inventory unit is required.", ct);

            var site = string.IsNullOrWhiteSpace(input.InventSiteId) ? order.InventSiteId : input.InventSiteId.Trim();
            var warehouse = string.IsNullOrWhiteSpace(input.InventLocationId) ? order.InventLocationId : input.InventLocationId.Trim();
            var siteError = await ValidateSiteWarehouseAsync(area, site, warehouse, ct);
            if (siteError != null) return await Fail(422, siteError, ct);

            var priceUnit = input.PriceUnit.GetValueOrDefault() > 0 ? input.PriceUnit!.Value : 1m;
            var lineNumbers = await Db.Set<PurchLine>().Where(x => x.DataAreaId == area && x.PurchId == order.PurchId)
                .Select(x => x.LineNumber).ToListAsync(ct);
            var lastLine = lineNumbers.Count == 0 ? 0m : lineNumbers.Max();
            var line = new PurchLine
            {
                DataAreaId = area, PurchId = order.PurchId, LineNumber = lastLine + 1, ItemId = item.ItemId,
                Name = string.IsNullOrWhiteSpace(input.Name) ? item.NameAlias : input.Name.Trim(),
                VendAccount = order.OrderAccount, CurrencyCode = order.CurrencyCode,
                PurchQty = input.PurchQty, PurchUnit = unit, QtyOrdered = inventQty.Value,
                PurchPrice = input.PurchPrice, PriceUnit = priceUnit, TaxGroup = order.TaxGroup, TaxItemGroup = purchModule?.TaxItemGroupId ?? string.Empty,
                LineAmount = input.PurchQty * input.PurchPrice / priceUnit,
                RemainPurchPhysical = input.PurchQty, RemainPurchFinancial = input.PurchQty,
                RemainInventPhysical = inventQty.Value, RemainInventFinancial = inventQty.Value,
                PurchStatus = PurchStatus.Backorder,
                DeliveryDate = (input.DeliveryDate ?? order.DeliveryDate).Date
            };
            await _supply.CreateAsync(order, line, site, warehouse, ct);
            Db.Set<PurchLine>().Add(line);
            if (order.DocumentStatus == DocumentStatus.PurchaseOrder) order.DocumentStatus = DocumentStatus.None;
            await _unitOfWork.CompleteAsync(ct);
            await _unitOfWork.CommitTransactionAsync(ct);
            return new(200, line, "Purchase line added.");
        }
        catch
        {
            await _unitOfWork.RollbackTransactionAsync(ct);
            Db.ChangeTracker.Clear();
            throw;
        }
    }

    public Task<PurchResult> CancelRemainderAsync(long recId, CancellationToken ct) =>
        Db.Database.CreateExecutionStrategy().ExecuteAsync(() => CancelRemainderCoreAsync(recId, ct));

    /// <summary>
    /// Cancels the quantity a purchase order has not received, which is what D365's purchase order
    /// cancellation does: only the part that has not been registered, received or invoiced goes
    /// away, and the received part stays processable.
    /// <para>
    /// A quantity that was received but not yet invoiced therefore stays invoiceable. The physical
    /// remainder goes to zero, and the financial remainder is reduced by that same unreceived
    /// quantity - those units will never arrive, so they can never be invoiced - which leaves
    /// exactly the received-but-uninvoiced quantity behind. Example: order 10, receive 6, invoice
    /// none, then cancel. The physical remainder was 4 and the financial remainder 10; afterwards
    /// they are 0 and 6, so the 6 delivered units can still be invoiced and nothing already posted
    /// is disturbed.
    /// </para>
    /// <para>
    /// A line that never received anything becomes Canceled and drops out of the header's status
    /// derivation, which is why the receipt and invoice flows already exclude cancelled lines. A
    /// line holding a received remainder keeps its real status so it can still be invoiced.
    /// </para>
    /// </summary>
    private async Task<PurchResult> CancelRemainderCoreAsync(long recId, CancellationToken ct)
    {
        var area = _company.GetDataAreaId();
        await _unitOfWork.BeginTransactionAsync(ct);
        try
        {
            var order = await Db.Set<PurchTable>().FirstOrDefaultAsync(x => x.RecId == recId && x.DataAreaId == area, ct);
            if (order == null) return await Fail(404, "Purchase order was not found in the selected company.", ct);
            if (order.PurchStatus == PurchStatus.Canceled)
                return await Fail(422, "The purchase order is already canceled.", ct);

            var lines = await Db.Set<PurchLine>()
                .Where(x => x.DataAreaId == area && x.PurchId == order.PurchId
                    && x.PurchStatus != PurchStatus.Canceled).ToListAsync(ct);
            if (lines.All(x => x.RemainPurchPhysical <= 0))
                return await Fail(422, "The purchase order has no unreceived quantity left to cancel.", ct);

            var cancelled = new List<object>();
            foreach (var line in lines)
            {
                if (line.RemainPurchPhysical <= 0) continue;
                var purchQty = line.RemainPurchPhysical;
                decimal inventQty;
                try
                {
                    inventQty = await _supply.CancelRemainingAsync(order, line, ct);
                }
                catch (InvalidOperationException error)
                {
                    return await Fail(422, error.Message, ct);
                }

                var receivedNothing = line.RemainPurchPhysical >= line.PurchQty;
                // Units that will never arrive can never be invoiced, so the financial remainder
                // loses the same quantity the physical one does and keeps only what was received.
                line.RemainPurchFinancial = Math.Max(0m, line.RemainPurchFinancial - purchQty);
                line.RemainInventFinancial = Math.Max(0m, line.RemainInventFinancial - inventQty);
                line.RemainPurchPhysical = 0;
                line.RemainInventPhysical = 0;
                line.PurchStatus = receivedNothing ? PurchStatus.Canceled
                    : line.RemainPurchFinancial <= 0 ? PurchStatus.Invoiced
                    : PurchStatus.Received;
                cancelled.Add(new
                {
                    lineNumber = line.LineNumber, itemId = line.ItemId, purchQty, inventQty,
                    status = line.PurchStatus, remainToInvoice = line.RemainPurchFinancial
                });
            }

            // Derived from the lines that still mean something, matching how the receipt and invoice
            // flows derive it: a wholly cancelled order is Canceled, otherwise the order follows
            // whatever its surviving lines still owe.
            var surviving = lines.Where(x => x.PurchStatus != PurchStatus.Canceled).ToList();
            order.PurchStatus = surviving.Count == 0 ? PurchStatus.Canceled
                : surviving.All(x => x.RemainPurchFinancial <= 0 && x.RemainPurchPhysical <= 0) ? PurchStatus.Invoiced
                : PurchStatus.Received;

            await _unitOfWork.CompleteAsync(ct);
            await _unitOfWork.CommitTransactionAsync(ct);
            return new(200, new { purchId = order.PurchId, status = order.PurchStatus, lines = cancelled },
                "Purchase order remainder canceled.");
        }
        catch
        {
            await _unitOfWork.RollbackTransactionAsync(ct);
            Db.ChangeTracker.Clear();
            throw;
        }
    }

    public async Task<PurchResult> GetAsync(long recId, CancellationToken ct)
    {
        var area = _company.GetDataAreaId();
        var order = await Db.Set<PurchTable>().AsNoTracking().FirstOrDefaultAsync(x => x.RecId == recId && x.DataAreaId == area, ct);
        if (order == null) return new(404, null, "Purchase order was not found in the selected company.");
        var lines = await Db.Set<PurchLine>().AsNoTracking()
            .Where(x => x.DataAreaId == area && x.PurchId == order.PurchId).ToListAsync(ct);
        return new(200, new { header = order, lines = lines.OrderBy(x => x.LineNumber).ToList() });
    }

    private async Task<string?> ValidateSiteWarehouseAsync(string area, string site, string warehouse, CancellationToken ct)
    {
        if (string.IsNullOrWhiteSpace(site) || string.IsNullOrWhiteSpace(warehouse))
            return "Site and warehouse are required.";
        var location = await Db.Set<InventLocation>().AsNoTracking()
            .FirstOrDefaultAsync(x => x.DataAreaId == area && x.InventLocationId == warehouse.Trim(), ct);
        if (location == null) return $"Warehouse '{warehouse}' was not found.";
        return string.Equals(location.InventSiteId, site.Trim(), StringComparison.OrdinalIgnoreCase)
            ? null : $"Warehouse '{warehouse}' does not belong to site '{site}'.";
    }

    private async Task<PurchResult> Fail(int status, string message, CancellationToken ct)
    {
        await _unitOfWork.RollbackTransactionAsync(ct);
        Db.ChangeTracker.Clear();
        return new(status, null, message);
    }
}
