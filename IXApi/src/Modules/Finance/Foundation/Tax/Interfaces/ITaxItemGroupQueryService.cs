using IAX.IXApi.Modules.Finance.Entities;

namespace IAX.IXApi.Modules.Finance.Foundation.Tax;

public interface ITaxItemGroupQueryService
{
    Task<IEnumerable<TaxItemGroupDto>> GetAllAsync(CancellationToken cancellationToken = default);
    Task<TaxItemGroupDto?> GetByIdAsync(string id, CancellationToken cancellationToken = default);
}
