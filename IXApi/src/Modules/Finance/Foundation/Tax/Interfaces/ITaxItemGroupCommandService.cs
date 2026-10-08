using IAX.IXApi.Modules.Finance.Entities;

namespace IAX.IXApi.Modules.Finance.Foundation.Tax;

public interface ITaxItemGroupCommandService
{
    Task<TaxItemGroupHeading> CreateAsync(TaxItemGroupDto dto, CancellationToken cancellationToken = default);
    Task<TaxItemGroupHeading?> UpdateAsync(string id, TaxItemGroupDto dto, CancellationToken cancellationToken = default);
}
