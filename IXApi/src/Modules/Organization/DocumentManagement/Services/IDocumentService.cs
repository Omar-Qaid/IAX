using IAX.IXApi.Modules.Organization.DocumentManagement.Models;

namespace IAX.IXApi.Modules.Organization.DocumentManagement.Services;

public interface IDocumentService
{
    Task<IReadOnlyList<DocuTypeDto>> GetTypesAsync(CancellationToken cancellationToken = default);
    Task<DocumentPageDto> GetForRecordAsync(int refTableId, long refRecId, int pageNumber, int pageSize, CancellationToken cancellationToken = default);
    Task<DocumentDto?> GetAsync(long id, CancellationToken cancellationToken = default);
    Task<DocumentDto> CreateAsync(CreateDocumentCommand command, CancellationToken cancellationToken = default);
    Task<DocumentDto?> UpdateAsync(long id, UpdateDocumentRequest request, CancellationToken cancellationToken = default);
    Task<DocumentContent?> OpenContentAsync(long id, CancellationToken cancellationToken = default);
    Task<bool> DeleteAsync(long id, CancellationToken cancellationToken = default);
}
