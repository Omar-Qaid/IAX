using System.Text.Json;
using IAX.IXApi.Infrastructure.Persistence;
using IAX.IXApi.Modules.Workflow.PrintTemplates;
using IAX.IXApi.Shared.Domain.Reporting;
using Microsoft.EntityFrameworkCore;

namespace IAX.IXApi.Infrastructure.Persistence.Seeding.Processes.Process651;

public sealed partial class WfProcess651SeedData
{
    private static async Task SeedPrintTemplateAsync(ApplicationDbContext db, string owner, CancellationToken ct)
    {
        const long processId = 651L;
        const string processName = "Return or reverse a payment for the online store";
        const string processNameAlias = "ارجاع أو عكس عملية دفع للمتجر الإلكتروني";
        var templateCode = $"PROCESS_{processId}_PRINTOUT";

        var template = await db.ReportTemplates
            .IgnoreQueryFilters()
            .SingleOrDefaultAsync(
                row => row.RefTableId == WorkflowReportResourceAuthorizer.ProcessTableId && row.RefRecId == processId && row.Code == templateCode,
                ct);

        if (template is null)
        {
            var processHasDefaultTemplate = await db.ReportTemplates
                .IgnoreQueryFilters()
                .AnyAsync(
                    row => row.RefTableId == WorkflowReportResourceAuthorizer.ProcessTableId
                        && row.RefRecId == processId
                        && row.IsDefault
                        && row.IsActive
                        && !row.IsDeleted,
                    ct);

            template = new ReportTemplate
            {
                RefTableId = WorkflowReportResourceAuthorizer.ProcessTableId,
                RefRecId = processId,
                Code = templateCode,
                Name = $"{processName} printout",
                NameAlias = $"طباعة {processNameAlias}",
                Description = $"Seeded A4 printout for the {processName} workflow.",
                PageSize = "A4",
                Orientation = "portrait",
                Language = "en",
                IsDefault = !processHasDefaultTemplate,
                Status = ReportTemplateStatus.Published,
                IsActive = true,
                CreatedBy = owner,
                OwnerAccountId = owner,
            };
            db.ReportTemplates.Add(template);
            await db.SaveChangesAsync(ct);
        }

        template.Name = $"{processName} printout";
        template.NameAlias = $"طباعة {processNameAlias}";
        template.Description = $"Seeded A4 printout for the {processName} workflow.";

        var templateJson = BuildGenericPrintTemplateJson(processName, processNameAlias);
        var versions = await db.ReportTemplateVersions
            .IgnoreQueryFilters()
            .Where(row => row.TemplateId == template.RecId)
            .OrderByDescending(row => row.VersionNo)
            .ToListAsync(ct);
        var version = versions.FirstOrDefault(row => row.TemplateJson == templateJson);

        if (version is null)
        {
            version = new ReportTemplateVersion
            {
                TemplateId = template.RecId,
                VersionNo = (versions.FirstOrDefault()?.VersionNo ?? 0) + 1,
                TemplateJson = templateJson,
                IsPublished = true,
                PublishedBy = owner,
                PublishedAt = DateTime.UtcNow,
                CreatedBy = owner,
                OwnerAccountId = owner,
            };
            db.ReportTemplateVersions.Add(version);
            await db.SaveChangesAsync(ct);
        }
        else if (!version.IsPublished || version.IsDeleted)
        {
            version.IsPublished = true;
            version.IsDeleted = false;
            version.PublishedBy = owner;
            version.PublishedAt ??= DateTime.UtcNow;
            await db.SaveChangesAsync(ct);
        }

        if (template.CurrentVersionId != version.RecId
            || template.Status != ReportTemplateStatus.Published
            || !template.IsActive
            || template.IsDeleted)
        {
            template.CurrentVersionId = version.RecId;
            template.Status = ReportTemplateStatus.Published;
            template.IsActive = true;
            template.IsDeleted = false;
            await db.SaveChangesAsync(ct);
        }

        // For the seeded request 192663
        var requestId = 192663L;
        var requestVersion = await db.ReportEntityVersions
            .IgnoreQueryFilters()
            .SingleOrDefaultAsync(
                row => row.RefTableId == WorkflowReportResourceAuthorizer.RequestTableId && row.RefRecId == requestId && row.TemplateId == template.RecId,
                ct);

        if (requestVersion is null)
        {
            db.ReportEntityVersions.Add(new ReportEntityVersion
            {
                RefTableId = WorkflowReportResourceAuthorizer.RequestTableId,
                RefRecId = requestId,
                TemplateId = template.RecId,
                TemplateVersionId = version.RecId,
                SelectedAt = DateTime.UtcNow,
                SelectedBy = owner,
                CreatedBy = owner,
                OwnerAccountId = owner,
            });
            await db.SaveChangesAsync(ct);
        }
        else if (requestVersion.IsDeleted)
        {
            requestVersion.IsDeleted = false;
            await db.SaveChangesAsync(ct);
        }
    }

    private static string BuildGenericPrintTemplateJson(string processName, string processNameAlias)
    {
        var document = new PrintTemplateDocument
        {
            SchemaVersion = 1,
            Language = "en",
            Direction = "ltr",
            Page = new PrintTemplatePage
            {
                Size = "A4",
                Orientation = "portrait",
                Margins = new PrintTemplateMargins { Top = 15, Right = 15, Bottom = 15, Left = 15 },
            },
            Header =
            [
                new PrintImageElement
                {
                    Id = "company-logo",
                    SourceType = "companyLogo",
                    AltText = "Company logo",
                },
                new PrintTextElement
                {
                    Id = "document-title",
                    Value = processName,
                    ValueAlias = processNameAlias,
                    TextAlias = processNameAlias,
                    Style = new PrintElementStyle { FontSize = 18, FontWeight = 700, Alignment = "center" },
                },
            ],
            Sections =
            [
                new PrintSectionElement
                {
                    Id = "request-information",
                    Title = "Request information",
                    TitleAlias = "بيانات الطلب",
                    Columns = 2,
                    Elements =
                    [
                        new PrintFieldElement
                        {
                            Id = "request-number",
                            Label = "Request",
                            LabelAlias = "الطلب",
                            Binding = new PrintFieldBinding { SourceType = "system", Source = "requestNumber" },
                        },
                        new PrintFieldElement
                        {
                            Id = "request-date",
                            Label = "Request date",
                            LabelAlias = "تاريخ الطلب",
                            Binding = new PrintFieldBinding { SourceType = "system", Source = "requestDate" },
                            Format = new PrintValueFormat { Type = "date", Pattern = "yyyy-MM-dd" },
                        },
                        new PrintFieldElement
                        {
                            Id = "request-status",
                            Label = "Status",
                            LabelAlias = "الحالة",
                            Binding = new PrintFieldBinding { SourceType = "system", Source = "requestStatus" },
                        },
                        new PrintFieldElement
                        {
                            Id = "requested-by",
                            Label = "Requested by",
                            LabelAlias = "مقدم الطلب",
                            Binding = new PrintFieldBinding { SourceType = "system", Source = "submittedBy" },
                        },
                    ],
                },
            ],
            Footer =
            [
                new PrintDateElement { Id = "print-date" },
                new PrintPageNumberElement { Id = "page-number" },
            ],
            MissingFieldBehavior = "empty",
        };

        return JsonSerializer.Serialize(document, new JsonSerializerOptions(JsonSerializerDefaults.Web));
    }
}
