using IAX.IXApi.Api.Controllers;
using IAX.IXApi.Modules.Finance.AccountsReceivable.SalesOrder.Interfaces;
using IAX.IXApi.Modules.Finance.Foundation.LogisticsAddresses;
using IAX.IXApi.Modules.Identity.Permissions;
using IAX.IXApi.Shared.Application.Contracts;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace IAX.IXApi.Modules.Finance.AccountsReceivable;

[ApiController]
[Route("api/v1/SalesTable")]
[DomainPermission("AccountsReceivable", "SalesOrders", "View")]
public sealed class SalesTableController : BaseController<SalesTable, SalesTableDto>
{
    private readonly ISalesTableService _salesTableService;
    private readonly ISalesOrderLookupService _lookups;
    private readonly ISalesOrderReadService _reads;
    private readonly ISalesOrderQuickCreateService _quickCreate;
    private readonly ISalesOrderCancellationService _cancellation;
    private readonly ISalesOrderDeliveryAddressService _deliveryAddresses;
    private readonly ISalesOrderHeaderService _header;
    private readonly ISalesOrderDiscountService _discounts;
    private readonly ISalesOrderLineService _lineWrites;

    public SalesTableController(
        ISalesTableService service,
        ILogger<SalesTableController> logger,
        ISalesOrderLookupService lookups,
        ISalesOrderReadService reads,
        ISalesOrderQuickCreateService quickCreate,
        ISalesOrderCancellationService cancellation,
        ISalesOrderDeliveryAddressService deliveryAddresses,
        ISalesOrderHeaderService header,
        ISalesOrderDiscountService discounts,
        ISalesOrderLineService lineWrites)
        : base(service, logger)
    {
        _salesTableService = service;
        _lookups = lookups;
        _reads = reads;
        _quickCreate = quickCreate;
        _cancellation = cancellation;
        _deliveryAddresses = deliveryAddresses;
        _header = header;
        _discounts = discounts;
        _lineWrites = lineWrites;
    }

    [HttpGet("units")]
    public async Task<IActionResult> Units(string? search = null, int pageNumber = 1, int pageSize = 25, CancellationToken cancellationToken = default)
        => Ok(APIResponse<object>.Ok(await _lookups.UnitsAsync(search, pageNumber, pageSize, cancellationToken)));

    [HttpGet("items")]
    public async Task<IActionResult> Items(string? search = null, int pageNumber = 1, int pageSize = 25, CancellationToken cancellationToken = default)
        => Ok(APIResponse<object>.Ok(await _lookups.ItemsAsync(search, pageNumber, pageSize, cancellationToken)));

    [HttpGet("inventory-dimensions")]
    public async Task<IActionResult> InventoryDimensions(CancellationToken cancellationToken = default)
        => Ok(APIResponse<object>.Ok(await _lookups.InventoryDimensionsAsync(cancellationToken)));

    [HttpGet("tax-groups")]
    public async Task<IActionResult> TaxGroups(CancellationToken cancellationToken = default)
        => Ok(APIResponse<object>.Ok(await _lookups.TaxGroupsAsync(cancellationToken)));

    [HttpGet("ledger-dimensions")]
    public async Task<IActionResult> LedgerDimensions(string? search = null, int pageNumber = 1,
        int pageSize = 50, CancellationToken cancellationToken = default)
        => Ok(APIResponse<object>.Ok(await _lookups.LedgerDimensionsAsync(search, pageNumber, pageSize, cancellationToken)));

    [HttpGet("return-lots")]
    public async Task<IActionResult> ReturnLots(string itemNumber, string? search = null,
        int pageNumber = 1, int pageSize = 50,
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(itemNumber))
            return BadRequest(APIResponse<object>.Fail("An item number is required to find return lots."));
        return Ok(APIResponse<object>.Ok(await _lookups.ReturnLotsAsync(itemNumber, search, pageNumber, pageSize, cancellationToken)));
    }

    [HttpGet("batch-numbers")]
    public async Task<IActionResult> BatchNumbers(string itemNumber, string? search = null,
        int pageNumber = 1, int pageSize = 50, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(itemNumber))
            return BadRequest(APIResponse<object>.Fail("An item number is required to find batches."));
        return Ok(APIResponse<object>.Ok(await _lookups.BatchNumbersAsync(itemNumber, search, pageNumber, pageSize, cancellationToken)));
    }

    [HttpGet("serial-numbers")]
    public async Task<IActionResult> SerialNumbers(string itemNumber, string? search = null,
        int pageNumber = 1, int pageSize = 50, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(itemNumber))
            return BadRequest(APIResponse<object>.Fail("An item number is required to find serial numbers."));
        return Ok(APIResponse<object>.Ok(await _lookups.SerialNumbersAsync(itemNumber, search, pageNumber, pageSize, cancellationToken)));
    }

    [HttpGet("{recId:long}/lines")]
    public async Task<IActionResult> Lines(long recId, CancellationToken cancellationToken = default)
    {
        var response = await _reads.LinesAsync(recId, cancellationToken);
        return response == null
            ? NotFound(APIResponse<object>.Fail("Sales order was not found."))
            : Ok(APIResponse<object>.Ok(response));
    }

    [HttpGet("{recId:long}/delivery-addresses")]
    public async Task<IActionResult> DeliveryAddresses(long recId, CancellationToken cancellationToken = default)
    {
        var response = await _reads.DeliveryAddressesAsync(recId, cancellationToken);
        return response == null
            ? NotFound(APIResponse<object>.Fail("Sales order was not found."))
            : Ok(APIResponse<object>.Ok(response));
    }

    [HttpPost("{recId:long}/delivery-addresses")]
    [DomainPermission("AccountsReceivable", "SalesOrders", "Edit")]
    public async Task<IActionResult> CreateDeliveryAddress(long recId, [FromBody] CreateSalesDeliveryAddressInput input,
        CancellationToken cancellationToken = default)
    {
        var result = await _deliveryAddresses.CreateAsync(recId, input.LineId, input.Address, cancellationToken);
        return result.Status switch
        {
            SalesDeliveryAddressStatus.OrderNotFound => NotFound(APIResponse<object>.Fail("Sales order was not found.")),
            SalesDeliveryAddressStatus.NotOpen => UnprocessableEntity(APIResponse<object>.Fail("Only open sales orders can be changed.")),
            SalesDeliveryAddressStatus.BadLineId => BadRequest(APIResponse<object>.Fail("Sales line was not found.")),
            SalesDeliveryAddressStatus.LineNotFound => NotFound(APIResponse<object>.Fail("Sales line was not found.")),
            SalesDeliveryAddressStatus.ProcessedLine => UnprocessableEntity(APIResponse<object>.Fail("Processed sales lines cannot be changed.")),
            _ => Ok(APIResponse<object>.Ok(new
            {
                id = result.Id.ToString(System.Globalization.CultureInfo.InvariantCulture),
                description = result.Description,
                address = result.Address
            }))
        };
    }

    [HttpGet("{recId:long}/totals")]
    public async Task<IActionResult> Totals(long recId, CancellationToken cancellationToken = default)
    {
        var response = await _reads.TotalsAsync(recId, cancellationToken);
        return response == null
            ? NotFound(APIResponse<object>.Fail("Sales order was not found."))
            : Ok(APIResponse<object>.Ok(response));
    }

    [HttpPost("{recId:long}/recalculate-discounts")]
    [DomainPermission("AccountsReceivable", "SalesOrders", "Edit")]
    public async Task<IActionResult> RecalculateDiscounts(long recId, CancellationToken cancellationToken = default)
    {
        var result = await _discounts.RecalculateAsync(recId, cancellationToken);
        return result.Status switch
        {
            SalesDiscountRecalculationStatus.NotFound => NotFound(APIResponse<object>.Fail("Sales order was not found in the selected company.")),
            SalesDiscountRecalculationStatus.NotOpen => UnprocessableEntity(APIResponse<object>.Fail("Only open sales orders can recalculate discounts.")),
            _ => Ok(APIResponse<object>.Ok(new
            {
                updatedLines = result.UpdatedLines,
                lineDiscount = result.LineDiscount,
                multiLineDiscount = result.MultiLineDiscount,
                totalDiscountPercent = result.TotalDiscountPercent
            }))
        };
    }

    public sealed class AddSalesLineInput : SalesLineInputDto { }

    [HttpPost("{recId:long}/lines")]
    [DomainPermission("AccountsReceivable", "SalesOrders", "Edit")]
    public async Task<IActionResult> AddLine(long recId, [FromBody] AddSalesLineInput input, CancellationToken cancellationToken = default)
        => PresentLineWrite(await _lineWrites.AddAsync(recId, input, cancellationToken));

    [HttpPut("{recId:long}/lines/{lineId:long}")]
    [DomainPermission("AccountsReceivable", "SalesOrders", "Edit")]
    public Task<IActionResult> UpdateLine(long recId, long lineId, [FromBody] AddSalesLineInput input, CancellationToken cancellationToken = default)
        => ChangeLine(recId, lineId, input, cancellationToken);

    [HttpDelete("{recId:long}/lines/{lineId:long}")]
    [DomainPermission("AccountsReceivable", "SalesOrders", "Edit")]
    public Task<IActionResult> RemoveLine(long recId, long lineId, CancellationToken cancellationToken = default)
        => ChangeLine(recId, lineId, null, cancellationToken);

    [HttpPost("{recId:long}/cancel")]
    [DomainPermission("AccountsReceivable", "SalesOrders", "Edit")]
    public async Task<IActionResult> Cancel(long recId, CancellationToken cancellationToken = default)
    {
        var status = await _cancellation.CancelAsync(recId, cancellationToken);
        return status switch
        {
            SalesOrderCancelStatus.NotFound => NotFound(APIResponse<object>.Fail("Sales order was not found.")),
            SalesOrderCancelStatus.NotOpen => UnprocessableEntity(APIResponse<object>.Fail("Only an open sales order can be cancelled.")),
            SalesOrderCancelStatus.Processed => UnprocessableEntity(APIResponse<object>.Fail(
                "This cancellation currently supports only completely open, unprocessed sales orders.")),
            _ => Ok(APIResponse<object>.Ok(new { cancelled = true }))
        };
    }

    private async Task<IActionResult> ChangeLine(long recId, long lineId, AddSalesLineInput? input, CancellationToken cancellationToken)
        => PresentLineWrite(await _lineWrites.ChangeAsync(recId, lineId, input, cancellationToken));

    private IActionResult PresentLineWrite(SalesLineWriteResult result) => result.Status switch
    {
        SalesLineWriteStatus.NotFound => NotFound(APIResponse<object>.Fail(result.Error!)),
        SalesLineWriteStatus.Invalid => UnprocessableEntity(APIResponse<object>.Fail(result.Error!)),
        _ => Ok(APIResponse<object>.Ok(result.Data!))
    };

    public sealed class CreateSalesDeliveryAddressInput
    {
        public string? LineId { get; set; }
        public AddressInfoDto Address { get; set; } = new();
    }

    [HttpGet("list")]
    public async Task<ActionResult<APIResponse<IEnumerable<SalesOrderListDto>>>> GetList(
        CancellationToken cancellationToken = default)

        => Ok(APIResponse<IEnumerable<SalesOrderListDto>>.Ok(await _reads.GetListAsync(cancellationToken)));

    public sealed class UpdateSalesHeaderInput : UpdateSalesHeaderInputDto { }

    [HttpPut("{recId:long}/header")]
    [DomainPermission("AccountsReceivable", "SalesOrders", "Edit")]
    public async Task<IActionResult> UpdateHeader(long recId, [FromBody] UpdateSalesHeaderInput input, CancellationToken cancellationToken = default)
    {
        var result = await _header.UpdateAsync(recId, input, cancellationToken);
        return result.Status switch
        {
            SalesHeaderUpdateStatus.NotFound => NotFound(APIResponse<object>.Fail(result.Error!)),
            SalesHeaderUpdateStatus.Invalid => UnprocessableEntity(APIResponse<object>.Fail(result.Error!)),
            _ => Ok(APIResponse<object>.Ok(new { saved = true }))
        };
    }

    [HttpPost("quick-create")]
    [DomainPermission("AccountsReceivable", "SalesOrders", "Create")]
    public async Task<ActionResult<APIResponse<SalesOrderListDto>>> QuickCreate(
        [FromBody] SalesOrderQuickCreateDto input, CancellationToken cancellationToken = default)
    {
        var result = await _quickCreate.CreateAsync(input, cancellationToken);
        return result.Error != null
            ? UnprocessableEntity(APIResponse<SalesOrderListDto>.Fail(result.Error))
            : Ok(APIResponse<SalesOrderListDto>.Ok(result.Order!, "Created successfully"));
    }

}
