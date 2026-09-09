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
