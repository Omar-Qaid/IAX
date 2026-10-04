using IAX.IXApi.Modules.Finance.AccountsReceivable.SalesOrder.Interfaces;
using IAX.IXApi.Modules.Finance.Common;
using IAX.IXApi.Modules.Finance.Entities;
using IAX.IXApi.Modules.Finance.Persistence;
using IAX.IXApi.Modules.Identity.Permissions;
using IAX.IXApi.Shared.Application.Contracts;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace IAX.IXApi.Modules.Finance.AccountsReceivable;

[ApiController]
[Route("api/v1/SalesTable/{destinationRecId:long}/copy")]
[DomainPermission("AccountsReceivable", "SalesOrders", "View")]
public sealed class SalesOrderCopyController : ControllerBase
{
    private readonly IFinanceDataContext _db;
    private readonly ISalesInventoryDemandService _inventoryDemand;

    public SalesOrderCopyController(IFinanceDataContext db, ISalesInventoryDemandService inventoryDemand)
    {
        _db = db;
        _inventoryDemand = inventoryDemand;
    }

    [HttpGet("from-all/documents")]
    public async Task<IActionResult> SalesOrders(long destinationRecId, CancellationToken ct)
    {
        var destination = await FindDestination(destinationRecId, ct);
        if (destination == null)
            return NotFound(APIResponse<object>.Fail("Destination sales order was not found."));
        var area = destination.DataAreaId;
        var rows = await _db.Set<SalesTable>().AsNoTracking()
            .Where(x => x.DataAreaId == area && x.RecId != destinationRecId
                && _db.Set<SalesLine>().Any(line => line.DataAreaId == area && line.SalesId == x.SalesId))
            .OrderByDescending(x => x.CreatedAt)
            .Select(x => new { id = x.RecId.ToString(), documentNumber = x.SalesId, account = x.CustAccount,
                accountName = x.SalesName, createdAt = x.CreatedAt, currencyCode = x.CurrencyCode,
                sourceType = "salesOrder" })
            .ToListAsync(ct);
        return Ok(APIResponse<object>.Ok(rows));
    }

    [HttpGet("from-all/documents/{sourceRecId:long}/lines")]
    public async Task<IActionResult> SalesOrderLines(long destinationRecId, long sourceRecId, CancellationToken ct)
    {
        var destination = await FindDestination(destinationRecId, ct);
        if (destination == null)
            return NotFound(APIResponse<object>.Fail("Destination sales order was not found."));
        var area = destination.DataAreaId;
        if (destinationRecId == sourceRecId)
            return UnprocessableEntity(APIResponse<object>.Fail("The destination order cannot be used as its own source."));
        var source = await _db.Set<SalesTable>().AsNoTracking()
            .SingleOrDefaultAsync(x => x.RecId == sourceRecId && x.DataAreaId == area, ct);
        if (source == null) return NotFound(APIResponse<object>.Fail("Source sales order was not found."));
        var lines = await _db.Set<SalesLine>().AsNoTracking()
            .Where(x => x.SalesId == source.SalesId && x.DataAreaId == area)
            .OrderBy(x => x.LineNum).ToListAsync(ct);
        return Ok(APIResponse<object>.Ok(await PresentLines(lines.Select(x => new CopySourceLine(
            x.RecId, x.ItemId, x.Name, x.SalesQty, x.SalesUnit, x.SalesPrice, x.PriceUnit,
            x.LineDisc, x.LinePercent, x.MultiLnDisc, x.MultiLnPercent, x.LineAmount,
            x.InventDimId, x.TaxGroup, x.TaxItemGroup, x.DlvMode, x.DlvTerm,
            x.ReceiptDateRequested, x.ShippingDateRequested, x.SalesCategory)).ToList(), area, ct)));
    }

    [HttpGet("from-journal/documents")]
    public async Task<IActionResult> Journals(long destinationRecId, CancellationToken ct)
    {
        var destination = await FindDestination(destinationRecId, ct);
        if (destination == null)
            return NotFound(APIResponse<object>.Fail("Destination sales order was not found."));
        var area = destination.DataAreaId;
        var rows = await _db.Set<CustConfirmJour>().AsNoTracking()
            .Where(x => x.DataAreaId == area
                && _db.Set<CustConfirmTrans>().Any(line => line.DataAreaId == area && line.ConfirmId == x.ConfirmId))
            .OrderByDescending(x => x.ConfirmDate)
            .Select(x => new { id = x.RecId.ToString(), documentNumber = x.ConfirmId, account = x.OrderAccount,
                accountName = x.DeliveryName, createdAt = x.ConfirmDate, currencyCode = x.CurrencyCode,
                sourceType = "confirmation" })
            .ToListAsync(ct);
        return Ok(APIResponse<object>.Ok(rows));
    }

    [HttpGet("from-journal/documents/{sourceRecId:long}/lines")]
    public async Task<IActionResult> JournalLines(long destinationRecId, long sourceRecId, CancellationToken ct)
    {
        var destination = await FindDestination(destinationRecId, ct);
        if (destination == null)
            return NotFound(APIResponse<object>.Fail("Destination sales order was not found."));
        var area = destination.DataAreaId;
        var journal = await _db.Set<CustConfirmJour>().AsNoTracking()
            .SingleOrDefaultAsync(x => x.RecId == sourceRecId && x.DataAreaId == area, ct);
        if (journal == null) return NotFound(APIResponse<object>.Fail("Confirmation journal was not found."));
        var lines = await _db.Set<CustConfirmTrans>().AsNoTracking()
            .Where(x => x.ConfirmId == journal.ConfirmId && x.DataAreaId == area)
            .OrderBy(x => x.LineNum).ToListAsync(ct);
        return Ok(APIResponse<object>.Ok(await PresentLines(lines.Select(x => new CopySourceLine(
            x.RecId, x.ItemId, x.Name, x.Qty, x.SalesUnit, x.SalesPrice, x.PriceUnit,
            x.LineDisc, x.LinePercent, x.MultiLnDisc, x.MultiLnPercent, x.LineAmount,
            x.InventDimId, x.TaxGroup, x.TaxItemGroup, string.Empty, x.DlvTerm,
            x.DlvDate, default, x.SalesCategory)).ToList(), area, ct)));
    }

    public sealed record CopyRequest(string Mode, List<string> LineIds, decimal QuantityFactor = 1,
        bool InvertSign = false, bool RecalculatePrice = false, bool CopyPrecisely = true);

    [HttpPost]
    [DomainPermission("AccountsReceivable", "SalesOrders", "Edit")]
    public async Task<IActionResult> Copy(long destinationRecId, [FromBody] CopyRequest request, CancellationToken ct)
    {
        if (request == null || request.LineIds == null)
            return BadRequest(APIResponse<object>.Fail("A copy request is required."));
        if (!string.Equals(request.Mode, "fromAll", StringComparison.OrdinalIgnoreCase)
            && !string.Equals(request.Mode, "fromJournal", StringComparison.OrdinalIgnoreCase))
            return BadRequest(APIResponse<object>.Fail("Copy mode must be fromAll or fromJournal."));
        if (request.LineIds.Count == 0)
            return BadRequest(APIResponse<object>.Fail("Select at least one source line."));
        var lineIds = new List<long>(request.LineIds.Count);
        foreach (var value in request.LineIds.Distinct())
        {
            if (!long.TryParse(value, out var id))
                return BadRequest(APIResponse<object>.Fail("One or more source line identifiers are invalid."));
            lineIds.Add(id);
        }
        if (request.QuantityFactor <= 0)
            return BadRequest(APIResponse<object>.Fail("Quantity factor must be greater than zero."));

        return await _db.Database.CreateExecutionStrategy().ExecuteAsync(async () =>
        {
            await using var transaction = await _db.Database.BeginTransactionAsync(System.Data.IsolationLevel.Serializable, ct);
            var destination = await _db.Set<SalesTable>().SingleOrDefaultAsync(
                x => x.RecId == destinationRecId, ct);
            if (destination == null) return (IActionResult)NotFound(APIResponse<object>.Fail("Destination sales order was not found."));
            var area = destination.DataAreaId;
            if (destination.SalesStatus != SalesStatus.Backorder)
                return UnprocessableEntity(APIResponse<object>.Fail("Lines can only be copied to an open sales order."));

            List<CopySourceLine> sources;
            if (request.Mode.Equals("fromJournal", StringComparison.OrdinalIgnoreCase))
            {
                sources = await _db.Set<CustConfirmTrans>().AsNoTracking()
                    .Where(x => lineIds.Contains(x.RecId) && x.DataAreaId == area)
                    .Select(x => new CopySourceLine(x.RecId, x.ItemId, x.Name, x.Qty, x.SalesUnit,
                        x.SalesPrice, x.PriceUnit, x.LineDisc, x.LinePercent, x.MultiLnDisc,
                        x.MultiLnPercent, x.LineAmount, x.InventDimId, x.TaxGroup, x.TaxItemGroup,
                        string.Empty, x.DlvTerm, x.DlvDate, default, x.SalesCategory)).ToListAsync(ct);
            }
            else
            {
                sources = await _db.Set<SalesLine>().AsNoTracking()
                    .Where(x => lineIds.Contains(x.RecId) && x.DataAreaId == area
                        && x.SalesId != destination.SalesId)
                    .Select(x => new CopySourceLine(x.RecId, x.ItemId, x.Name, x.SalesQty, x.SalesUnit,
                        x.SalesPrice, x.PriceUnit, x.LineDisc, x.LinePercent, x.MultiLnDisc,
                        x.MultiLnPercent, x.LineAmount, x.InventDimId, x.TaxGroup, x.TaxItemGroup,
                        x.DlvMode, x.DlvTerm, x.ReceiptDateRequested, x.ShippingDateRequested,
                        x.SalesCategory)).ToListAsync(ct);
            }
            if (sources.Count != lineIds.Count)
                return UnprocessableEntity(APIResponse<object>.Fail("One or more selected source lines were not found."));

            var dimensionIds = sources.Select(x => x.InventDimId).Where(x => x != string.Empty).Distinct().ToList();
            var dimensions = await _db.Set<InventDim>().AsNoTracking()
                .Where(x => x.DataAreaId == area && dimensionIds.Contains(x.InventDimId))
                .ToDictionaryAsync(x => x.InventDimId, ct);
            var nextLine = await _db.Set<SalesLine>().Where(x => x.SalesId == destination.SalesId && x.DataAreaId == area)
                .MaxAsync(x => (decimal?)x.LineNum, ct) ?? 0;
            var created = 0;
            foreach (var source in sources.OrderBy(x => x.Id))
            {
                var sign = request.InvertSign ? -1 : 1;
                var quantity = source.Quantity * request.QuantityFactor * sign;
                if (quantity == 0) continue;
                var price = source.UnitPrice;
                if (request.RecalculatePrice)
                {
                    var module = await _db.Set<InventTableModule>().AsNoTracking().FirstOrDefaultAsync(
                        x => x.ItemId == source.ItemId && x.DataAreaId == area && (int)x.ModuleType == 2, ct);
                    if (module != null) price = module.Price / (module.PriceUnit > 0 ? module.PriceUnit : 1);
                }
                var line = new SalesLine
                {
                    SalesId = destination.SalesId, LineNum = ++nextLine, ItemId = source.ItemId,
                    Name = source.Name, CustAccount = destination.CustAccount, CustGroupId = destination.CustGroup,
                    CurrencyCode = destination.CurrencyCode, SalesQty = quantity, QtyOrdered = quantity,
                    RemainSalesPhysical = quantity, RemainSalesFinancial = quantity,
                    SalesUnit = source.Unit, PriceUnit = source.PriceUnit > 0 ? source.PriceUnit : 1,
                    SalesPrice = price, LineAmount = quantity * price,
                    LineDisc = request.CopyPrecisely ? source.LineDiscount * request.QuantityFactor * sign : 0,
                    LinePercent = request.CopyPrecisely ? source.LineDiscountPercent : 0,
                    MultiLnDisc = request.CopyPrecisely ? source.MultiLineDiscount * request.QuantityFactor * sign : 0,
                    MultiLnPercent = request.CopyPrecisely ? source.MultiLineDiscountPercent : 0,
                    TaxGroup = source.TaxGroup, TaxItemGroup = source.TaxItemGroup,
                    DlvMode = source.DeliveryMode, DlvTerm = source.DeliveryTerms,
                    ReceiptDateRequested = source.DeliveryDate == default ? destination.ReceiptDateRequested : source.DeliveryDate,
                    ShippingDateRequested = source.ShippingDate == default ? destination.ShippingDateRequested : source.ShippingDate,
                    SalesCategory = source.SalesCategory, SalesStatus = SalesStatus.Backorder,
                    SalesType = destination.SalesType ?? SalesType.Sales, DataAreaId = area
                };
                dimensions.TryGetValue(source.InventDimId, out var dimension);
                await _inventoryDemand.CreateAsync(destination, line,
                    dimension?.InventSiteId ?? destination.InventSiteId,
                    dimension?.InventLocationId ?? destination.InventLocationId, ct);
                _db.Set<SalesLine>().Add(line);
                destination.SmmSalesAmountTotal += line.LineAmount;
                created++;
            }
            await _db.SaveChangesAsync(ct);
            await transaction.CommitAsync(ct);
            return Ok(APIResponse<object>.Ok(new { copiedLineCount = created }));
        });
    }

    private async Task<object> PresentLines(List<CopySourceLine> lines, string area, CancellationToken ct)
    {
        var ids = lines.Select(x => x.InventDimId).Where(x => x != string.Empty).Distinct().ToList();
        var dims = await _db.Set<InventDim>().AsNoTracking().Where(x => x.DataAreaId == area && ids.Contains(x.InventDimId))
            .ToDictionaryAsync(x => x.InventDimId, ct);
        return lines.Select(x => new { id = x.Id.ToString(), x.ItemId, description = x.Name, quantity = x.Quantity,
            unit = x.Unit, unitPrice = x.UnitPrice, netAmount = x.LineAmount,
            discount = x.LineDiscount, discountPercent = x.LineDiscountPercent,
            site = dims.TryGetValue(x.InventDimId, out var d) ? d.InventSiteId : string.Empty,
            warehouse = dims.TryGetValue(x.InventDimId, out d) ? d.InventLocationId : string.Empty });
    }

    private Task<SalesTable?> FindDestination(long destinationRecId, CancellationToken ct)
        => _db.Set<SalesTable>().AsNoTracking().SingleOrDefaultAsync(
            x => x.RecId == destinationRecId, ct);

    private sealed record CopySourceLine(long Id, string ItemId, string Name, decimal Quantity,
        string Unit, decimal UnitPrice, decimal PriceUnit, decimal LineDiscount,
        decimal LineDiscountPercent, decimal MultiLineDiscount, decimal MultiLineDiscountPercent,
        decimal LineAmount, string InventDimId, string TaxGroup, string TaxItemGroup,
        string DeliveryMode, string DeliveryTerms, DateTime DeliveryDate, DateTime ShippingDate,
        long SalesCategory);
}
