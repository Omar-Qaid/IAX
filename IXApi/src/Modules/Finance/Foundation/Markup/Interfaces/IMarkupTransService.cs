namespace IAX.IXApi.Modules.Finance.Foundation.Markup;

public interface IMarkupTransService
{
    Task<MarkupResult<IEnumerable<MarkupTransDto>>> GetDocumentChargesAsync(string documentType, long documentRecId, string level = "header", CancellationToken cancellationToken = default);
    Task<MarkupResult<IEnumerable<MarkupCodeDto>>> GetChargeCodesAsync(string documentType, CancellationToken cancellationToken = default);
    Task<MarkupResult<MarkupTransDto>> CreateAsync(string documentType, long documentRecId, MarkupTransDto input, string level = "header", CancellationToken cancellationToken = default);
    Task<MarkupResult<MarkupTransDto>> UpdateAsync(long id, MarkupTransDto input, CancellationToken cancellationToken = default);
    Task<MarkupResult<bool>> DeleteAsync(long id, CancellationToken cancellationToken = default);
}
