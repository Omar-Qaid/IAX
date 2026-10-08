using IAX.IXApi.Modules.Finance.Entities;

namespace IAX.IXApi.Modules.Finance.Foundation.Tax;

public interface ITaxPeriodService
{
    Task<IEnumerable<TaxPeriodHeadDto>> GetAllAsync(CancellationToken cancellationToken = default);
    Task<TaxPeriodHeadDto?> GetByIdAsync(string id, CancellationToken cancellationToken = default);
    Task<TaxPeriodHead> CreateAsync(TaxPeriodHeadDto dto, CancellationToken cancellationToken = default);
    Task<TaxPeriodHead?> UpdateAsync(string id, TaxPeriodHeadDto dto, CancellationToken cancellationToken = default);
    Task<bool> DeleteAsync(string id, CancellationToken cancellationToken = default);
}
