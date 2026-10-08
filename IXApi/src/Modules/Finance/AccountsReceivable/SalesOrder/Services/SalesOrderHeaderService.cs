using IAX.IXApi.Modules.Finance.Common;
using IAX.IXApi.Modules.Finance.Entities;
using IAX.IXApi.Modules.Finance.Persistence;
using IAX.IXApi.Shared.Application.Identity;
using Microsoft.EntityFrameworkCore;

namespace IAX.IXApi.Modules.Finance.AccountsReceivable;

public enum SalesHeaderUpdateStatus { NotFound, Invalid, Saved }
public sealed record SalesHeaderUpdateResult(SalesHeaderUpdateStatus Status, string? Error = null);

public sealed class SalesOrderHeaderService
{
    private readonly IFinanceDataContext _dbContext;
    private readonly ICompanyExecutionContext _company;
    private readonly SalesOrderValidationService _validation;

    public SalesOrderHeaderService(IFinanceDataContext dbContext, ICompanyExecutionContext company,
        SalesOrderValidationService validation)
    {
        _dbContext = dbContext;
        _company = company;
        _validation = validation;
    }

    public async Task<SalesHeaderUpdateResult> UpdateAsync(long recId, UpdateSalesHeaderInputDto input,
        CancellationToken cancellationToken)
    {
        return await _dbContext.Database.CreateExecutionStrategy().ExecuteAsync(async () =>
        {
            await using var transaction = await _dbContext.Database.BeginTransactionAsync(System.Data.IsolationLevel.Serializable, cancellationToken);
            var order = await _dbContext.Set<SalesTable>().FirstOrDefaultAsync(row => row.RecId == recId, cancellationToken);
            if (order == null) return new SalesHeaderUpdateResult(SalesHeaderUpdateStatus.NotFound, "Sales order was not found.");
            if (!string.Equals(order.DataAreaId, _company.GetDataAreaId(), StringComparison.OrdinalIgnoreCase))
                return new SalesHeaderUpdateResult(SalesHeaderUpdateStatus.NotFound, "Sales order was not found in the selected company.");
            if (order.SalesStatus != SalesStatus.Backorder)
                return new SalesHeaderUpdateResult(SalesHeaderUpdateStatus.Invalid, "Only open sales orders can be changed.");
            var invoiceAccount = string.IsNullOrWhiteSpace(input.InvoiceAccount) ? order.InvoiceAccount : input.InvoiceAccount.Trim();
            if (!await _dbContext.Set<CustTable>().AnyAsync(row => row.AccountNum == invoiceAccount && row.DataAreaId == order.DataAreaId, cancellationToken))
                return new SalesHeaderUpdateResult(SalesHeaderUpdateStatus.Invalid, "Invoice account was not found.");
            var currency = string.IsNullOrWhiteSpace(input.CurrencyCode) ? order.CurrencyCode : input.CurrencyCode.Trim();
            if (!await _dbContext.Set<Currency>().AnyAsync(row => row.CurrencyCode == currency
                    && row.DataAreaId == order.DataAreaId, cancellationToken))
                return new SalesHeaderUpdateResult(SalesHeaderUpdateStatus.Invalid, "Currency was not found in the selected company.");
            var paymentTerms = input.PaymentTerms.Trim();
            if (!string.IsNullOrEmpty(paymentTerms)
                && !await _dbContext.Set<PaymTerm>().AnyAsync(term => term.PaymTermId == paymentTerms, cancellationToken))
                return new SalesHeaderUpdateResult(SalesHeaderUpdateStatus.Invalid, "Payment terms were not found.");
            var paymentMethod = input.PaymentMethod.Trim();
            if (!string.IsNullOrEmpty(paymentMethod)
                && !await _dbContext.Set<CustPaymModeTable>().AnyAsync(mode => mode.PaymMode == paymentMethod, cancellationToken))
                return new SalesHeaderUpdateResult(SalesHeaderUpdateStatus.Invalid, "Payment method was not found.");
            var deliveryMode = input.DeliveryMode.Trim();
            if (!string.IsNullOrEmpty(deliveryMode)
                && !await _dbContext.Set<DlvMode>().AnyAsync(mode => mode.Code == deliveryMode, cancellationToken))
                return new SalesHeaderUpdateResult(SalesHeaderUpdateStatus.Invalid, "Delivery mode was not found.");
            var deliveryTerms = input.DeliveryTerms.Trim();
            if (!string.IsNullOrEmpty(deliveryTerms)
                && !await _dbContext.Set<DlvTerm>().AnyAsync(term => term.Code == deliveryTerms, cancellationToken))
                return new SalesHeaderUpdateResult(SalesHeaderUpdateStatus.Invalid, "Delivery terms were not found.");
            if (currency != order.CurrencyCode && await _dbContext.Set<SalesLine>().AnyAsync(row => row.SalesId == order.SalesId && row.DataAreaId == order.DataAreaId, cancellationToken))
                return new SalesHeaderUpdateResult(SalesHeaderUpdateStatus.Invalid, "Currency cannot be changed after sales lines have been added.");
            order.InvoiceAccount = invoiceAccount;
            order.CurrencyCode = currency;
            order.CustomerRef = input.CustomerReference?.Trim() ?? string.Empty;
            order.PaymTerm = paymentTerms;
            order.DlvMode = deliveryMode;
            order.DlvTerm = deliveryTerms;
            var inventSiteId = input.InventSiteId.Trim();
            var inventLocationId = input.InventLocationId.Trim();
            if (!string.IsNullOrEmpty(inventSiteId) &&
                !await _dbContext.Set<InventSite>().AnyAsync(row => row.SiteId == inventSiteId
                    && row.DataAreaId == order.DataAreaId, cancellationToken))
                return new SalesHeaderUpdateResult(SalesHeaderUpdateStatus.Invalid, "Site was not found.");
            if (!string.IsNullOrEmpty(inventLocationId) &&
                !await _dbContext.Set<InventLocation>().AnyAsync(row => row.InventLocationId == inventLocationId
                    && row.InventSiteId == inventSiteId && row.DataAreaId == order.DataAreaId, cancellationToken))
                return new SalesHeaderUpdateResult(SalesHeaderUpdateStatus.Invalid, "Warehouse was not found in the selected site.");
            order.InventSiteId = inventSiteId;
            order.InventLocationId = inventLocationId;
            order.OrderDate = input.OrderDate?.Date ?? order.OrderDate;
            order.SalesNameAlias = input.SalesNameAlias.Trim();
            order.SalesType = input.SalesType;
            order.OneTimeCustomer = input.OneTimeCustomer ? NoYes.Yes : NoYes.No;
            order.Email = input.Email.Trim();
            order.Phone = input.Phone.Trim();
            order.Deadline = input.Deadline?.Date ?? default;
            order.CustRequisitionNum = input.CustomerRequisitionNumber.Trim();
            order.SmmCampaignId = input.CampaignId.Trim();
            var taxGroupId = input.TaxGroupId.Trim();
            if (!string.IsNullOrEmpty(taxGroupId)
                && !await _dbContext.Set<TaxGroupHeading>().AsNoTracking()
                    .AnyAsync(group => group.TaxGroup == taxGroupId, cancellationToken))
                return new SalesHeaderUpdateResult(SalesHeaderUpdateStatus.Invalid, "Sales tax group was not found.");
            order.TaxGroupId = taxGroupId;
            order.InclTax = input.PricesIncludeSalesTax;
            order.SalesGroup = input.SalesGroup.Trim();
            order.LanguageId = input.LanguageId.Trim();
            order.DeliveryDate = input.DeliveryDate?.Date ?? order.DeliveryDate;
            order.ReceiptDateRequested = order.DeliveryDate;
            order.ShippingDateRequested = input.ShippingDateRequested?.Date ?? default;
            var deliveryName = input.DeliveryName.Trim();
            var deliveryAddressId = 0L;
            if (!string.IsNullOrWhiteSpace(input.DeliveryPostalAddress)
                && input.DeliveryPostalAddress != "0"
                && (!long.TryParse(input.DeliveryPostalAddress, out deliveryAddressId)
                    || !await _validation.IsDeliveryAddressAvailableForOrderAsync(order, deliveryAddressId, cancellationToken)))
                return new SalesHeaderUpdateResult(SalesHeaderUpdateStatus.Invalid, "The selected delivery address does not belong to this customer.");
            order.DeliveryName = deliveryName;
            order.DeliveryPostalAddress = deliveryAddressId;
            order.ShippingDateConfirmed = input.ShippingDateConfirmed?.Date ?? default;
            order.ReceiptDateConfirmed = input.ReceiptDateConfirmed?.Date ?? default;
            order.DeliveryDateControlType = input.DeliveryDateControlType;
            order.MpsFullRunCtpStatus = input.MpsFullRunCtpStatus;
            order.ShipCarrierBlindShipment = input.BlindShipment ? NoYes.Yes : NoYes.No;
            order.ShipCarrierResidential = input.ResidentialDestination;
            order.MpsExcludeSalesOrder = input.ExcludeFromMasterPlanning;
            order.DlvReason = input.DeliveryReason.Trim();
            order.ExportReason = input.ExportReason.Trim();
            order.ShipCarrierName = input.ShippingCarrier.Trim();
            order.ShipCarrierId = input.CarrierId.Trim();
            order.MarkupGroup = input.CarrierGroup.Trim();
            order.ShipCarrierAccountCode = input.BrokerId.Trim();
            order.Transport = input.TransportMode.Trim();
            order.ShipCarrierDlvType = input.CarrierService;
            order.PaymMode = paymentMethod;
            order.PaymentSched = input.PaymentSchedule.Trim();
            order.PaymSpec = input.PaymentSpecification.Trim();
            order.FixedDueDate = input.FixedDueDate?.Date ?? default;
            order.CashDiscBaseDate = input.PaymentTermsBaseDate?.Date ?? default;
            order.CashDisc = input.CashDiscountCode.Trim();
            order.CashDiscPercent = input.DiscountPercent;
            order.DiscPercent = input.TotalDiscountPercent;
            order.FixedExchRate = input.FixedExchangeRate;
            order.PriceGroupId = input.PriceGroup.Trim();
            order.LineDisc = input.LineDiscountGroup.Trim();
            order.MultiLineDisc = input.MultiLineDiscountGroup.Trim();
            order.EndDisc = input.TotalDiscountGroup.Trim();
            order.MarkupGroup = input.ChargesGroup.Trim();
            order.PdsCustRebateGroupId = input.CustomerRebateGroup.Trim();
            order.PdsRebateProgramTmaGroup = input.CustomerTmaGroup.Trim();
            order.TamRebateReference = input.RebateReference.Trim();
            order.SalesPoolId = input.SalesPool.Trim();
            order.ShipCarrierAccount = input.CarrierCustomerAccount.Trim();
            order.FreightZone = input.FreightZone.Trim();
            order.Notes = input.Notes.Trim();
            order.IntercompanyAutoCreateOrders = input.IntercompanyAutoCreateOrders;
            order.IntercompanyDirectDelivery = input.IntercompanyDirectDelivery;
            order.IntercompanyOrigin = input.IntercompanyOrigin;
            order.IntercompanyAllowIndirectCreation = input.IntercompanyAllowIndirectCreation;
            order.Reservation = input.Reservation;
            await _dbContext.SaveChangesAsync(cancellationToken);
            await transaction.CommitAsync(cancellationToken);
            return new SalesHeaderUpdateResult(SalesHeaderUpdateStatus.Saved);
        });
    }
}
