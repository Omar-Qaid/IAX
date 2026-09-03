namespace IAX.IXApi.Modules.Workflow.PrintTemplates;

public interface IPrintTemplateService
{
    Task<IReadOnlyList<PrintTemplateSummaryDto>> ListByRecordAsync(int refTableId, long refRecId, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<PrintTemplateSummaryDto>> ListPublishedByRecordAsync(int refTableId, long refRecId, CancellationToken cancellationToken = default);
    Task<PublishedPrintTemplateDto?> GetPublishedForRecordAsync(int refTableId, long refRecId, long templateId, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<PrintTemplateSummaryDto>> ListByProcessAsync(long processId, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<PrintTemplateSummaryDto>> ListPublishedByProcessAsync(long processId, CancellationToken cancellationToken = default);
    Task<PrintTemplateDto?> GetAsync(long templateId, CancellationToken cancellationToken = default);
    Task<PublishedPrintTemplateDto?> GetPublishedForProcessAsync(long processId, long templateId, CancellationToken cancellationToken = default);
    Task<PublishedPrintTemplateDto?> GetPublishedForRequestAsync(long requestId, long templateId, CancellationToken cancellationToken = default);
    Task<PrintTemplateDto> CreateAsync(CreatePrintTemplateDto input, CancellationToken cancellationToken = default);
    Task<PrintTemplateDto?> UpdateAsync(long templateId, UpdatePrintTemplateDto input, CancellationToken cancellationToken = default);
    Task<PrintTemplateDto?> PublishAsync(long templateId, long? templateVersionId, CancellationToken cancellationToken = default);
    Task<PrintTemplateDto?> ArchiveAsync(long templateId, CancellationToken cancellationToken = default);
    Task<bool> DeleteDraftAsync(long templateId, CancellationToken cancellationToken = default);
    Task<PrintTemplateValidationResultDto?> ValidateAsync(long templateId, CancellationToken cancellationToken = default);
}
