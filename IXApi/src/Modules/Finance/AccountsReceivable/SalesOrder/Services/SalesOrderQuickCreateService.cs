using IAX.IXApi.Modules.Administration.NumberSequences;
using IAX.IXApi.Modules.Finance.Common;
using IAX.IXApi.Modules.Finance.Entities;
using IAX.IXApi.Modules.Finance.Persistence;
using IAX.IXApi.Shared.Application.Identity;
using Microsoft.EntityFrameworkCore;

namespace IAX.IXApi.Modules.Finance.AccountsReceivable;

public sealed record SalesOrderQuickCreateResult(SalesOrderListDto? Order, string? Error = null);

public sealed class SalesOrderQuickCreateService
{
    private readonly IFinanceDataContext _dbContext;
    private readonly ICompanyExecutionContext _company;
    private readonly ISysNumberSequenceService _numberSequences;
    private readonly SalesOrderValidationService _validation;

    public SalesOrderQuickCreateService(IFinanceDataContext dbContext, ICompanyExecutionContext company,
        ISysNumberSequenceService numberSequences, SalesOrderValidationService validation)
    {
        _dbContext = dbContext;
        _company = company;
        _numberSequences = numberSequences;
        _validation = validation;
    }

    public async Task<SalesOrderQuickCreateResult> CreateAsync(SalesOrderQuickCreateDto input,
        CancellationToken cancellationToken)
    {
        var account = input.CustomerAccount.Trim();
        var dataAreaId = _company.GetDataAreaId();
        var customer = await _dbContext.Set<CustTable>()
            .AsNoTracking()
            .FirstOrDefaultAsync(candidate => candidate.AccountNum == account
                && candidate.DataAreaId == dataAreaId, cancellationToken);
        if (customer == null)
            return new SalesOrderQuickCreateResult(null, "Customer account was not found.");

        var party = customer.Party > 0
            ? await _dbContext.Set<DirPartyTable>()
                .AsNoTracking()
                .FirstOrDefaultAsync(candidate => candidate.RecId == customer.Party, cancellationToken)
            : null;
        var deliveryPostalAddress = input.DeliveryPostalAddress ?? 0;
        if (!string.IsNullOrWhiteSpace(input.DeliveryPostalAddressId)
            && (!long.TryParse(input.DeliveryPostalAddressId, out deliveryPostalAddress) || deliveryPostalAddress <= 0))
            return new SalesOrderQuickCreateResult(null, "The selected delivery address is invalid.");
        var taxGroupId = string.IsNullOrWhiteSpace(input.TaxGroupId) ? customer.TaxGroupId : input.TaxGroupId.Trim();
        if (!string.IsNullOrWhiteSpace(taxGroupId)
            && !await _dbContext.Set<TaxGroupHeading>().AsNoTracking()
                .AnyAsync(group => group.TaxGroup == taxGroupId, cancellationToken))
            return new SalesOrderQuickCreateResult(null, "The selected sales tax group was not found.");
        var invoiceAccount = string.IsNullOrWhiteSpace(input.InvoiceAccount)
            ? (string.IsNullOrWhiteSpace(customer.InvoiceAccount) ? account : customer.InvoiceAccount.Trim())
            : input.InvoiceAccount.Trim();
        if (!await _dbContext.Set<CustTable>().AsNoTracking().AnyAsync(candidate =>
                candidate.AccountNum == invoiceAccount && candidate.DataAreaId == dataAreaId, cancellationToken))
            return new SalesOrderQuickCreateResult(null, "The invoice account was not found in the selected company.");
        var currencyCode = string.IsNullOrWhiteSpace(input.CurrencyCode)
            ? customer.CurrencyCode : input.CurrencyCode.Trim();
        if (string.IsNullOrWhiteSpace(currencyCode)
            || !await _dbContext.Set<Currency>().AsNoTracking().AnyAsync(currency =>
                currency.CurrencyCode == currencyCode && currency.DataAreaId == dataAreaId, cancellationToken))
            return new SalesOrderQuickCreateResult(null, "The selected currency was not found in the selected company.");
        var paymentTerms = string.IsNullOrWhiteSpace(input.PaymentTerms) ? customer.PaymTermId : input.PaymentTerms.Trim();
        if (!string.IsNullOrWhiteSpace(paymentTerms)
            && !await _dbContext.Set<PaymTerm>().AnyAsync(term => term.PaymTermId == paymentTerms, cancellationToken))
            return new SalesOrderQuickCreateResult(null, "The selected payment terms were not found.");
        var paymentMethod = string.IsNullOrWhiteSpace(input.PaymentMethod) ? customer.PaymModeId : input.PaymentMethod.Trim();
        if (!string.IsNullOrWhiteSpace(paymentMethod)
            && !await _dbContext.Set<CustPaymModeTable>().AnyAsync(mode => mode.PaymMode == paymentMethod, cancellationToken))
            return new SalesOrderQuickCreateResult(null, "The selected payment method was not found.");
        var deliveryMode = string.IsNullOrWhiteSpace(input.DeliveryMode) ? customer.DlvModeId : input.DeliveryMode.Trim();
        if (!string.IsNullOrWhiteSpace(deliveryMode)
            && !await _dbContext.Set<DlvMode>().AnyAsync(mode => mode.Code == deliveryMode, cancellationToken))
            return new SalesOrderQuickCreateResult(null, "The selected delivery mode was not found.");
        var deliveryTerms = input.DeliveryTerms?.Trim() ?? string.Empty;
        if (!string.IsNullOrWhiteSpace(deliveryTerms)
            && !await _dbContext.Set<DlvTerm>().AnyAsync(term => term.Code == deliveryTerms, cancellationToken))
            return new SalesOrderQuickCreateResult(null, "The selected delivery terms were not found.");
        var inventSiteId = string.IsNullOrWhiteSpace(input.InventSiteId)
            ? customer.InventSiteId : input.InventSiteId.Trim();
        var inventLocationId = string.IsNullOrWhiteSpace(input.InventLocationId)
            ? customer.InventLocationId : input.InventLocationId.Trim();
        var dimensionError = await _validation.ValidateSiteWarehouseAsync(inventSiteId, inventLocationId, cancellationToken);
        if (dimensionError != null) return new(null, dimensionError);
        if (customer.Party > 0)
        {
            var customerAddresses = await (
                from link in _dbContext.Set<DirPartyLocation>().AsNoTracking()
                join address in _dbContext.Set<LogisticsPostalAddress>().AsNoTracking()
                    on link.Location equals address.Location
                where link.Party == customer.Party && link.IsPostalAddress == NoYes.Yes
                    && address.DataAreaId == customer.DataAreaId
                    && address.ValidFrom <= DateTime.UtcNow && address.ValidTo >= DateTime.UtcNow
                orderby link.IsPrimary descending, address.RecId
                select address.RecId
            ).ToListAsync(cancellationToken);
            if (deliveryPostalAddress != 0 && !customerAddresses.Contains(deliveryPostalAddress))
                return new SalesOrderQuickCreateResult(null, "The selected delivery address does not belong to this customer.");
            if (deliveryPostalAddress == 0)
                deliveryPostalAddress = customerAddresses.FirstOrDefault();
        }
        var customerContacts = customer.Party > 0
            ? await (
                from link in _dbContext.Set<DirPartyLocation>().AsNoTracking()
                join contact in _dbContext.Set<LogisticsElectronicAddress>().AsNoTracking()
                    on link.Location equals contact.Location
                where link.Party == customer.Party && link.IsPostalAddress == NoYes.No
                    && (contact.Type == ElectronicAddressType.Email || contact.Type == ElectronicAddressType.Phone)
                orderby link.IsPrimary descending, contact.IsPrimary descending, contact.RecId
                select new { contact.Type, contact.Locator }
            ).ToListAsync(cancellationToken)
            : [];
        var email = customerContacts.FirstOrDefault(contact => contact.Type == ElectronicAddressType.Email)?.Locator ?? string.Empty;
        var phone = customerContacts.FirstOrDefault(contact => contact.Type == ElectronicAddressType.Phone)?.Locator ?? string.Empty;
        var chosenContact = input.Contact?.Trim();
        if (!string.IsNullOrEmpty(chosenContact))
        {
            if (string.Equals(input.ContactType, "Phone", StringComparison.OrdinalIgnoreCase)) phone = chosenContact;
            else email = chosenContact;
        }
        var salesId = await NextAvailableSalesIdAsync(cancellationToken);
        var today = DateTime.UtcNow.Date;
        var order = new SalesTable
        {
            SalesId = salesId,
            SalesName = string.IsNullOrWhiteSpace(input.SalesName) ? party?.Name ?? account : input.SalesName.Trim(),
            SalesNameAlias = party?.NameAlias ?? string.Empty,
            SalesStatus = SalesStatus.Backorder,
            DocumentStatus = DocumentStatus.None,
            SalesType = SalesType.Sales,
            CustAccount = account,
            InvoiceAccount = invoiceAccount,
            CustGroup = customer.CustGroupId,
            CurrencyCode = currencyCode,
            TaxGroupId = taxGroupId,
            PaymTerm = paymentTerms,
            PaymMode = paymentMethod,
            DlvMode = deliveryMode,
            DlvTerm = deliveryTerms,
            InventSiteId = inventSiteId,
            InventLocationId = inventLocationId,
            SalesGroup = input.SalesGroup?.Trim() ?? string.Empty,
            SalesPoolId = customer.SalesPoolId,
            PriceGroupId = input.PriceGroup?.Trim() ?? string.Empty,
            CustRequisitionNum = input.CustomerRequisitionNumber?.Trim() ?? string.Empty,
            IntercompanyOrder = input.Intercompany,
            IntercompanyCompanyId = input.IntercompanyCompanyId?.Trim() ?? string.Empty,
            OneTimeCustomer = input.OneTimeCustomer ? NoYes.Yes : NoYes.No,
            DeliveryName = input.DeliveryName?.Trim() ?? party?.Name ?? account,
            DeliveryPostalAddress = deliveryPostalAddress,
            CustomerRef = input.CustomerReference?.Trim() ?? string.Empty,
            Email = email,
            Phone = phone,
            OrderDate = today,
            DeliveryDate = input.RequestedReceiptDate?.Date ?? today,
            ReceiptDateRequested = input.RequestedReceiptDate?.Date ?? today,
            ShippingDateRequested = input.RequestedShipDate?.Date ?? today,
            ReceiptDateConfirmed = input.ConfirmDates ? input.RequestedReceiptDate?.Date ?? today : default,
            ShippingDateConfirmed = input.ConfirmDates ? input.RequestedShipDate?.Date ?? today : default,
            DeliveryDateControlType = Enum.IsDefined(typeof(SalesDlvDateControlType), input.DeliveryDateControlType)
                ? (SalesDlvDateControlType)input.DeliveryDateControlType
                : default,
            DataAreaId = dataAreaId ?? "dat"
        };

        _dbContext.Set<SalesTable>().Add(order);
        await _dbContext.SaveChangesAsync(cancellationToken);

        return new SalesOrderQuickCreateResult(new SalesOrderListDto
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
            DeliveryTerms = order.DlvTerm,
            OrderDate = order.OrderDate,
            InventSiteId = order.InventSiteId,
            InventLocationId = order.InventLocationId,
            SalesNameAlias = order.SalesNameAlias,
            SalesType = (int)(order.SalesType ?? SalesType.Sales),
            OneTimeCustomer = order.OneTimeCustomer == NoYes.Yes,
            Email = order.Email,
            Phone = order.Phone,
            Deadline = order.Deadline == default ? null : order.Deadline,
            CustomerRequisitionNumber = order.CustRequisitionNum,
            Notes = order.Notes,
            CampaignId = order.SmmCampaignId,
            TaxGroupId = order.TaxGroupId,
            PricesIncludeSalesTax = order.InclTax,
            SalesGroup = order.SalesGroup,
            LanguageId = order.LanguageId,
            PaymentTerms = order.PaymTerm
        });
    }

    private async Task<string> NextAvailableSalesIdAsync(CancellationToken cancellationToken)
    {
        const int maximumCollisionAttempts = 10_000;
        for (var attempt = 0; attempt < maximumCollisionAttempts; attempt++)
        {
            var candidate = (await _numberSequences.NextAsync(
                "SalesTable", cancellationToken: cancellationToken)).Code;
            var exists = await _dbContext.Set<SalesTable>()
                .IgnoreQueryFilters()
                .AsNoTracking()
                .AnyAsync(order => order.SalesId == candidate, cancellationToken);
            if (!exists) return candidate;
        }

        throw new InvalidOperationException(
            "The sales order number sequence could not produce an unused SalesId. " +
            "Review the SalesTable number sequence NextRec value.");
    }
}
