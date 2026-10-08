using IAX.IXApi.Modules.Finance.AccountsReceivable;
using IAX.IXApi.Modules.Finance.Entities;
using Mapster;

namespace IAX.IXApi.Modules.Finance.AccountsReceivable.Customer;

public sealed record CustomerMappingSource(CustTable Customer, DirPartyTable? Party);

public sealed class CustomerMapping : IRegister
{
    public void Register(TypeAdapterConfig config)
    {
        config.NewConfig<CustomerMappingSource, CustomerListDto>()
            .Map(d => d.RecId, s => s.Customer.RecId)
            .Map(d => d.Party, s => s.Customer.Party)
            .Map(d => d.AccountNumber, s => s.Customer.AccountNum)
            .Map(d => d.Name, s => s.Party != null ? s.Party.Name : s.Customer.AccountNum)
            .Map(d => d.NameAr, s => s.Party != null ? s.Party.NameAlias : null)
            .Map(d => d.CustomerGroupId, s => s.Customer.CustGroupId)
            .Map(d => d.CurrencyCode, s => s.Customer.CurrencyCode)
            .Map(d => d.CustCategory, s => s.Customer.CustCategory)
            .Map(d => d.PaymTermId, s => s.Customer.PaymTermId)
            .Map(d => d.PaymModeId, s => s.Customer.PaymModeId)
            .Map(d => d.DlvModeId, s => s.Customer.DlvModeId)
            .Map(d => d.TaxGroupId, s => s.Customer.TaxGroupId)
            .Map(d => d.VatNum, s => s.Customer.VatNum)
            .Map(d => d.CountryRegionId, s => s.Customer.CountryRegionId)
            .Map(d => d.Memo, s => s.Customer.Memo)
            .Map(d => d.InvoiceAccount, s => s.Customer.InvoiceAccount)
            .Map(d => d.InventSiteId, s => s.Customer.InventSiteId)
            .Map(d => d.InventLocationId, s => s.Customer.InventLocationId)
            .Map(d => d.SalesPoolId, s => s.Customer.SalesPoolId)
            .Map(d => d.CashDiscBaseDays, s => s.Customer.CashDiscBaseDays)
            .Map(d => d.UseCashDisc, s => s.Customer.UseCashDisc)
            .Map(d => d.InclTax, s => s.Customer.InclTax)
            .Map(d => d.BlockFloorLimitUseInChannel, s => s.Customer.BlockFloorLimitUseInChannel)
            .Map(d => d.PrepaymentValue, s => s.Customer.PrepaymentValue)
            .Map(d => d.PrePayType, s => s.Customer.PrePayType)
            .Map(d => d.Status, s => s.Customer.IsActive ? "active" : "inactive")
            .Map(d => d.CreatedAt, s => s.Customer.CreatedAt ?? DateTime.UtcNow);
    }
}
