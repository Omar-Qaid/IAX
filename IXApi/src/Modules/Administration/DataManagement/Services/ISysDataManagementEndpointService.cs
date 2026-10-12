using IAX.IXApi.Modules.Administration.DataManagement.Contracts;

namespace IAX.IXApi.Modules.Administration.DataManagement.Services;

public interface ISysDataManagementEndpointService
{
    Task<SysImportResult?> ImportAsync(Type entityType, Stream stream, CancellationToken cancellationToken);
    Task<Stream?> ExportAsync(Type entityType, CancellationToken cancellationToken);
    Task<bool> ExportViewAsync(Type entityType, SysExportRequest request, Stream output, CancellationToken cancellationToken);
    Task<Stream?> GenerateTemplateAsync(Type entityType, CancellationToken cancellationToken);
    IEnumerable<object> GetFields(Type entityType);
}
