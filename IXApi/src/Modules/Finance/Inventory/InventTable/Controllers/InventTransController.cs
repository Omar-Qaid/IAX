using IAX.IXApi.Modules.Finance.Entities;
using IAX.IXApi.Modules.Finance.AccountsReceivable;
using IAX.IXApi.Modules.Finance.Persistence;
using IAX.IXApi.Modules.Identity.Permissions;
using IAX.IXApi.Shared.Application.Contracts;
using IAX.IXApi.Shared.Application.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace IAX.IXApi.Modules.Finance.Inventory;

[ApiController]
[Route("api/v1/InventTrans")]
[DomainPermission("Inventory", "Transactions", "View")]
public sealed class InventTransController(
    IFinanceDataContext dbContext,
    ICompanyExecutionContext company) : ControllerBase
{
    [HttpGet("list")]
    public async Task<ActionResult<APIResponse<IEnumerable<InventTransListDto>>>> GetList(
        CancellationToken cancellationToken = default)
    {
        var dataAreaId = company.GetDataAreaId();
        if (string.IsNullOrWhiteSpace(dataAreaId))
            return BadRequest(APIResponse<IEnumerable<InventTransListDto>>.Fail(
                "A company must be selected to view inventory transactions."));

        var transactions = await dbContext.Set<InventTrans>()
            .AsNoTracking()
            .Where(transaction => transaction.DataAreaId == dataAreaId)
            .OrderByDescending(transaction => transaction.DateStatus)
            .ThenByDescending(transaction => transaction.RecId)
            .ToListAsync(cancellationToken);

        var originIds = transactions.Select(transaction => transaction.InventTransOrigin).Distinct().ToList();
        var origins = await dbContext.Set<InventTransOrigin>()
            .AsNoTracking()
            .Where(origin => origin.DataAreaId == dataAreaId && originIds.Contains(origin.RecId))
            .ToDictionaryAsync(origin => origin.RecId, cancellationToken);

        var dimensionIds = transactions
            .Select(transaction => transaction.InventDimId)
            .Where(id => id != string.Empty)
            .Distinct()
            .ToList();
        var dimensions = await dbContext.Set<InventDim>()
            .AsNoTracking()
            .Where(dimension => dimension.DataAreaId == dataAreaId && dimensionIds.Contains(dimension.InventDimId))
            .ToDictionaryAsync(dimension => dimension.InventDimId, cancellationToken);

        var salesInventTransIds = origins.Values
            .Where(origin => origin.ReferenceCategory == Common.InventRefType.SalesTable)
            .Select(origin => origin.InventTransId)
            .Distinct()
            .ToList();
        var salesPrices = await dbContext.Set<SalesLine>()
            .AsNoTracking()
            .Where(line => line.DataAreaId == dataAreaId && salesInventTransIds.Contains(line.InventTransId))
            .GroupBy(line => line.InventTransId)
            .Select(group => new { InventTransId = group.Key, UnitPrice = group.Select(line => line.SalesPrice).FirstOrDefault() })
            .ToDictionaryAsync(row => row.InventTransId, row => row.UnitPrice, cancellationToken);

        var result = transactions.Select(transaction =>
        {
            origins.TryGetValue(transaction.InventTransOrigin, out var origin);
            dimensions.TryGetValue(transaction.InventDimId, out var dimension);
            var costAmount = transaction.CostAmountPosted
                + transaction.CostAmountPhysical
                + transaction.CostAmountAdjustment;
            return new InventTransListDto
            {
                RecId = transaction.RecId,
                ItemNumber = transaction.ItemId,
                PhysicalDate = AsPostedDate(transaction.DatePhysical),
                FinancialDate = AsPostedDate(transaction.DateFinancial),
                Reference = ReferenceLabel(origin?.ReferenceCategory),
                ReferenceNumber = origin?.ReferenceId ?? string.Empty,
                InventTransId = origin?.InventTransId ?? string.Empty,
                ReceiptStatus = StatusLabel(transaction.StatusReceipt),
                IssueStatus = StatusLabel(transaction.StatusIssue),
                Quantity = transaction.Qty,
                UnitPrice = origin != null && salesPrices.TryGetValue(origin.InventTransId, out var salesPrice)
                    ? salesPrice
                    : 0,
                UnitCost = transaction.Qty == 0 ? 0 : Math.Abs(costAmount / transaction.Qty),
                CostAmount = costAmount,
                CurrencyCode = transaction.CurrencyCode,
                Site = dimension?.InventSiteId ?? string.Empty,
                Warehouse = dimension?.InventLocationId ?? string.Empty,
                BatchNumber = dimension?.InventBatchId ?? string.Empty,
                SerialNumber = dimension?.InventSerialId ?? string.Empty,
                ExpectedDate = AsPostedDate(transaction.DateExpected),
                Voucher = transaction.Voucher,
                DataAreaId = transaction.DataAreaId
            };
        }).ToList();

        return Ok(APIResponse<IEnumerable<InventTransListDto>>.Ok(result));
    }

    private static DateTime? AsPostedDate(DateTime value) => value == default ? null : value;

    private static string StatusLabel(Common.StatusIssue status) => status switch
    {
        Common.StatusIssue.None => string.Empty,
        Common.StatusIssue.Ordered => "On order",
        _ => status.ToString()
    };

    private static string StatusLabel(Common.StatusReceipt status) => status switch
    {
        Common.StatusReceipt.None => string.Empty,
        Common.StatusReceipt.Ordered => "Ordered",
        _ => status.ToString()
    };

    private static string ReferenceLabel(Common.InventRefType? reference) => reference switch
    {
        Common.InventRefType.SalesTable => "Sales order",
        Common.InventRefType.PurchaseOrder => "Purchase order",
        Common.InventRefType.TransferOrder => "Transfer order",
        Common.InventRefType.ProductionOrder => "Production order",
        Common.InventRefType.InventoryJournal => "Inventory journal",
        Common.InventRefType.InventoryAdjustment => "Inventory adjustment",
        Common.InventRefType.PhysicalinventoryCount => "Physical inventory count",
        Common.InventRefType.ReturnOrder => "Return order",
        _ => string.Empty
    };
}

public sealed class InventTransListDto
{
    public long RecId { get; init; }
    public string ItemNumber { get; init; } = string.Empty;
    public DateTime? PhysicalDate { get; init; }
    public DateTime? FinancialDate { get; init; }
    public string Reference { get; init; } = string.Empty;
    public string ReferenceNumber { get; init; } = string.Empty;
    public string InventTransId { get; init; } = string.Empty;
    public string ReceiptStatus { get; init; } = string.Empty;
    public string IssueStatus { get; init; } = string.Empty;
    public decimal Quantity { get; init; }
    public decimal UnitPrice { get; init; }
    public decimal UnitCost { get; init; }
    public decimal CostAmount { get; init; }
    public string CurrencyCode { get; init; } = string.Empty;
    public string Site { get; init; } = string.Empty;
    public string Warehouse { get; init; } = string.Empty;
    public string BatchNumber { get; init; } = string.Empty;
    public string SerialNumber { get; init; } = string.Empty;
    public DateTime? ExpectedDate { get; init; }
    public string Voucher { get; init; } = string.Empty;
    public string DataAreaId { get; init; } = string.Empty;
}
