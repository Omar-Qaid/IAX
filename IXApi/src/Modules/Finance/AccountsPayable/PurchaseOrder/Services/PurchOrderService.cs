using IAX.IXApi.Infrastructure.Persistence.Repositories;
using IAX.IXApi.Modules.Administration.NumberSequences;
using IAX.IXApi.Modules.Finance.AccountsReceivable;
using IAX.IXApi.Modules.Finance.Common;
using IAX.IXApi.Modules.Finance.Entities;
using IAX.IXApi.Shared.Application.Identity;
using FluentValidation;
using Microsoft.EntityFrameworkCore;

namespace IAX.IXApi.Modules.Finance.AccountsPayable;

/// <summary>
/// Purchase order header/line maintenance. Adding a line for a stocked item creates its Ordered
/// inventory receipt (D365: approved PO creates ordered inbound quantity, no physical stock).
/// </summary>
public sealed class PurchOrderService(IUnitOfWork unitOfWork, ICompanyExecutionContext company,
    ISysNumberSequenceService numbers, IPurchInventorySupplyService supply,
    SalesUnitConversionService units, IValidator<PurchOrderCreateDto>? createValidator = null,
    IValidator<PurchLineInputDto>? lineValidator = null) : IPurchOrderService
{
    private DbContext Db => unitOfWork.Context;

    public async Task<PurchResult> CreateAsync(PurchOrderCreateDto input, CancellationToken ct)
    {
        var area = company.GetDataAreaId();
        if (string.IsNullOrWhiteSpace(area)) return new(400, null, "A company must be selected.");
        var vendor = await Db.Set<VendTable>().AsNoTracking()
            .FirstOrDefaultAsync(x => x.DataAreaId == area && x.AccountNum == input.OrderAccount, ct);
        if (vendor == null) return new(422, null, "The vendor was not found in the selected company.");
        if (vendor.Blocked == CustVendBlocked.All)
            return new(422, null, "The vendor is blocked for purchasing.");
        var createValidation = await (createValidator ?? new PurchOrderCreateDtoValidator()).ValidateAsync(input, ct);
        if (!createValidation.IsValid) return new(422, null, createValidation.Errors[0].ErrorMessage);
        var siteError = await ValidateSiteWarehouseAsync(area, input.InventSiteId, input.InventLocationId, ct);
        if (siteError != null) return new(422, null, siteError);

        var order = new PurchTable
        {
            DataAreaId = area,
            PurchId = (await numbers.NextAsync("PurchTable", area, ct)).Code,
            OrderAccount = vendor.AccountNum,
            InvoiceAccount = string.IsNullOrWhiteSpace(input.InvoiceAccount)
                ? (string.IsNullOrWhiteSpace(vendor.InvoiceAccount) ? vendor.AccountNum : vendor.InvoiceAccount)
                : input.InvoiceAccount.Trim(),
            PurchName = string.IsNullOrWhiteSpace(input.PurchName) ? vendor.AccountNum : input.PurchName.Trim(),
            CurrencyCode = string.IsNullOrWhiteSpace(input.CurrencyCode) ? vendor.Currency : input.CurrencyCode.Trim(),
            Payment = vendor.PaymTermId,
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
        await unitOfWork.CompleteAsync(ct);
        return new(200, order, "Purchase order created.");
    }

    public Task<PurchResult> AddLineAsync(long recId, PurchLineInputDto input, CancellationToken ct) =>
        Db.Database.CreateExecutionStrategy().ExecuteAsync(() => AddLineCoreAsync(recId, input, ct));

    private async Task<PurchResult> AddLineCoreAsync(long recId, PurchLineInputDto input, CancellationToken ct)
    {
        var area = company.GetDataAreaId();
        await unitOfWork.BeginTransactionAsync(ct);
        try
        {
            var order = await Db.Set<PurchTable>().FirstOrDefaultAsync(x => x.RecId == recId && x.DataAreaId == area, ct);
            if (order == null) return await Fail(404, "Purchase order was not found in the selected company.", ct);
            if (order.PurchStatus != PurchStatus.Backorder)
                return await Fail(422, "Lines can only be added to an open purchase order.", ct);
            if (order.PurchaseType == PurchaseType.ReturnItem)
                return await Fail(422, "Return order lines are created from the original vendor invoice.", ct);
            var lineValidation = await (lineValidator ?? new PurchLineInputDtoValidator()).ValidateAsync(input, ct);
            if (!lineValidation.IsValid) return await Fail(422, lineValidation.Errors[0].ErrorMessage, ct);

            var item = await Db.Set<InventTable>().AsNoTracking()
                .FirstOrDefaultAsync(x => x.DataAreaId == area && x.ItemId == input.ItemId, ct);
            if (item == null) return await Fail(422, "The item is not released to the selected company.", ct);
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
            var inventQty = await units.ToInventoryQuantityAsync(item, unit, input.PurchQty, ct);
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
                PurchPrice = input.PurchPrice, PriceUnit = priceUnit,
                LineAmount = input.PurchQty * input.PurchPrice / priceUnit,
                RemainPurchPhysical = input.PurchQty, RemainPurchFinancial = input.PurchQty,
                RemainInventPhysical = inventQty.Value, RemainInventFinancial = inventQty.Value,
                PurchStatus = PurchStatus.Backorder,
                DeliveryDate = (input.DeliveryDate ?? order.DeliveryDate).Date
            };
            await supply.CreateAsync(order, line, site, warehouse, ct);
            Db.Set<PurchLine>().Add(line);
            if (order.DocumentStatus == DocumentStatus.PurchaseOrder) order.DocumentStatus = DocumentStatus.None;
            await unitOfWork.CompleteAsync(ct);
            await unitOfWork.CommitTransactionAsync(ct);
            return new(200, line, "Purchase line added.");
        }
        catch
        {
            await unitOfWork.RollbackTransactionAsync(ct);
            Db.ChangeTracker.Clear();
            throw;
        }
    }

    public async Task<PurchResult> GetAsync(long recId, CancellationToken ct)
    {
        var area = company.GetDataAreaId();
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
        await unitOfWork.RollbackTransactionAsync(ct);
        Db.ChangeTracker.Clear();
        return new(status, null, message);
    }
}
