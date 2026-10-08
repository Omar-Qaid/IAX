using IAX.IXApi.Modules.Finance.Entities;

namespace IAX.IXApi.Modules.Finance.Foundation.Tax;

public interface ITaxGroupQueryService
{
    Task<IEnumerable<TaxGroupDto>> GetAllAsync(CancellationToken cancellationToken = default);
    Task<TaxGroupDto?> GetByIdAsync(string id, CancellationToken cancellationToken = default);
}
