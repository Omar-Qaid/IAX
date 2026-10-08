using Mapster;

namespace IAX.IXApi.Modules.Finance.AccountsReceivable.Confirm;

public sealed class SalesOrderConfirmationMapping : IRegister
{
    public void Register(TypeAdapterConfig config)
    {
        config.NewConfig<CustConfirmJour, ConfirmationListItemDto>()
            .Map(d => d.Id, s => s.RecId.ToString());
        config.NewConfig<CustConfirmJour, ConfirmationHeaderDto>()
            .Map(d => d.Id, s => s.RecId.ToString())
            .Map(d => d.DeliveryPostalAddress, s => s.DeliveryPostalAddress.ToString());
        config.NewConfig<CustConfirmTrans, ConfirmationLineDto>()
            .Map(d => d.Id, s => s.RecId.ToString());
    }
}
