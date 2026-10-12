using IAX.IXApi.Modules.Finance.Foundation.LogisticsAddresses;

namespace IAX.IXApi.Modules.Finance.AccountsReceivable;

public interface ISalesOrderDeliveryAddressService
{
    Task<SalesDeliveryAddressResult> CreateAsync(long recId, string? lineIdText,
        AddressInfoDto address, CancellationToken cancellationToken);
}
