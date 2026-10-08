using IAX.IXApi.Modules.Finance.AccountsReceivable;
using Mapster;

namespace IAX.IXApi.Modules.Finance.AccountsReceivable.Customer;

public sealed record CustomerQuickUpdateSource(CustomerQuickCreateDto Input, CustTable Customer);

public sealed class CustomerQuickMapping : IRegister
{
    public void Register(TypeAdapterConfig config)
    {
        config.NewConfig<CustomerQuickCreateDto, CustTable>()
            .Map(d => d.CustGroupId, s => s.CustGroupId.Trim())
            .Map(d => d.CurrencyCode, s => s.CurrencyCode.Trim())
            .Map(d => d.CustCategory, s => s.CustCategory != null ? s.CustCategory.Trim() : string.Empty)
            .Map(d => d.PaymTermId, s => s.PaymTermId != null ? s.PaymTermId.Trim() : string.Empty)
            .Map(d => d.PaymModeId, s => s.PaymModeId != null ? s.PaymModeId.Trim() : string.Empty)
            .Map(d => d.DlvModeId, s => s.DlvModeId != null ? s.DlvModeId.Trim() : string.Empty)
            .Map(d => d.TaxGroupId, s => s.TaxGroupId != null ? s.TaxGroupId.Trim() : string.Empty)
            .Map(d => d.VatNum, s => s.VatNum != null ? s.VatNum.Trim() : string.Empty)
            .Map(d => d.CountryRegionId, s => s.CountryRegionId != null ? s.CountryRegionId.Trim() : string.Empty)
            .Map(d => d.Memo, s => s.Memo != null ? s.Memo.Trim() : null)
            .Map(d => d.InvoiceAccount, s => s.InvoiceAccount != null ? s.InvoiceAccount.Trim() : string.Empty)
            .Map(d => d.InventSiteId, s => s.InventSiteId != null ? s.InventSiteId.Trim() : string.Empty)
            .Map(d => d.InventLocationId, s => s.InventLocationId != null ? s.InventLocationId.Trim() : string.Empty)
            .Map(d => d.SalesPoolId, s => s.SalesPoolId != null ? s.SalesPoolId.Trim() : string.Empty)
            .Map(d => d.CashDiscBaseDays, s => s.CashDiscBaseDays ?? 0)
            .Map(d => d.UseCashDisc, s => s.UseCashDisc ?? 0)
            .Map(d => d.InclTax, s => s.InclTax ?? 0)
            .Map(d => d.BlockFloorLimitUseInChannel, s => s.BlockFloorLimitUseInChannel ?? 0)
            .Map(d => d.PrepaymentValue, s => s.PrepaymentValue ?? 0)
            .Map(d => d.PrePayType, s => s.PrePayType ?? 0);

        config.NewConfig<CustomerQuickUpdateSource, CustTable>()
            .Map(d => d.CustGroupId, s => s.Input.CustGroupId.Trim())
            .Map(d => d.CurrencyCode, s => s.Input.CurrencyCode.Trim())
            .Map(d => d.CustCategory, s => s.Input.CustCategory != null ? s.Input.CustCategory.Trim() : string.Empty)
            .Map(d => d.PaymTermId, s => s.Input.PaymTermId != null ? s.Input.PaymTermId.Trim() : string.Empty)
            .Map(d => d.PaymModeId, s => s.Input.PaymModeId != null ? s.Input.PaymModeId.Trim() : string.Empty)
            .Map(d => d.DlvModeId, s => s.Input.DlvModeId != null ? s.Input.DlvModeId.Trim() : string.Empty)
            .Map(d => d.TaxGroupId, s => s.Input.TaxGroupId != null ? s.Input.TaxGroupId.Trim() : string.Empty)
            .Map(d => d.VatNum, s => s.Input.VatNum != null ? s.Input.VatNum.Trim() : string.Empty)
            .Map(d => d.CountryRegionId, s => s.Input.CountryRegionId != null ? s.Input.CountryRegionId.Trim() : string.Empty)
            .Map(d => d.Memo, s => s.Input.Memo)
            .Map(d => d.InvoiceAccount, s => s.Input.InvoiceAccount != null ? s.Input.InvoiceAccount.Trim() : s.Customer.InvoiceAccount)
            .Map(d => d.InventSiteId, s => s.Input.InventSiteId != null ? s.Input.InventSiteId.Trim() : s.Customer.InventSiteId)
            .Map(d => d.InventLocationId, s => s.Input.InventLocationId != null ? s.Input.InventLocationId.Trim() : s.Customer.InventLocationId)
            .Map(d => d.SalesPoolId, s => s.Input.SalesPoolId != null ? s.Input.SalesPoolId.Trim() : s.Customer.SalesPoolId)
            .Map(d => d.CashDiscBaseDays, s => s.Input.CashDiscBaseDays ?? s.Customer.CashDiscBaseDays)
            .Map(d => d.UseCashDisc, s => s.Input.UseCashDisc ?? s.Customer.UseCashDisc)
            .Map(d => d.InclTax, s => s.Input.InclTax ?? s.Customer.InclTax)
            .Map(d => d.BlockFloorLimitUseInChannel, s => s.Input.BlockFloorLimitUseInChannel ?? s.Customer.BlockFloorLimitUseInChannel)
            .Map(d => d.PrepaymentValue, s => s.Input.PrepaymentValue ?? s.Customer.PrepaymentValue)
            .Map(d => d.PrePayType, s => s.Input.PrePayType ?? s.Customer.PrePayType);
    }
}
