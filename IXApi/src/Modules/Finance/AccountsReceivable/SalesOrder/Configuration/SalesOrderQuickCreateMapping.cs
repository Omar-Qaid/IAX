using IAX.IXApi.Modules.Finance.Common;
using IAX.IXApi.Modules.Finance.Entities;
using Mapster;

namespace IAX.IXApi.Modules.Finance.AccountsReceivable;

public sealed record SalesOrderQuickCreateMappingSource(SalesTable Order, string CustomerName);

public sealed class SalesOrderQuickCreateMapping : IRegister
{
    public void Register(TypeAdapterConfig config)
    {
        config.NewConfig<SalesOrderQuickCreateMappingSource, SalesOrderListDto>()
            .Map(d => d.RecId, s => s.Order.RecId)
            .Map(d => d.SalesId, s => s.Order.SalesId)
            .Map(d => d.CustomerAccount, s => s.Order.CustAccount)
            .Map(d => d.CustomerName, s => s.CustomerName)
            .Map(d => d.InvoiceAccount, s => s.Order.InvoiceAccount)
            .Map(d => d.CustomerGroup, s => s.Order.CustGroup)
            .Map(d => d.CurrencyCode, s => s.Order.CurrencyCode)
            .Map(d => d.SalesStatus, s => s.Order.SalesStatus.ToString())
            .Map(d => d.DocumentStatus, s => s.Order.DocumentStatus.ToString())
            .Map(d => d.DeliveryDate, s => s.Order.DeliveryDate)
            .Map(d => d.ShippingDateRequested, s => s.Order.ShippingDateRequested)
            .Map(d => d.OrderTotal, s => s.Order.SmmSalesAmountTotal)
            .Map(d => d.CustomerReference, s => s.Order.CustomerRef)
            .Map(d => d.DeliveryMode, s => s.Order.DlvMode)
            .Map(d => d.DeliveryTerms, s => s.Order.DlvTerm)
            .Map(d => d.OrderDate, s => s.Order.OrderDate)
            .Map(d => d.InventSiteId, s => s.Order.InventSiteId)
            .Map(d => d.InventLocationId, s => s.Order.InventLocationId)
            .Map(d => d.SalesNameAlias, s => s.Order.SalesNameAlias)
            .Map(d => d.SalesType, s => (int)(s.Order.SalesType ?? SalesType.Sales))
            .Map(d => d.OneTimeCustomer, s => s.Order.OneTimeCustomer == NoYes.Yes)
            .Map(d => d.Email, s => s.Order.Email)
            .Map(d => d.Phone, s => s.Order.Phone)
            .Map(d => d.Deadline, s => s.Order.Deadline == default ? (DateTime?)null : s.Order.Deadline)
            .Map(d => d.CustomerRequisitionNumber, s => s.Order.CustRequisitionNum)
            .Map(d => d.Notes, s => s.Order.Notes)
            .Map(d => d.CampaignId, s => s.Order.SmmCampaignId)
            .Map(d => d.TaxGroupId, s => s.Order.TaxGroupId)
            .Map(d => d.PricesIncludeSalesTax, s => s.Order.InclTax)
            .Map(d => d.SalesGroup, s => s.Order.SalesGroup)
            .Map(d => d.LanguageId, s => s.Order.LanguageId)
            .Map(d => d.PaymentTerms, s => s.Order.PaymTerm);
    }
}
