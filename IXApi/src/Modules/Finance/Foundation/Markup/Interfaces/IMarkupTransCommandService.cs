using IAX.IXApi.Modules.Finance.AccountsReceivable;
using IAX.IXApi.Modules.Finance.Common;

namespace IAX.IXApi.Modules.Finance.Foundation.Markup;

public interface IMarkupTransCommandService
{
    Task<MarkupTrans> CreateAsync(MarkupModuleType moduleType, int transTableId, long documentRecId, MarkupTransDto input, CancellationToken ct);
    Task<MarkupTrans> UpdateAsync(MarkupTrans entity, MarkupTransDto input, CancellationToken ct);
    Task DeleteAsync(MarkupTrans entity, CancellationToken ct);
}
