using IAX.IXApi.Infrastructure.Identity;
using IAX.IXApi.Modules.Workflow.Persistence;
using IAX.IXApi.Shared.Domain.Reporting;
using Microsoft.EntityFrameworkCore;
using System.Text.Json;

namespace IAX.IXApi.Modules.Workflow.PrintTemplates;

public sealed class PrintTemplateService : IPrintTemplateService
{
    public const int WorkflowProcessTableId = 476793887;
    public const int WorkflowRequestTableId = 1001;
    private static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web);
    private readonly IWorkflowDataContext _context;
    private readonly ICurrentUserService _currentUser;
    private readonly PrintTemplateDocumentValidator _documentValidator;

    public PrintTemplateService(
        IWorkflowDataContext context,
        ICurrentUserService currentUser,
        PrintTemplateDocumentValidator documentValidator)
    {
        _context = context;
        _currentUser = currentUser;
        _documentValidator = documentValidator;
    }

    public Task<IReadOnlyList<PrintTemplateSummaryDto>> ListByProcessAsync(long processId, CancellationToken cancellationToken = default) =>
        ListByRecordAsync(WorkflowProcessTableId, processId, cancellationToken);

    public async Task<IReadOnlyList<PrintTemplateSummaryDto>> ListByRecordAsync(int refTableId, long refRecId, CancellationToken cancellationToken = default)
    {
        return await _context.ReportTemplates.AsNoTracking()
            .Where(item => item.RefTableId == refTableId && item.RefRecId == refRecId)
            .OrderByDescending(item => item.IsDefault).ThenBy(item => item.Name)
            .Select(item => new PrintTemplateSummaryDto
            {
                TemplateId = item.RecId,
                RefTableId = item.RefTableId,
                RefRecId = item.RefRecId,
                ProcessId = item.RefRecId,
                ProcessName = _context.WfProcesses.Where(process => process.RecId == item.RefRecId)
                    .Select(process => process.Name ?? process.Code ?? string.Empty).FirstOrDefault() ?? string.Empty,
                Code = item.Code ?? string.Empty,
                Name = item.Name ?? string.Empty,
                NameAlias = item.NameAlias,
                Description = item.Description,
                PageSize = item.PageSize,
                Orientation = item.Orientation,
                Language = item.Language,
                IsDefault = item.IsDefault,
                Status = item.Status,
                CurrentVersionId = item.CurrentVersionId,
                CurrentVersionNo = item.CurrentVersion == null ? null : item.CurrentVersion.VersionNo,
                LatestVersionNo = item.Versions.Max(version => (int?)version.VersionNo) ?? 0,
                HasDraft = item.Versions.Any(version => !version.IsPublished),
                IsActive = item.IsActive,
                LastModifiedAt = item.LastModifiedAt
            })
            .ToListAsync(cancellationToken);
    }

    public Task<IReadOnlyList<PrintTemplateSummaryDto>> ListPublishedByProcessAsync(long processId, CancellationToken cancellationToken = default) =>
        ListPublishedByRecordAsync(WorkflowProcessTableId, processId, cancellationToken);

    public async Task<IReadOnlyList<PrintTemplateSummaryDto>> ListPublishedByRecordAsync(int refTableId, long refRecId, CancellationToken cancellationToken = default)
    {
        return await _context.ReportTemplates.AsNoTracking()
            .Where(item => item.RefTableId == refTableId && item.RefRecId == refRecId
                && item.IsActive
                && item.Status == ReportTemplateStatus.Published
                && item.CurrentVersionId != null
                && item.CurrentVersion != null
                && item.CurrentVersion.IsPublished)
            .OrderByDescending(item => item.IsDefault).ThenBy(item => item.Name)
            .Select(item => new PrintTemplateSummaryDto
            {
                TemplateId = item.RecId,
                RefTableId = item.RefTableId,
                RefRecId = item.RefRecId,
                ProcessId = item.RefRecId,
                ProcessName = _context.WfProcesses.Where(process => process.RecId == item.RefRecId)
                    .Select(process => process.Name ?? process.Code ?? string.Empty).FirstOrDefault() ?? string.Empty,
                Code = item.Code ?? string.Empty,
                Name = item.Name ?? string.Empty,
                NameAlias = item.NameAlias,
                Description = item.Description,
                PageSize = item.PageSize,
                Orientation = item.Orientation,
                Language = item.Language,
                IsDefault = item.IsDefault,
                Status = item.Status,
                CurrentVersionId = item.CurrentVersionId,
                CurrentVersionNo = item.CurrentVersion!.VersionNo,
                LatestVersionNo = item.CurrentVersion.VersionNo,
                HasDraft = false,
                IsActive = item.IsActive,
                LastModifiedAt = item.LastModifiedAt
            })
            .ToListAsync(cancellationToken);
    }

    public async Task<PrintTemplateDto?> GetAsync(long templateId, CancellationToken cancellationToken = default)
    {
        var template = await TemplateQuery(asNoTracking: true)
            .SingleOrDefaultAsync(item => item.RecId == templateId, cancellationToken);
        if (template == null) return null;
        var result = Map(template);
        result.ProcessName = await ProcessNameAsync(template.RefRecId, cancellationToken);
        return result;
    }

    public async Task<PublishedPrintTemplateDto?> GetPublishedForRequestAsync(
        long requestId,
        long templateId,
        CancellationToken cancellationToken = default)
    {
        var requestProcessId = await _context.WfRequests.AsNoTracking()
            .Where(item => item.RecId == requestId)
            .Select(item => (long?)item.ProcessId)
            .SingleOrDefaultAsync(cancellationToken);
        if (!requestProcessId.HasValue) return null;

        return await GetPublishedForProcessAsync(requestProcessId.Value, templateId, cancellationToken);
    }

    public async Task<PublishedPrintTemplateDto?> GetPublishedForProcessAsync(
        long processId,
        long templateId,
        CancellationToken cancellationToken = default) =>
        await GetPublishedForRecordAsync(WorkflowProcessTableId, processId, templateId, cancellationToken);

    public async Task<PublishedPrintTemplateDto?> GetPublishedForRecordAsync(
        int refTableId,
        long refRecId,
        long templateId,
        CancellationToken cancellationToken = default)
    {
        var template = await TemplateQuery(asNoTracking: true)
            .SingleOrDefaultAsync(item => item.RecId == templateId
                && item.RefTableId == refTableId
                && item.RefRecId == refRecId
                && item.IsActive
                && item.Status == ReportTemplateStatus.Published
                && item.CurrentVersionId != null
                && item.CurrentVersion != null
                && item.CurrentVersion.IsPublished,
                cancellationToken);
        if (template?.CurrentVersion == null) return null;

        var currentVersion = template.CurrentVersion;
        return new PublishedPrintTemplateDto
        {
            TemplateId = template.RecId,
            RefTableId = template.RefTableId,
            RefRecId = template.RefRecId,
            ProcessId = template.RefRecId,
            ProcessName = await ProcessNameAsync(template.RefRecId, cancellationToken),
            Code = template.Code ?? string.Empty,
            Name = template.Name ?? string.Empty,
            NameAlias = template.NameAlias,
            Description = template.Description,
            PageSize = template.PageSize,
            Orientation = template.Orientation,
            Language = template.Language,
            IsDefault = template.IsDefault,
            Status = template.Status,
            CurrentVersionId = template.CurrentVersionId,
            CurrentVersionNo = currentVersion.VersionNo,
            LatestVersionNo = template.Versions.Max(item => item.VersionNo),
            HasDraft = template.Versions.Any(item => !item.IsPublished),
            IsActive = template.IsActive,
            LastModifiedAt = template.LastModifiedAt,
            TemplateVersionId = currentVersion.RecId,
            VersionNo = currentVersion.VersionNo,
            Document = Deserialize(currentVersion.TemplateJson)
        };
    }

    public async Task<PrintTemplateDto> CreateAsync(CreatePrintTemplateDto input, CancellationToken cancellationToken = default)
    {
        var processId = input.RefRecId > 0 ? input.RefRecId : input.ProcessId ?? 0;
        var refTableId = input.RefTableId > 0 ? input.RefTableId : WorkflowProcessTableId;
        var errors = _documentValidator.Validate(input.Document).ToList();
        if (refTableId != WorkflowProcessTableId)
            errors.Add("The Workflow print-template endpoint only accepts WfProcesses records.");
        if (!await _context.WfProcesses.AsNoTracking().AnyAsync(item => item.RecId == processId && item.IsActive, cancellationToken))
            errors.Add("The selected workflow process does not exist or is inactive.");
        if (await CodeExistsAsync(refTableId, processId, input.Code, null, cancellationToken))
            errors.Add($"Template code '{input.Code}' already exists for this process.");
        ThrowIfInvalid(errors);

        long templateId = 0;
        await ExecuteInTransactionAsync(async () =>
        {
            if (input.IsDefault) await ClearOtherDefaultsAsync(refTableId, processId, null, cancellationToken);

            var template = new ReportTemplate
            {
                RefTableId = refTableId,
                RefRecId = processId,
                Code = input.Code.Trim(),
                Name = input.Name.Trim(),
                NameAlias = input.NameAlias?.Trim(),
                Description = input.Description?.Trim(),
                PageSize = input.Document.Page.Size,
                Orientation = input.Document.Page.Orientation,
                Language = input.Document.Language,
                IsDefault = input.IsDefault,
                Status = ReportTemplateStatus.Draft
            };
            _context.ReportTemplates.Add(template);
            await _context.SaveChangesAsync(cancellationToken);
            templateId = template.RecId;

            _context.ReportTemplateVersions.Add(new ReportTemplateVersion
            {
                TemplateId = template.RecId,
                VersionNo = 1,
                TemplateJson = Serialize(input.Document)
            });
            await _context.SaveChangesAsync(cancellationToken);
        }, cancellationToken);
        return (await GetAsync(templateId, cancellationToken))!;
    }

    public async Task<PrintTemplateDto?> UpdateAsync(long templateId, UpdatePrintTemplateDto input, CancellationToken cancellationToken = default)
    {
        var template = await TemplateQuery(asNoTracking: false)
            .SingleOrDefaultAsync(item => item.RecId == templateId, cancellationToken);
        if (template == null) return null;
        if (template.Status == ReportTemplateStatus.Archived)
            throw new PrintTemplateValidationException(["Archived templates cannot be edited."]);

        var errors = _documentValidator.Validate(input.Document).ToList();
        if (await CodeExistsAsync(template.RefTableId, template.RefRecId, input.Code, templateId, cancellationToken))
            errors.Add($"Template code '{input.Code}' already exists for this process.");
        ThrowIfInvalid(errors);

        await ExecuteInTransactionAsync(async () =>
        {
            if (input.IsDefault && !template.IsDefault)
            {
                await ClearOtherDefaultsAsync(template.RefTableId, template.RefRecId, template.RecId, cancellationToken);
                template.IsDefault = true;
            }
            else if (!input.IsDefault)
            {
                template.IsDefault = false;
            }

            template.Code = input.Code.Trim();
            template.Name = input.Name.Trim();
            template.NameAlias = input.NameAlias?.Trim();
            template.Description = input.Description?.Trim();
            template.PageSize = input.Document.Page.Size;
            template.Orientation = input.Document.Page.Orientation;
            template.Language = input.Document.Language;

            var draft = template.Versions.OrderByDescending(item => item.VersionNo).FirstOrDefault(item => !item.IsPublished);
            if (draft == null)
            {
                draft = new ReportTemplateVersion
                {
                    TemplateId = template.RecId,
                    VersionNo = template.Versions.Select(item => item.VersionNo).DefaultIfEmpty().Max() + 1
                };
                _context.ReportTemplateVersions.Add(draft);
            }
            draft.TemplateJson = Serialize(input.Document);
            await _context.SaveChangesAsync(cancellationToken);
        }, cancellationToken);
        return await GetAsync(templateId, cancellationToken);
    }

    public async Task<PrintTemplateDto?> PublishAsync(long templateId, long? templateVersionId, CancellationToken cancellationToken = default)
    {
        var template = await TemplateQuery(asNoTracking: false)
            .SingleOrDefaultAsync(item => item.RecId == templateId, cancellationToken);
        if (template == null) return null;
        if (template.Status == ReportTemplateStatus.Archived)
            throw new PrintTemplateValidationException(["Archived templates cannot be published."]);

        var version = templateVersionId.HasValue
            ? template.Versions.SingleOrDefault(item => item.RecId == templateVersionId.Value)
            : template.Versions.OrderByDescending(item => item.VersionNo).FirstOrDefault(item => !item.IsPublished);
        if (version == null) throw new PrintTemplateValidationException(["No draft template version is available to publish."]);
        if (version.IsPublished) throw new PrintTemplateValidationException(["Published template versions are immutable."]);

        var document = Deserialize(version.TemplateJson);
        var errors = (await ValidateDocumentForProcessAsync(template.RefRecId, document, cancellationToken)).ToList();
        ThrowIfInvalid(errors);

        version.IsPublished = true;
        version.PublishedBy = _currentUser.GetCurrentUserId();
        version.PublishedAt = DateTime.UtcNow;
        template.CurrentVersionId = version.RecId;
        template.Status = ReportTemplateStatus.Published;
        await _context.SaveChangesAsync(cancellationToken);
        return await GetAsync(templateId, cancellationToken);
    }

    public async Task<PrintTemplateDto?> ArchiveAsync(long templateId, CancellationToken cancellationToken = default)
    {
        var template = await _context.ReportTemplates.SingleOrDefaultAsync(item => item.RecId == templateId, cancellationToken);
        if (template == null) return null;
        template.Status = ReportTemplateStatus.Archived;
        template.IsDefault = false;
        template.IsActive = false;
        await _context.SaveChangesAsync(cancellationToken);
        return await GetAsync(templateId, cancellationToken);
    }

    public async Task<bool> DeleteDraftAsync(long templateId, CancellationToken cancellationToken = default)
    {
        var template = await TemplateQuery(asNoTracking: false)
            .SingleOrDefaultAsync(item => item.RecId == templateId, cancellationToken);
        if (template == null) return false;
        if (template.Versions.Any(item => item.IsPublished))
            throw new PrintTemplateValidationException(["A template with published versions cannot be deleted; archive it instead."]);
        _context.ReportTemplateVersions.RemoveRange(template.Versions);
        _context.ReportTemplates.Remove(template);
        await _context.SaveChangesAsync(cancellationToken);
        return true;
    }

    public async Task<PrintTemplateValidationResultDto?> ValidateAsync(long templateId, CancellationToken cancellationToken = default)
    {
        var template = await TemplateQuery(asNoTracking: true)
            .SingleOrDefaultAsync(item => item.RecId == templateId, cancellationToken);
        if (template == null) return null;
        var editable = EditableVersion(template);
        var document = Deserialize(editable.TemplateJson);
        return new PrintTemplateValidationResultDto
        {
            Errors = (await ValidateDocumentForProcessAsync(template.RefRecId, document, cancellationToken)).ToList()
        };
    }

    private async Task<IReadOnlyList<string>> ValidateDocumentForProcessAsync(long processId, PrintTemplateDocument document, CancellationToken cancellationToken)
    {
        var errors = _documentValidator.Validate(document).ToList();
        var requestedControls = _documentValidator.RequestControlIds(document);
        if (requestedControls.Count > 0)
        {
            var valid = await _context.WfRequestControls.AsNoTracking()
                .Where(item => item.ProcessId == processId && requestedControls.Contains(item.RecId))
                .Select(item => item.RecId).ToListAsync(cancellationToken);
            errors.AddRange(requestedControls.Except(valid).Select(id => $"RequestControlId '{id}' is missing from this process."));
        }
        var requestedSteps = _documentValidator.WorkflowStepIds(document);
        if (requestedSteps.Count > 0)
        {
            var valid = await _context.WfSteps.AsNoTracking()
                .Where(item => item.ProcessId == processId && requestedSteps.Contains(item.RecId))
                .Select(item => item.RecId).ToListAsync(cancellationToken);
            errors.AddRange(requestedSteps.Except(valid).Select(id => $"Workflow StepId '{id}' is missing from this process."));
        }
        return errors;
    }

    private async Task<string> ProcessNameAsync(long processId, CancellationToken cancellationToken) =>
        await _context.WfProcesses.AsNoTracking()
            .Where(item => item.RecId == processId)
            .Select(item => item.Name ?? item.Code ?? string.Empty)
            .SingleOrDefaultAsync(cancellationToken) ?? string.Empty;

    private IQueryable<ReportTemplate> TemplateQuery(bool asNoTracking)
    {
        IQueryable<ReportTemplate> query = _context.ReportTemplates
            .Include(item => item.CurrentVersion)
            .Include(item => item.Versions);
        return asNoTracking ? query.AsNoTracking() : query;
    }

    private async Task<bool> CodeExistsAsync(int refTableId, long refRecId, string code, long? exceptId, CancellationToken cancellationToken) =>
        await _context.ReportTemplates.AsNoTracking().AnyAsync(
            item => item.RefTableId == refTableId && item.RefRecId == refRecId && item.Code == code.Trim() && (!exceptId.HasValue || item.RecId != exceptId.Value),
            cancellationToken);

    private async Task ClearOtherDefaultsAsync(int refTableId, long refRecId, long? exceptId, CancellationToken cancellationToken)
    {
        var defaults = await _context.ReportTemplates
            .Where(item => item.RefTableId == refTableId && item.RefRecId == refRecId && item.IsDefault && (!exceptId.HasValue || item.RecId != exceptId.Value))
            .ToListAsync(cancellationToken);
        foreach (var item in defaults) item.IsDefault = false;
    }

    private async Task ExecuteInTransactionAsync(Func<Task> operation, CancellationToken cancellationToken)
    {
        var strategy = _context.Database.CreateExecutionStrategy();
        await strategy.ExecuteAsync(async () =>
        {
            await using var transaction = await _context.Database.BeginTransactionAsync(cancellationToken);
            await operation();
            await transaction.CommitAsync(cancellationToken);
        });
    }

    private static ReportTemplateVersion EditableVersion(ReportTemplate template) =>
        template.Versions.OrderByDescending(item => !item.IsPublished).ThenByDescending(item => item.VersionNo).First();

    private static PrintTemplateDto Map(ReportTemplate template)
    {
        var editable = EditableVersion(template);
        return new PrintTemplateDto
        {
            TemplateId = template.RecId,
            RefTableId = template.RefTableId,
            RefRecId = template.RefRecId,
            ProcessId = template.RefRecId,
            Code = template.Code ?? string.Empty,
            Name = template.Name ?? string.Empty,
            NameAlias = template.NameAlias,
            Description = template.Description,
            PageSize = template.PageSize,
            Orientation = template.Orientation,
            Language = template.Language,
            IsDefault = template.IsDefault,
            Status = template.Status,
            CurrentVersionId = template.CurrentVersionId,
            CurrentVersionNo = template.CurrentVersion?.VersionNo,
            LatestVersionNo = template.Versions.Max(item => item.VersionNo),
            HasDraft = template.Versions.Any(item => !item.IsPublished),
            IsActive = template.IsActive,
            LastModifiedAt = template.LastModifiedAt,
            EditableVersionId = editable.RecId,
            EditableVersionNo = editable.VersionNo,
            EditableVersionPublished = editable.IsPublished,
            Document = Deserialize(editable.TemplateJson),
            Versions = template.Versions.OrderByDescending(item => item.VersionNo).Select(item => new PrintTemplateVersionDto
            {
                TemplateVersionId = item.RecId,
                VersionNo = item.VersionNo,
                IsPublished = item.IsPublished,
                PublishedBy = item.PublishedBy,
                PublishedAt = item.PublishedAt,
                CreatedAt = item.CreatedAt
            }).ToList()
        };
    }

    private static string Serialize(PrintTemplateDocument document) => JsonSerializer.Serialize(document, JsonOptions);

    private static PrintTemplateDocument Deserialize(string json)
    {
        try
        {
            return JsonSerializer.Deserialize<PrintTemplateDocument>(json, JsonOptions)
                ?? throw new JsonException("The template document is empty.");
        }
        catch (JsonException exception)
        {
            throw new PrintTemplateValidationException([$"Template JSON is invalid: {exception.Message}"]);
        }
    }

    private static void ThrowIfInvalid(IEnumerable<string> errors)
    {
        var list = errors.Distinct().ToList();
        if (list.Count > 0) throw new PrintTemplateValidationException(list);
    }
}
