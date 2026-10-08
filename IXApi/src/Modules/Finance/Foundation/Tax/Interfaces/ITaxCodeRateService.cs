using IAX.IXApi.Modules.Finance.Entities;

namespace IAX.IXApi.Modules.Finance.Foundation.Tax;

public interface ITaxCodeRateService
{
    Task SyncTaxDataRateAsync(string taxCode, decimal taxValue, CancellationToken cancellationToken);
}
