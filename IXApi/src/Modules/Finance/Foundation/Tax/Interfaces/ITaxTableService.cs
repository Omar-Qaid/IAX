using IAX.IXApi.Infrastructure.Persistence.Services;
using IAX.IXApi.Modules.Finance.Entities;

namespace IAX.IXApi.Modules.Finance.Foundation.Tax;

public interface ITaxTableService : IBaseService<TaxTable> {
    Task<IEnumerable<TaxTableDto>> GetDetailsAsync(CancellationToken cancellationToken = default);
    Task<TaxTableDto?> GetDetailAsync(string id, CancellationToken cancellationToken = default);
    Task<TaxTableDto?> CreateFromDtoAsync(TaxTableDto dto, CancellationToken cancellationToken = default);
    Task<TaxTableDto?> UpdateFromDtoAsync(string id, TaxTableDto dto, CancellationToken cancellationToken = default);
    Task<bool> DeleteByCodeAsync(string id, CancellationToken cancellationToken = default);
}
