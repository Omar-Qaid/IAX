using System.Text.Json;
using IAX.IXApi.Modules.Workflow.PrintTemplates;
using Xunit;

namespace IAX.IXApi.Tests;

/// <summary>
/// The payment request's print layout, as the workflow baseline seeds it, keeps the approved Arabic structure and
/// binds to the request controls seeded alongside it.
/// </summary>
public sealed class PaymentRequestPrintTemplateTests
{
    [Fact]
    public void Payment_request_template_matches_the_approved_arabic_structure()
    {
        using var stream = typeof(IAX.IXApi.Infrastructure.Persistence.SeedingERPV2.Core.Models.BaselineManifest).Assembly
            .GetManifestResourceStream("SeedingERPV2.Baseline.baseline-workflow.json")!;
        var manifest = JsonDocument.Parse(stream).RootElement;
        JsonElement[] Rows(string table) => manifest.GetProperty("allocated").EnumerateArray()
            .Single(x => x.GetProperty("table").GetString() == table).GetProperty("rows").EnumerateArray().ToArray();

        var process = Rows("WfProcesses").Single(x => x.GetProperty("Code").GetString() == "PAYMENT_REQUEST").GetProperty("RecId").GetInt64();
        var controls = Rows("WfRequestControls").Where(x => x.GetProperty("ProcessId").GetInt64() == process)
            .ToDictionary(x => x.GetProperty("Code").GetString()!, x => x.GetProperty("RecId").GetInt64());
        var template = Rows("ReportTemplates").Single(x => x.GetProperty("RefRecId").GetInt64() == process).GetProperty("RecId").GetInt64();
        var json = Rows("ReportTemplateVersions").Single(x => x.GetProperty("TemplateId").GetInt64() == template).GetProperty("TemplateJson").GetString()!;
        var document = JsonSerializer.Deserialize<PrintTemplateDocument>(json, new JsonSerializerOptions(JsonSerializerDefaults.Web))!;

        Assert.Equal("ar", document.Language);
        Assert.Equal("rtl", document.Direction);
        Assert.Equal("A4", document.Page.Size);
        Assert.Contains(document.Header.SelectMany(Descendants), element => element is PrintImageElement { SourceType: "companyLogo" });
        Assert.Contains(document.Header.SelectMany(Descendants), element => element is PrintTextElement { ValueAlias: "طلب الصرف" });

        var elements = document.Sections.SelectMany(Descendants).ToList();
        var table = Assert.Single(elements.OfType<PrintTableElement>());
        Assert.Equal(controls["PAYMENT_DETAILS"], table.DataSource.RequestControlId);
        Assert.Equal(["sequence", "beneficiary", "invoice_number", "invoice_amount", "vat", "total", "payment_statement"],
            table.Columns.Select(column => column.Field));
        Assert.Contains(elements.OfType<PrintFieldElement>(), field => field.Binding.RequestControlId == controls["PAYMENT_GRAND_TOTAL"]);
        Assert.Contains(elements, element => element.Id == "payment-finance-approvals-row");
        Assert.Contains(elements, element => element.Id == "payment-final-approvals-row");
        Assert.Empty(new PrintTemplateDocumentValidator().Validate(document));
        var all = document.Header.Concat(document.Sections).Concat(document.Footer).SelectMany(Descendants).ToList();
        Assert.All(all.OfType<PrintTextElement>(), text =>
        {
            Assert.False(string.IsNullOrWhiteSpace(text.ValueAlias));
            Assert.Equal(text.ValueAlias, text.TextAlias);
        });
        Assert.All(table.Columns, column => Assert.False(string.IsNullOrWhiteSpace(column.LabelAlias)));
    }

    private static IEnumerable<PrintTemplateElement> Descendants(PrintTemplateElement element)
    {
        yield return element;
        var children = element switch
        {
            PrintSectionElement section => section.Elements,
            PrintRowElement row => row.Elements,
            PrintColumnElement column => column.Elements,
            _ => [],
        };
        foreach (var child in children.SelectMany(Descendants))
            yield return child;
    }
}
