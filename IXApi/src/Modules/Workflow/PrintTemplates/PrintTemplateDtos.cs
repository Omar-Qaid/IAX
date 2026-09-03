using IAX.IXApi.Shared.Domain.Reporting;

namespace IAX.IXApi.Modules.Workflow.PrintTemplates;

public class PrintTemplateSummaryDto
{
    public long TemplateId { get; set; }
    public int RefTableId { get; set; }
    public long RefRecId { get; set; }
    // Compatibility fields retained for existing Workflow clients.
    public long ProcessId { get; set; }
    public string ProcessName { get; set; } = string.Empty;
    public string Code { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string? NameAlias { get; set; }
    public string? Description { get; set; }
    public string PageSize { get; set; } = "A4";
    public string Orientation { get; set; } = "portrait";
    public string Language { get; set; } = "en";
    public bool IsDefault { get; set; }
    public ReportTemplateStatus Status { get; set; }
    public long? CurrentVersionId { get; set; }
    public int? CurrentVersionNo { get; set; }
    public int LatestVersionNo { get; set; }
    public bool HasDraft { get; set; }
    public bool IsActive { get; set; }
    public DateTime? LastModifiedAt { get; set; }
}

public sealed class PrintTemplateDto : PrintTemplateSummaryDto
{
    public long EditableVersionId { get; set; }
    public int EditableVersionNo { get; set; }
    public bool EditableVersionPublished { get; set; }
    public PrintTemplateDocument Document { get; set; } = new();
    public List<PrintTemplateVersionDto> Versions { get; set; } = [];
}

public sealed class PublishedPrintTemplateDto : PrintTemplateSummaryDto
{
    public long TemplateVersionId { get; set; }
    public int VersionNo { get; set; }
    public PrintTemplateDocument Document { get; set; } = new();
}

public sealed class PrintTemplateVersionDto
{
    public long TemplateVersionId { get; set; }
    public int VersionNo { get; set; }
    public bool IsPublished { get; set; }
    public string? PublishedBy { get; set; }
    public DateTime? PublishedAt { get; set; }
    public DateTime? CreatedAt { get; set; }
}

public sealed class CreatePrintTemplateDto
{
    public int RefTableId { get; set; }
    public long RefRecId { get; set; }
    public long? ProcessId { get; set; }
    public string Code { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string? NameAlias { get; set; }
    public string? Description { get; set; }
    public bool IsDefault { get; set; }
    public PrintTemplateDocument Document { get; set; } = new();
}

public sealed class UpdatePrintTemplateDto
{
    public string Code { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string? NameAlias { get; set; }
    public string? Description { get; set; }
    public bool IsDefault { get; set; }
    public PrintTemplateDocument Document { get; set; } = new();
}

public sealed class PublishPrintTemplateDto
{
    public long? TemplateVersionId { get; set; }
}

public sealed class PrintTemplateValidationResultDto
{
    public bool IsValid => Errors.Count == 0;
    public List<string> Errors { get; set; } = [];
}

public sealed class PrintTemplateValidationException(IEnumerable<string> errors)
    : Exception("The print template is invalid.")
{
    public IReadOnlyList<string> Errors { get; } = errors.Distinct().ToList();
}
