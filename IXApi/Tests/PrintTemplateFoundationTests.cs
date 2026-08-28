using IAX.IXApi.Modules.Workflow.PrintTemplates;
using System.Text.Json;
using Xunit;

namespace IXApi.Tests;

public sealed class PrintTemplateFoundationTests
{
    private readonly PrintTemplateDocumentValidator _validator = new();

    [Fact]
    public void ValidatesTypedRequestAndWorkflowBindings()
    {
        var document = new PrintTemplateDocument
        {
            Language = "ar",
            Direction = "rtl",
            Sections =
            [
                new PrintSectionElement
                {
                    Id = "request",
                    Columns = 2,
                    Elements =
                    [
                        new PrintFieldElement
                        {
                            Id = "employee",
                            Binding = new PrintFieldBinding
                            {
                                SourceType = "requestControl",
                                RequestControlId = 201,
                                ControlId = 4
                            }
                        },
                        new PrintWorkflowApprovalElement { Id = "finance", StepId = 25 }
                    ]
                }
            ]
        };

        Assert.Empty(_validator.Validate(document));
        Assert.Equal([201L], _validator.RequestControlIds(document));
        Assert.Equal([25L], _validator.WorkflowStepIds(document));
    }

    [Fact]
    public void RejectsUnsafeOrAmbiguousTemplateMetadata()
    {
        var document = new PrintTemplateDocument
        {
            SchemaVersion = 2,
            Sections =
            [
                new PrintFieldElement
                {
                    Id = "duplicate",
                    Binding = new PrintFieldBinding { SourceType = "requestControl" },
                    VisibleWhen = new PrintVisibilityCondition { Operator = "javascript" }
                },
                new PrintTextElement { Id = "duplicate", Value = "" }
            ]
        };

        var errors = _validator.Validate(document);

        Assert.Contains(errors, error => error.Contains("Unsupported schema version"));
        Assert.Contains(errors, error => error.Contains("stable RequestControlId"));
        Assert.Contains(errors, error => error.Contains("unsupported condition operator"));
        Assert.Contains(errors, error => error.Contains("duplicate element id"));
    }

    [Fact]
    public void PolymorphicTemplateJsonRoundTripsWithoutUntypedPayloads()
    {
        var document = new PrintTemplateDocument
        {
            Header =
            [
                new PrintTextElement
                {
                    Id = "title",
                    Value = "Payment request",
                    Style = new PrintElementStyle
                    {
                        Height = 120,
                        Padding = 6,
                        MarginBottom = 10,
                        BorderWidth = 2,
                        BorderColor = "#174f82",
                        BorderRadius = 8,
                        ObjectFit = "cover"
                    }
                }
            ],
            Footer = [new PrintPageNumberElement { Id = "page" }]
        };
        var options = new JsonSerializerOptions(JsonSerializerDefaults.Web);

        var json = JsonSerializer.Serialize(document, options);
        var roundTrip = JsonSerializer.Deserialize<PrintTemplateDocument>(json, options);

        Assert.IsType<PrintTextElement>(Assert.Single(roundTrip!.Header));
        var title = Assert.IsType<PrintTextElement>(Assert.Single(roundTrip.Header));
        Assert.Equal(120m, title.Style!.Height);
        Assert.Equal("cover", title.Style.ObjectFit);
        Assert.IsType<PrintPageNumberElement>(Assert.Single(roundTrip.Footer));
        Assert.Contains("\"type\":\"text\"", json);
    }

    [Fact]
    public void SupportsReportFieldsAndAdvancedValueFormatting()
    {
        var document = new PrintTemplateDocument
        {
            Footer =
            [
                new PrintFieldElement
                {
                    Id = "pageCount",
                    Binding = new PrintFieldBinding { SourceType = "report", Source = "pageNumberOfTotal" }
                },
                new PrintFieldElement
                {
                    Id = "amount",
                    Binding = new PrintFieldBinding { SourceType = "system", Source = "amount" },
                    Format = new PrintValueFormat
                    {
                        Type = "currency",
                        Currency = "SAR",
                        DecimalPlaces = 3,
                        UseGrouping = true,
                        NegativeFormat = "parentheses"
                    }
                },
                new PrintFieldElement
                {
                    Id = "printedDate",
                    Binding = new PrintFieldBinding { SourceType = "report", Source = "printedDate" },
                    Format = new PrintValueFormat { Type = "date", Pattern = "yyyy-MM-dd" }
                }
            ]
        };

        Assert.Empty(_validator.Validate(document));

        var options = new JsonSerializerOptions(JsonSerializerDefaults.Web);
        var json = JsonSerializer.Serialize(document, options);
        var roundTrip = JsonSerializer.Deserialize<PrintTemplateDocument>(json, options)!;
        var amount = Assert.IsType<PrintFieldElement>(roundTrip.Footer[1]);

        Assert.Equal(3, amount.Format!.DecimalPlaces);
        Assert.True(amount.Format.UseGrouping);
        Assert.Equal("parentheses", amount.Format.NegativeFormat);
    }
}
