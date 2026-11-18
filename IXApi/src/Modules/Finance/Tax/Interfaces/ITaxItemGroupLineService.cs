using IAX.IXApi.Modules.Finance.Entities;

namespace IAX.IXApi.Modules.Finance.Tax;

public interface ITaxItemGroupLineService
{
    Task<TaxOnItemDto?> AddLineAsync(string id, TaxOnItemDto lineDto);
    Task<bool> DeleteLineAsync(long lineId);
}
