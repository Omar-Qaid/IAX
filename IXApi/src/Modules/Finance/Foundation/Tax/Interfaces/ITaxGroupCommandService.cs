using IAX.IXApi.Modules.Finance.Entities;

namespace IAX.IXApi.Modules.Finance.Foundation.Tax;

public interface ITaxGroupCommandService
{
    Task<TaxGroupHeading> CreateAsync(TaxGroupDto dto, CancellationToken cancellationToken = default);
    Task<TaxGroupHeading?> UpdateAsync(string id, TaxGroupDto dto, CancellationToken cancellationToken = default);
}
