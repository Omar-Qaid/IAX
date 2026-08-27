using System.Text.Json.Serialization;
using System.Text.Json;

namespace IAX.IXApi.Modules.Workflow.PrintTemplates;

public sealed class PrintTemplateDocument
{
    public int SchemaVersion { get; set; } = 1;
    public string Language { get; set; } = "en";
    public string Direction { get; set; } = "ltr";
    public PrintTemplatePage Page { get; set; } = new();
    public List<PrintTemplateElement> Header { get; set; } = [];
    public List<PrintTemplateElement> Sections { get; set; } = [];
    public List<PrintTemplateElement> Footer { get; set; } = [];
    public string MissingFieldBehavior { get; set; } = "empty";
}

public sealed class PrintTemplatePage
{
    public string Size { get; set; } = "A4";
    public string Orientation { get; set; } = "portrait";
    public PrintTemplateMargins Margins { get; set; } = new();
}

public sealed class PrintTemplateMargins
{
    public decimal Top { get; set; } = 15;
    public decimal Right { get; set; } = 15;
    public decimal Bottom { get; set; } = 15;
    public decimal Left { get; set; } = 15;
}

[JsonPolymorphic(TypeDiscriminatorPropertyName = "type")]
[JsonDerivedType(typeof(PrintTextElement), "text")]
[JsonDerivedType(typeof(PrintFieldElement), "field")]
[JsonDerivedType(typeof(PrintSectionElement), "section")]
[JsonDerivedType(typeof(PrintRowElement), "row")]
[JsonDerivedType(typeof(PrintColumnElement), "column")]
[JsonDerivedType(typeof(PrintDividerElement), "divider")]
[JsonDerivedType(typeof(PrintImageElement), "image")]
[JsonDerivedType(typeof(PrintTableElement), "table")]
[JsonDerivedType(typeof(PrintWorkflowApprovalElement), "workflowApproval")]
[JsonDerivedType(typeof(PrintSignatureElement), "signature")]
[JsonDerivedType(typeof(PrintQrCodeElement), "qrCode")]
[JsonDerivedType(typeof(PrintBarcodeElement), "barcode")]
[JsonDerivedType(typeof(PrintAttachmentElement), "attachment")]
[JsonDerivedType(typeof(PrintPageNumberElement), "pageNumber")]
[JsonDerivedType(typeof(PrintDateElement), "printDate")]
[JsonDerivedType(typeof(PrintSpacerElement), "spacer")]
[JsonDerivedType(typeof(PrintPageBreakElement), "pageBreak")]
public abstract class PrintTemplateElement
{
    public string Id { get; set; } = Guid.NewGuid().ToString("N");
    public PrintVisibilityCondition? VisibleWhen { get; set; }
    public PrintElementStyle? Style { get; set; }
}

public sealed class PrintTextElement : PrintTemplateElement
{
    public string Value { get; set; } = string.Empty;
}

public sealed class PrintFieldElement : PrintTemplateElement
{
    public string Label { get; set; } = string.Empty;
    public PrintFieldBinding Binding { get; set; } = new();
    public PrintValueFormat? Format { get; set; }
    public string? Fallback { get; set; }
}

public sealed class PrintSectionElement : PrintTemplateElement
{
    public string? Title { get; set; }
    public int Columns { get; set; } = 1;
    public List<PrintTemplateElement> Elements { get; set; } = [];
}

public sealed class PrintRowElement : PrintTemplateElement
{
    public List<PrintTemplateElement> Elements { get; set; } = [];
}

public sealed class PrintColumnElement : PrintTemplateElement
{
    public int Span { get; set; } = 1;
    public List<PrintTemplateElement> Elements { get; set; } = [];
}

public sealed class PrintDividerElement : PrintTemplateElement;

public sealed class PrintImageElement : PrintTemplateElement
{
    public string SourceType { get; set; } = "companyLogo";
    public PrintFieldBinding? Binding { get; set; }
    public string? Source { get; set; }
    public string? AltText { get; set; }
}

public sealed class PrintTableElement : PrintTemplateElement
{
    public PrintFieldBinding DataSource { get; set; } = new();
    public List<PrintTableColumn> Columns { get; set; } = [];
    public bool RepeatHeader { get; set; } = true;
}

public sealed class PrintTableColumn
{
    public string Id { get; set; } = Guid.NewGuid().ToString("N");
    public string Label { get; set; } = string.Empty;
    public string Field { get; set; } = string.Empty;
    public PrintValueFormat? Format { get; set; }
    public decimal? Width { get; set; }
}

public sealed class PrintWorkflowApprovalElement : PrintTemplateElement
{
    public long StepId { get; set; }
    public bool ShowName { get; set; } = true;
    public bool ShowJobTitle { get; set; } = true;
    public bool ShowStatus { get; set; } = true;
    public bool ShowDate { get; set; } = true;
    public bool ShowComment { get; set; }
    public bool ShowSignature { get; set; } = true;
}

public sealed class PrintSignatureElement : PrintTemplateElement
{
    public PrintFieldBinding Binding { get; set; } = new();
    public string? Label { get; set; }
}

public sealed class PrintQrCodeElement : PrintTemplateElement
{
    public PrintFieldBinding Binding { get; set; } = new() { SourceType = "system", Source = "requestNumber" };
}

public sealed class PrintBarcodeElement : PrintTemplateElement
{
    public PrintFieldBinding Binding { get; set; } = new();
    public string Format { get; set; } = "code128";
}

public sealed class PrintAttachmentElement : PrintTemplateElement
{
    public PrintFieldBinding? Binding { get; set; }
    public bool ImagesOnly { get; set; }
}

public sealed class PrintPageNumberElement : PrintTemplateElement;
public sealed class PrintDateElement : PrintTemplateElement;

public sealed class PrintSpacerElement : PrintTemplateElement
{
    public decimal Height { get; set; } = 4;
}

public sealed class PrintPageBreakElement : PrintTemplateElement;

public sealed class PrintFieldBinding
{
    public string SourceType { get; set; } = "system";
    public string? Source { get; set; }
    public long? RequestControlId { get; set; }
    public byte? ControlId { get; set; }
    public long? StepId { get; set; }
}

public sealed class PrintVisibilityCondition
{
    public PrintFieldBinding Field { get; set; } = new();
    public string Operator { get; set; } = "=";
    public JsonElement? Value { get; set; }
}

public sealed class PrintValueFormat
{
    public string Type { get; set; } = "text";
    public string? Pattern { get; set; }
    public string? Currency { get; set; }
    public string? TrueText { get; set; }
    public string? FalseText { get; set; }
}

public sealed class PrintElementStyle
{
    public decimal? Width { get; set; }
    public decimal? FontSize { get; set; }
    public int? FontWeight { get; set; }
    public string? Alignment { get; set; }
    public string? Color { get; set; }
    public string? BackgroundColor { get; set; }
    public bool KeepTogether { get; set; }
}
