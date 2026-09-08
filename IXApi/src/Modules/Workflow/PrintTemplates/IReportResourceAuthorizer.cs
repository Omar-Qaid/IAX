namespace IAX.IXApi.Modules.Workflow.PrintTemplates;

public interface IReportResourceAuthorizer
{
    Task<bool> CanReadAsync(int refTableId, long refRecId, CancellationToken cancellationToken = default);
    Task<bool> CanDesignAsync(int refTableId, long refRecId, CancellationToken cancellationToken = default);
    Task<string> GetDisplayNameAsync(int refTableId, long refRecId, CancellationToken cancellationToken = default);
}
