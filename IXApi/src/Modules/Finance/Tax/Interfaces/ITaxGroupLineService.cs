using IAX.IXApi.Modules.Finance.Entities;

namespace IAX.IXApi.Modules.Finance.Tax;

public interface ITaxGroupLineService
{
    Task<(TaxGroupDataDto? Line, bool Updated)> AddLineAsync(string id, TaxGroupDataDto lineDto);
    Task<bool> DeleteLineAsync(long lineId);
}
