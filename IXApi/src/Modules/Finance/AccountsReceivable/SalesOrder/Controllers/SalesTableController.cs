using IAX.IXApi.Infrastructure.Persistence.Repositories;
using IAX.IXApi.Modules.Finance.Entities;
using IAX.IXApi.Modules.Finance.Foundation.LogisticsAddresses;
using IAX.IXApi.Modules.Identity.Permissions;
using IAX.IXApi.Shared.Application.Contracts;
using IAX.IXApi.Modules.Administration.NumberSequences;
using IAX.IXApi.Infrastructure.Identity;
using IAX.IXApi.Modules.Finance.Common;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace IAX.IXApi.Modules.Finance.AccountsReceivable;

[ApiController]
[Route("api/v1/SalesTable")]
[DomainPermission("AccountsReceivable", "SalesOrders", "View")]
public sealed class SalesTableController : ControllerBase
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly ISysNumberSequenceService _numberSequences;
    private readonly ICurrentUserService _currentUser;

    public SalesTableController(
        IUnitOfWork unitOfWork,
        ISysNumberSequenceService numberSequences,
        ICurrentUserService currentUser)
    {
        _unitOfWork = unitOfWork;
        _numberSequences = numberSequences;
        _currentUser = currentUser;
    }

    [HttpGet("units")]
    public async Task<IActionResult> Units(string? search = null, int pageNumber = 1, int pageSize = 25, CancellationToken cancellationToken = default)
    {
        pageNumber = Math.Max(1, pageNumber);
        pageSize = Math.Clamp(pageSize, 1, 100);
        var query = _unitOfWork.Context.Set<UnitOfMeasure>().AsNoTracking();
        if (!string.IsNullOrWhiteSpace(search)) query = query.Where(unit => unit.Symbol.Contains(search));
        var totalRecords = await query.CountAsync(cancellationToken);
        var data = await query.OrderBy(unit => unit.Symbol).Skip((pageNumber - 1) * pageSize).Take(pageSize)
            .Select(unit => new { symbol = unit.Symbol }).ToListAsync(cancellationToken);
        return Ok(APIResponse<object>.Ok(new { data, pageNumber, totalRecords, totalPages = (int)Math.Ceiling((double)totalRecords / pageSize) }));
    }

    [HttpGet("items")]
    public async Task<IActionResult> Items(string? search = null, int pageNumber = 1, int pageSize = 25, CancellationToken cancellationToken = default)
    {
        pageNumber = Math.Max(1, pageNumber);
        pageSize = Math.Clamp(pageSize, 1, 100);
        var query = _unitOfWork.Context.Set<InventTable>().AsNoTracking();
        if (!string.IsNullOrWhiteSpace(search))
            query = query.Where(item => item.ItemId.Contains(search) || item.NameAlias.Contains(search));
        var totalRecords = await query.CountAsync(cancellationToken);
        var items = await query.OrderBy(item => item.ItemId).Skip((pageNumber - 1) * pageSize).Take(pageSize)
            .ToListAsync(cancellationToken);
        var itemIds = items.Select(item => item.ItemId).ToList();
        var modules = await _unitOfWork.Context.Set<InventTableModule>().AsNoTracking()
            .Where(module => itemIds.Contains(module.ItemId) && (int)module.ModuleType == 2).ToListAsync(cancellationToken);
        var data = items.Select(item => {
            var module = modules.FirstOrDefault(row => row.ItemId == item.ItemId);
            return new { itemNumber = item.ItemId, name = item.NameAlias, itemType = item.ItemType.ToString(),
                unit = module?.UnitId ?? string.Empty, unitPrice = module == null ? 0 : module.Price / (module.PriceUnit > 0 ? module.PriceUnit : 1) };
        }).ToList();
        return Ok(APIResponse<object>.Ok(new { data, pageNumber, totalRecords, totalPages = (int)Math.Ceiling((double)totalRecords / pageSize) }));
    }

    [HttpGet("{recId:long}/lines")]
    public async Task<IActionResult> Lines(long recId, CancellationToken cancellationToken = default)
    {
        var order = await _unitOfWork.Context.Set<SalesTable>().AsNoTracking().FirstOrDefaultAsync(row => row.RecId == recId, cancellationToken);
        if (order == null) return NotFound(APIResponse<object>.Fail("Sales order was not found."));
        var lines = await _unitOfWork.Context.Set<SalesLine>().AsNoTracking()
            .Where(line => line.SalesId == order.SalesId && line.DataAreaId == order.DataAreaId)
            .OrderBy(line => line.LineNum).ToListAsync(cancellationToken);
        return Ok(APIResponse<object>.Ok(lines.Select(LineRecord)));
    }

    public sealed class AddSalesLineInput
    {
        [System.ComponentModel.DataAnnotations.Required]
        public string ItemNumber { get; set; } = string.Empty;
        [System.ComponentModel.DataAnnotations.Range(typeof(decimal), "0.000001", "1000000000")]
        public decimal Quantity { get; set; }
        [System.ComponentModel.DataAnnotations.Range(typeof(decimal), "0", "1000000000")]
        public decimal UnitPrice { get; set; }
        [System.ComponentModel.DataAnnotations.StringLength(100)]
        public string? Description { get; set; }
        [System.ComponentModel.DataAnnotations.StringLength(10)]
        public string? Unit { get; set; }
        public DateTime? DeliveryDate { get; set; }
        [System.ComponentModel.DataAnnotations.EnumDataType(typeof(SalesType))]
        public SalesType? LineType { get; set; }
        [System.ComponentModel.DataAnnotations.EnumDataType(typeof(SalesDeliveryType))]
        public SalesDeliveryType? DeliveryType { get; set; }
        [System.ComponentModel.DataAnnotations.Range(typeof(long), "0", "9223372036854775807")]
        public long SalesCategory { get; set; }
    }

    [HttpPost("{recId:long}/lines")]
    [DomainPermission("AccountsReceivable", "SalesOrders", "Edit")]
    public async Task<IActionResult> AddLine(long recId, [FromBody] AddSalesLineInput input, CancellationToken cancellationToken = default)
    {
        return await _unitOfWork.Context.Database.CreateExecutionStrategy().ExecuteAsync(async () =>
        {
            await using var transaction = await _unitOfWork.Context.Database.BeginTransactionAsync(System.Data.IsolationLevel.Serializable, cancellationToken);
            var order = await _unitOfWork.Context.Set<SalesTable>().FirstOrDefaultAsync(row => row.RecId == recId, cancellationToken);
            if (order == null) return (IActionResult)NotFound(APIResponse<object>.Fail("Sales order was not found."));
            if (order.SalesStatus != SalesStatus.Backorder)
                return UnprocessableEntity(APIResponse<object>.Fail("Lines can only be added to open sales orders."));
            var item = await _unitOfWork.Context.Set<InventTable>().AsNoTracking()
                .FirstOrDefaultAsync(row => row.ItemId == input.ItemNumber && row.DataAreaId == order.DataAreaId, cancellationToken);
            if (item == null) return UnprocessableEntity(APIResponse<object>.Fail("Item was not found."));
            var module = await _unitOfWork.Context.Set<InventTableModule>().AsNoTracking()
                .FirstOrDefaultAsync(row => row.ItemId == item.ItemId && row.DataAreaId == order.DataAreaId && (int)row.ModuleType == 2, cancellationToken);
            if (module == null || string.IsNullOrWhiteSpace(module.UnitId))
                return UnprocessableEntity(APIResponse<object>.Fail("The item must have a sales unit configured."));
            var lastLine = await _unitOfWork.Context.Set<SalesLine>()
                .Where(row => row.SalesId == order.SalesId && row.DataAreaId == order.DataAreaId)
                .MaxAsync(row => (decimal?)row.LineNum, cancellationToken) ?? 0;
            var line = new SalesLine {
                SalesId = order.SalesId, LineNum = lastLine + 1, ItemId = item.ItemId, Name = item.NameAlias,
                CustAccount = order.CustAccount, CustGroupId = order.CustGroup, CurrencyCode = order.CurrencyCode,
                SalesQty = input.Quantity, QtyOrdered = input.Quantity, RemainSalesPhysical = input.Quantity,
                RemainSalesFinancial = input.Quantity, SalesUnit = string.IsNullOrWhiteSpace(input.Unit) ? module.UnitId : input.Unit.Trim(), PriceUnit = 1,
                SalesPrice = input.UnitPrice, LineAmount = input.Quantity * input.UnitPrice,
                SalesStatus = SalesStatus.Backorder, SalesType = input.LineType ?? order.SalesType ?? SalesType.Sales,
                DeliveryType = input.DeliveryType ?? SalesDeliveryType.None, SalesCategory = input.SalesCategory,
                ReceiptDateRequested = input.DeliveryDate?.Date ?? order.ReceiptDateRequested, ShippingDateRequested = order.ShippingDateRequested,
                DataAreaId = order.DataAreaId,
            };
            _unitOfWork.Context.Set<SalesLine>().Add(line);
            order.SmmSalesAmountTotal += line.LineAmount;
            await _unitOfWork.CompleteAsync(cancellationToken);
            await transaction.CommitAsync(cancellationToken);
            return Ok(APIResponse<object>.Ok(LineRecord(line)));
        });
    }


    [HttpPut("{recId:long}/lines/{lineId:long}")]
    [DomainPermission("AccountsReceivable", "SalesOrders", "Edit")]
    public Task<IActionResult> UpdateLine(long recId, long lineId, [FromBody] AddSalesLineInput input, CancellationToken cancellationToken = default)
        => ChangeLine(recId, lineId, input, cancellationToken);

    [HttpDelete("{recId:long}/lines/{lineId:long}")]
    [DomainPermission("AccountsReceivable", "SalesOrders", "Edit")]
    public Task<IActionResult> RemoveLine(long recId, long lineId, CancellationToken cancellationToken = default)
        => ChangeLine(recId, lineId, null, cancellationToken);

    private async Task<IActionResult> ChangeLine(long recId, long lineId, AddSalesLineInput? input, CancellationToken cancellationToken)
    {
        return await _unitOfWork.Context.Database.CreateExecutionStrategy().ExecuteAsync(async () => {
            await using var transaction = await _unitOfWork.Context.Database.BeginTransactionAsync(System.Data.IsolationLevel.Serializable, cancellationToken);
            var order = await _unitOfWork.Context.Set<SalesTable>().FirstOrDefaultAsync(row => row.RecId == recId, cancellationToken);
            if (order == null) return (IActionResult)NotFound(APIResponse<object>.Fail("Sales order was not found."));
            if (order.SalesStatus != SalesStatus.Backorder)
                return UnprocessableEntity(APIResponse<object>.Fail("Only open sales orders can be changed."));
            var line = await _unitOfWork.Context.Set<SalesLine>().FirstOrDefaultAsync(row => row.RecId == lineId && row.SalesId == order.SalesId && row.DataAreaId == order.DataAreaId, cancellationToken);
            if (line == null) return NotFound(APIResponse<object>.Fail("Sales line was not found."));
            if (line.SalesStatus != SalesStatus.Backorder || line.RemainSalesPhysical != line.SalesQty || line.RemainSalesFinancial != line.SalesQty)
                return UnprocessableEntity(APIResponse<object>.Fail("Processed sales lines cannot be changed."));
            if (input != null && input.ItemNumber != line.ItemId)
                return UnprocessableEntity(APIResponse<object>.Fail("The item number cannot be changed."));
            var previousAmount = line.LineAmount;
            if (input == null) _unitOfWork.Context.Set<SalesLine>().Remove(line);
            else {
                // Item identity and name remain unchanged after item selection.
                line.SalesType = input.LineType ?? line.SalesType;
                line.DeliveryType = input.DeliveryType ?? line.DeliveryType;
                line.SalesCategory = input.SalesCategory;
                line.SalesQty = input.Quantity;
                line.QtyOrdered = input.Quantity;
                line.RemainSalesPhysical = input.Quantity;
                line.RemainSalesFinancial = input.Quantity;
                line.SalesPrice = input.UnitPrice;
                line.PriceUnit = 1;
                line.LineAmount = input.Quantity * input.UnitPrice;
                if (!string.IsNullOrWhiteSpace(input.Unit)) line.SalesUnit = input.Unit.Trim();
                if (input.DeliveryDate.HasValue) line.ReceiptDateRequested = input.DeliveryDate.Value.Date;
            }
            order.SmmSalesAmountTotal += (input == null ? 0 : line.LineAmount) - previousAmount;
            await _unitOfWork.CompleteAsync(cancellationToken);
            await transaction.CommitAsync(cancellationToken);
            return Ok(APIResponse<object>.Ok(input == null ? new { deleted = true } : LineRecord(line)));
        });
    }

    private static object LineRecord(SalesLine line) => new {
        id = line.RecId.ToString(), lineNumber = line.LineNum, itemNumber = line.ItemId,
        lineType = (int)line.SalesType, deliveryType = (int)line.DeliveryType, salesCategory = line.SalesCategory,
        description = line.Name, quantity = line.SalesQty, unit = line.SalesUnit,
        unitPrice = line.SalesPrice, lineTotal = line.LineAmount, deliveryDate = line.ReceiptDateRequested,
    };

    [HttpGet("list")]
    public async Task<ActionResult<APIResponse<IEnumerable<SalesOrderListDto>>>> GetList(
        CancellationToken cancellationToken = default)
    {
        var orders = await _unitOfWork.Context.Set<SalesTable>()
            .AsNoTracking()
            .OrderByDescending(order => order.RecId)
            .ToListAsync(cancellationToken);

        var customerAccounts = orders.Select(order => order.CustAccount).Where(account => account != string.Empty).Distinct().ToList();
        var customers = await _unitOfWork.Context.Set<CustTable>()
            .AsNoTracking()
            .Where(customer => customerAccounts.Contains(customer.AccountNum))
            .Select(customer => new { customer.AccountNum, customer.Party })
            .ToListAsync(cancellationToken);
        var partyIds = customers.Select(customer => customer.Party).Where(id => id > 0).Distinct().ToList();
        var parties = await _unitOfWork.Context.Set<DirPartyTable>()
            .AsNoTracking()
            .Where(party => partyIds.Contains(party.RecId))
            .ToDictionaryAsync(party => party.RecId, party => party.Name, cancellationToken);
        var customerNames = customers.ToDictionary(
            customer => customer.AccountNum,
            customer => parties.GetValueOrDefault(customer.Party) ?? customer.AccountNum);

        var result = orders.Select(order => new SalesOrderListDto
        {
            RecId = order.RecId,
            SalesId = order.SalesId,
            CustomerAccount = order.CustAccount,
            CustomerName = customerNames.GetValueOrDefault(order.CustAccount) ?? order.SalesName,
            InvoiceAccount = order.InvoiceAccount,
            CustomerGroup = order.CustGroup,
            CurrencyCode = order.CurrencyCode,
            SalesStatus = order.SalesStatus.ToString(),
            DocumentStatus = order.DocumentStatus.ToString(),
            DeliveryDate = order.DeliveryDate,
            ShippingDateRequested = order.ShippingDateRequested,
            OrderTotal = order.SmmSalesAmountTotal,
            CustomerReference = order.CustomerRef,
            DeliveryMode = order.DlvMode,
            PaymentTerms = order.PaymTerm
        }).ToList();

        return Ok(APIResponse<IEnumerable<SalesOrderListDto>>.Ok(result));
    }

    public sealed class UpdateSalesHeaderInput
    {
        [System.ComponentModel.DataAnnotations.Required]
        [System.ComponentModel.DataAnnotations.StringLength(FieldLengths.InvoiceAccount)]
        public string InvoiceAccount { get; set; } = string.Empty;
        [System.ComponentModel.DataAnnotations.Required]
        [System.ComponentModel.DataAnnotations.StringLength(FieldLengths.CurrencyCode)]
        public string CurrencyCode { get; set; } = string.Empty;
        [System.ComponentModel.DataAnnotations.StringLength(FieldLengths.ReferenceId)]
        public string CustomerReference { get; set; } = string.Empty;
        [System.ComponentModel.DataAnnotations.StringLength(FieldLengths.PaymTermId)]
        public string PaymentTerms { get; set; } = string.Empty;
        [System.ComponentModel.DataAnnotations.StringLength(FieldLengths.DlvModeId)]
        public string DeliveryMode { get; set; } = string.Empty;
        [System.ComponentModel.DataAnnotations.Required]
        public DateTime? DeliveryDate { get; set; }
    }

    [HttpPut("{recId:long}/header")]
    [DomainPermission("AccountsReceivable", "SalesOrders", "Edit")]
    public async Task<IActionResult> UpdateHeader(long recId, [FromBody] UpdateSalesHeaderInput input, CancellationToken cancellationToken = default)
    {
        return await _unitOfWork.Context.Database.CreateExecutionStrategy().ExecuteAsync(async () =>
        {
            await using var transaction = await _unitOfWork.Context.Database.BeginTransactionAsync(System.Data.IsolationLevel.Serializable, cancellationToken);
            var order = await _unitOfWork.Context.Set<SalesTable>().FirstOrDefaultAsync(row => row.RecId == recId, cancellationToken);
            if (order == null) return (IActionResult)NotFound(APIResponse<object>.Fail("Sales order was not found."));
            if (order.SalesStatus != SalesStatus.Backorder)
                return UnprocessableEntity(APIResponse<object>.Fail("Only open sales orders can be changed."));
            var invoiceAccount = input.InvoiceAccount.Trim();
            if (!await _unitOfWork.Context.Set<CustTable>().AnyAsync(row => row.AccountNum == invoiceAccount && row.DataAreaId == order.DataAreaId, cancellationToken))
                return UnprocessableEntity(APIResponse<object>.Fail("Invoice account was not found."));
            var currency = input.CurrencyCode.Trim();
            if (currency != order.CurrencyCode && await _unitOfWork.Context.Set<SalesLine>().AnyAsync(row => row.SalesId == order.SalesId && row.DataAreaId == order.DataAreaId, cancellationToken))
                return UnprocessableEntity(APIResponse<object>.Fail("Currency cannot be changed after sales lines have been added."));
            order.InvoiceAccount = invoiceAccount;
            order.CurrencyCode = currency;
            order.CustomerRef = input.CustomerReference?.Trim() ?? string.Empty;
            order.PaymTerm = input.PaymentTerms?.Trim() ?? string.Empty;
            order.DlvMode = input.DeliveryMode?.Trim() ?? string.Empty;
            order.DeliveryDate = input.DeliveryDate!.Value.Date;
            order.ReceiptDateRequested = order.DeliveryDate;
            await _unitOfWork.CompleteAsync(cancellationToken);
            await transaction.CommitAsync(cancellationToken);
            return Ok(APIResponse<object>.Ok(new { saved = true }));
        });
    }

    [HttpPost("quick-create")]
    [DomainPermission("AccountsReceivable", "SalesOrders", "Create")]
    public async Task<ActionResult<APIResponse<SalesOrderListDto>>> QuickCreate(
        [FromBody] SalesOrderQuickCreateDto input,
        CancellationToken cancellationToken = default)
    {
        var account = input.CustomerAccount.Trim();
        var customer = await _unitOfWork.Context.Set<CustTable>()
            .AsNoTracking()
            .FirstOrDefaultAsync(candidate => candidate.AccountNum == account, cancellationToken);
        if (customer == null)
            return UnprocessableEntity(APIResponse<SalesOrderListDto>.Fail("Customer account was not found."));

        var party = customer.Party > 0
            ? await _unitOfWork.Context.Set<DirPartyTable>()
                .AsNoTracking()
                .FirstOrDefaultAsync(candidate => candidate.RecId == customer.Party, cancellationToken)
            : null;
        var sequence = await _numberSequences.NextAsync("SalesTable", cancellationToken: cancellationToken);
        var today = DateTime.UtcNow.Date;
        var order = new SalesTable
        {
            SalesId = sequence.Code,
            SalesName = string.IsNullOrWhiteSpace(input.SalesName) ? party?.Name ?? account : input.SalesName.Trim(),
            SalesNameAlias = party?.NameAlias ?? string.Empty,
            SalesStatus = SalesStatus.Backorder,
            DocumentStatus = DocumentStatus.None,
            SalesType = SalesType.Sales,
            CustAccount = account,
            InvoiceAccount = string.IsNullOrWhiteSpace(input.InvoiceAccount)
                ? string.IsNullOrWhiteSpace(customer.InvoiceAccount) ? account : customer.InvoiceAccount
                : input.InvoiceAccount.Trim(),
            CustGroup = customer.CustGroupId,
            CurrencyCode = string.IsNullOrWhiteSpace(input.CurrencyCode) ? customer.CurrencyCode : input.CurrencyCode.Trim(),
            TaxGroupId = customer.TaxGroupId,
            PaymTerm = input.PaymentTerms?.Trim() ?? customer.PaymTermId,
            PaymMode = input.PaymentMethod?.Trim() ?? customer.PaymModeId,
            DlvMode = string.IsNullOrWhiteSpace(input.DeliveryMode) ? customer.DlvModeId : input.DeliveryMode.Trim(),
            DlvTerm = input.DeliveryTerms?.Trim() ?? string.Empty,
            InventSiteId = string.IsNullOrWhiteSpace(input.InventSiteId) ? customer.InventSiteId : input.InventSiteId.Trim(),
            InventLocationId = string.IsNullOrWhiteSpace(input.InventLocationId) ? customer.InventLocationId : input.InventLocationId.Trim(),
            SalesGroup = input.SalesGroup?.Trim() ?? string.Empty,
            CustRequisitionNum = input.CustomerRequisitionNumber?.Trim() ?? string.Empty,
            IntercompanyOrder = input.Intercompany,
            IntercompanyCompanyId = input.IntercompanyCompanyId?.Trim() ?? string.Empty,
            OneTimeCustomer = input.OneTimeCustomer ? NoYes.Yes : NoYes.No,
            DeliveryName = input.DeliveryName?.Trim() ?? party?.Name ?? account,
            DeliveryPostalAddress = input.DeliveryPostalAddress ?? 0,
            CustomerRef = input.CustomerReference?.Trim() ?? string.Empty,
            Email = input.Contact?.Trim() ?? string.Empty,
            DeliveryDate = input.RequestedReceiptDate?.Date ?? today,
            ReceiptDateRequested = input.RequestedReceiptDate?.Date ?? today,
            ShippingDateRequested = input.RequestedShipDate?.Date ?? today,
            ReceiptDateConfirmed = input.ConfirmDates ? input.RequestedReceiptDate?.Date ?? today : default,
            ShippingDateConfirmed = input.ConfirmDates ? input.RequestedShipDate?.Date ?? today : default,
            DeliveryDateControlType = Enum.IsDefined(typeof(SalesDlvDateControlType), input.DeliveryDateControlType)
                ? (SalesDlvDateControlType)input.DeliveryDateControlType
                : default,
            DataAreaId = _currentUser.GetDataAreaId() ?? "dat"
        };

        _unitOfWork.Context.Set<SalesTable>().Add(order);
        await _unitOfWork.CompleteAsync(cancellationToken);

        return Ok(APIResponse<SalesOrderListDto>.Ok(new SalesOrderListDto
        {
            RecId = order.RecId,
            SalesId = order.SalesId,
            CustomerAccount = order.CustAccount,
            CustomerName = party?.Name ?? order.SalesName,
            InvoiceAccount = order.InvoiceAccount,
            CustomerGroup = order.CustGroup,
            CurrencyCode = order.CurrencyCode,
            SalesStatus = order.SalesStatus.ToString(),
            DocumentStatus = order.DocumentStatus.ToString(),
            DeliveryDate = order.DeliveryDate,
            ShippingDateRequested = order.ShippingDateRequested,
            OrderTotal = order.SmmSalesAmountTotal,
            CustomerReference = order.CustomerRef,
            DeliveryMode = order.DlvMode,
            PaymentTerms = order.PaymTerm
        }, "Created successfully"));
    }
}
