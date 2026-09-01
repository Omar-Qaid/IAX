using System.Reflection;
using IAX.IXApi.Infrastructure.Persistence.Seeding.Chunks;
using IAX.IXApi.Modules.Workflow.PrintTemplates;
using IAX.IXApi.Modules.Workflow.Requests;
using IAX.IXApi.Modules.Workflow.Steps;
using Xunit;

namespace IAX.IXApi.Tests;

public sealed class PaymentRequestPrintTemplateTests
{
    [Fact]
    public void Payment_request_template_matches_the_approved_arabic_structure()
    {
        var controlCodes = new[]
        {
            "PAYMENT_REQUEST_TYPE",
            "PAYMENT_REQUEST_DATE",
            "PAYMENT_SITE",
            "PAYMENT_DEPARTMENT",
            "PAYMENT_DETAILS",
            "PAYMENT_GRAND_TOTAL",
        };
        var controls = controlCodes.Select((code, index) => new WfRequestControl
            {
                RecId = index + 1,
                Code = code,
                ControlId = (byte)(index + 1),
            })
            .ToDictionary(control => control.Code!, StringComparer.OrdinalIgnoreCase);

        var stepCodes = new[]
        {
            "PAYMENT_AUDIT_STEP",
            "PAYMENT_ACCOUNTANT_STEP",
            "PAYMENT_ACCOUNTING_REVIEW_STEP",
            "PAYMENT_FINANCE_MANAGER_STEP",
            "PAYMENT_EXECUTIVE_DIRECTOR_STEP",
            "PAYMENT_CEO_STEP",
        };
        var steps = stepCodes.Select((code, index) => new WfStep
            {
                RecId = index + 1,
                Code = code,
                NameAlias = code,
            })
            .ToDictionary(step => step.Code!, StringComparer.OrdinalIgnoreCase);

        var method = typeof(WfProcessSeedData).GetMethod(
            "BuildPaymentRequestPrintTemplate",
            BindingFlags.Static | BindingFlags.NonPublic);
        Assert.NotNull(method);
        var document = Assert.IsType<PrintTemplateDocument>(method.Invoke(null, [controls, steps]));

        Assert.Equal("ar", document.Language);
        Assert.Equal("rtl", document.Direction);
        Assert.Equal("A4", document.Page.Size);
        Assert.Contains(document.Header.SelectMany(Descendants), element =>
            element is PrintImageElement { SourceType: "companyLogo" });
        Assert.Contains(document.Header.SelectMany(Descendants), element =>
            element is PrintTextElement { Value: "طلب الصرف" });

        var elements = document.Sections.SelectMany(Descendants).ToList();
        var table = Assert.Single(elements.OfType<PrintTableElement>());
        Assert.Equal(controls["PAYMENT_DETAILS"].RecId, table.DataSource.RequestControlId);
        Assert.Equal(7, table.Columns.Count);
        Assert.Equal(
            ["sequence", "beneficiary", "invoice_number", "invoice_amount", "vat", "total", "payment_statement"],
            table.Columns.Select(column => column.Field));
        Assert.Contains(elements.OfType<PrintFieldElement>(), field =>
            field.Binding.RequestControlId == controls["PAYMENT_GRAND_TOTAL"].RecId);
        Assert.Contains(elements, element => element.Id == "payment-finance-approvals-row");
        Assert.Contains(elements, element => element.Id == "payment-final-approvals-row");
        Assert.Empty(new PrintTemplateDocumentValidator().Validate(document));
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
