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
    private readonly SalesOrderCopyService _copy;

    public SalesOrderCopyController(IFinanceDataContext db, SalesOrderCopyService copy)
    {
        _db = db;
        _copy = copy;
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
        bool InvertSign = false, bool RecalculatePrice = false, bool CopyPrecisely = true)
        : SalesOrderCopyRequest(Mode, LineIds, QuantityFactor, InvertSign, RecalculatePrice, CopyPrecisely);

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

        var result = await _copy.CopyAsync(destinationRecId, request, lineIds, ct);
        return result.Status switch
        {
            SalesOrderCopyStatus.DestinationNotFound => NotFound(APIResponse<object>.Fail("Destination sales order was not found.")),
            SalesOrderCopyStatus.DestinationClosed => UnprocessableEntity(APIResponse<object>.Fail("Lines can only be copied to an open sales order.")),
            SalesOrderCopyStatus.SourceLinesNotFound => UnprocessableEntity(APIResponse<object>.Fail("One or more selected source lines were not found.")),
            SalesOrderCopyStatus.InvalidUnit => UnprocessableEntity(APIResponse<object>.Fail("A source line has no valid conversion to its inventory unit.")),
            _ => Ok(APIResponse<object>.Ok(new { copiedLineCount = result.CopiedLineCount }))
        };
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

}
