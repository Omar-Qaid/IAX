namespace IAX.IXApi.Modules.Workflow.PrintTemplates;

public sealed class PrintTemplateDocumentValidator
{
    private static readonly HashSet<string> Directions = ["ltr", "rtl"];
    private static readonly HashSet<string> Languages = ["en", "ar"];
    private static readonly HashSet<string> PageSizes = ["A4", "Letter"];
    private static readonly HashSet<string> Orientations = ["portrait", "landscape"];
    private static readonly HashSet<string> MissingBehaviors = ["empty", "na", "placeholder"];
    private static readonly HashSet<string> Operators = ["=", "!=", ">", ">=", "<", "<=", "contains", "notContains", "isEmpty", "isNotEmpty", "in", "notIn"];
    private static readonly HashSet<string> SourceTypes = ["system", "company", "report", "requestControl", "workflow", "attachment", "repeating"];
    private static readonly HashSet<string> FormatTypes = ["text", "date", "dateTime", "number", "currency", "percentage", "boolean"];
    private static readonly HashSet<string> NegativeFormats = ["minus", "parentheses", "trailingMinus"];
    private static readonly HashSet<string> DatePatterns = ["dd/MM/yyyy", "dd-MM-yyyy", "MM/dd/yyyy", "dd MMM yyyy", "yyyy-MM-dd", "HH:mm"];

    public IReadOnlyList<string> Validate(PrintTemplateDocument? document)
    {
        var errors = new List<string>();
        if (document == null) return ["Template document is required."];
        if (document.SchemaVersion != 1) errors.Add($"Unsupported schema version '{document.SchemaVersion}'.");
        if (!Languages.Contains(document.Language)) errors.Add("Language must be 'en' or 'ar'.");
        if (!Directions.Contains(document.Direction)) errors.Add("Direction must be 'ltr' or 'rtl'.");
        if (!PageSizes.Contains(document.Page.Size)) errors.Add("Page size must be A4 or Letter.");
        if (!Orientations.Contains(document.Page.Orientation)) errors.Add("Orientation must be portrait or landscape.");
        if (!MissingBehaviors.Contains(document.MissingFieldBehavior)) errors.Add("Missing-field behavior is invalid.");
        if (new[] { document.Page.Margins.Top, document.Page.Margins.Right, document.Page.Margins.Bottom, document.Page.Margins.Left }.Any(value => value < 0 || value > 50))
            errors.Add("Page margins must be between 0 and 50 millimetres.");

        var ids = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        ValidateElements(document.Header.Concat(document.Sections).Concat(document.Footer), ids, errors, "document");
        return errors;
    }

    public IReadOnlySet<long> RequestControlIds(PrintTemplateDocument document) =>
        Bindings(document).Where(item => item.SourceType == "requestControl" && item.RequestControlId.HasValue)
            .Select(item => item.RequestControlId!.Value).ToHashSet();

    public IReadOnlySet<long> WorkflowStepIds(PrintTemplateDocument document) =>
        Elements(document).OfType<PrintWorkflowApprovalElement>().Select(item => item.StepId)
            .Concat(Bindings(document).Where(item => item.StepId.HasValue).Select(item => item.StepId!.Value))
            .Where(item => item > 0).ToHashSet();

    private static void ValidateElements(IEnumerable<PrintTemplateElement> elements, HashSet<string> ids, List<string> errors, string path)
    {
        var index = 0;
        foreach (var element in elements)
        {
            var elementPath = $"{path}[{index++}]";
            if (string.IsNullOrWhiteSpace(element.Id)) errors.Add($"{elementPath}: element id is required.");
            else if (!ids.Add(element.Id)) errors.Add($"{elementPath}: duplicate element id '{element.Id}'.");
            ValidateCondition(element.VisibleWhen, errors, elementPath);

            switch (element)
            {
                case PrintTextElement text when string.IsNullOrWhiteSpace(text.Value):
                    errors.Add($"{elementPath}: text value is required.");
                    break;
                case PrintFieldElement field:
                    ValidateBinding(field.Binding, errors, elementPath);
                    ValidateFormat(field.Format, errors, elementPath);
                    break;
                case PrintSectionElement section:
                    if (section.Columns is < 1 or > 12) errors.Add($"{elementPath}: section columns must be between 1 and 12.");
                    ValidateElements(section.Elements, ids, errors, $"{elementPath}.elements");
                    break;
                case PrintRowElement row:
                    ValidateElements(row.Elements, ids, errors, $"{elementPath}.elements");
                    break;
                case PrintColumnElement column:
                    if (column.Span is < 1 or > 12) errors.Add($"{elementPath}: column span must be between 1 and 12.");
                    ValidateElements(column.Elements, ids, errors, $"{elementPath}.elements");
                    break;
                case PrintImageElement image when image.Binding != null:
                    ValidateBinding(image.Binding, errors, elementPath);
                    break;
                case PrintTableElement table:
                    ValidateBinding(table.DataSource, errors, elementPath);
                    if (table.Columns.Count == 0) errors.Add($"{elementPath}: table requires at least one column.");
                    if (table.Columns.Any(column => string.IsNullOrWhiteSpace(column.Id) || string.IsNullOrWhiteSpace(column.Field)))
                        errors.Add($"{elementPath}: every table column requires id and field.");
                    foreach (var column in table.Columns)
                        ValidateFormat(column.Format, errors, $"{elementPath}.columns[{column.Id}]");
                    break;
                case PrintWorkflowApprovalElement approval when approval.StepId <= 0:
                    errors.Add($"{elementPath}: workflow step id is required.");
                    break;
                case PrintSignatureElement signature:
                    ValidateBinding(signature.Binding, errors, elementPath);
                    break;
                case PrintQrCodeElement qr:
                    ValidateBinding(qr.Binding, errors, elementPath);
                    break;
                case PrintBarcodeElement barcode:
                    ValidateBinding(barcode.Binding, errors, elementPath);
                    break;
                case PrintAttachmentElement attachment when attachment.Binding != null:
                    ValidateBinding(attachment.Binding, errors, elementPath);
                    break;
                case PrintSpacerElement spacer when spacer.Height is < 0 or > 100:
                    errors.Add($"{elementPath}: spacer height must be between 0 and 100 millimetres.");
                    break;
            }
        }
    }

    private static void ValidateCondition(PrintVisibilityCondition? condition, List<string> errors, string path)
    {
        if (condition == null) return;
        ValidateBinding(condition.Field, errors, $"{path}.visibleWhen");
        if (!Operators.Contains(condition.Operator)) errors.Add($"{path}: unsupported condition operator '{condition.Operator}'.");
    }

    private static void ValidateBinding(PrintFieldBinding binding, List<string> errors, string path)
    {
        if (!SourceTypes.Contains(binding.SourceType))
        {
            errors.Add($"{path}: unsupported source type '{binding.SourceType}'.");
            return;
        }
        if (binding.SourceType == "requestControl" && binding.RequestControlId is not > 0)
            errors.Add($"{path}: requestControl binding requires a stable RequestControlId.");
        if (binding.SourceType == "workflow" && binding.StepId is not > 0 && string.IsNullOrWhiteSpace(binding.Source))
            errors.Add($"{path}: workflow binding requires StepId or source.");
        if (binding.SourceType is "system" or "company" or "report" or "repeating" && string.IsNullOrWhiteSpace(binding.Source))
            errors.Add($"{path}: {binding.SourceType} binding requires source.");
    }

    private static void ValidateFormat(PrintValueFormat? format, List<string> errors, string path)
    {
        if (format == null) return;
        if (!FormatTypes.Contains(format.Type)) errors.Add($"{path}: unsupported value format '{format.Type}'.");
        if (format.DecimalPlaces is < 0 or > 8) errors.Add($"{path}: decimal places must be between 0 and 8.");
        if (!string.IsNullOrWhiteSpace(format.NegativeFormat) && !NegativeFormats.Contains(format.NegativeFormat))
            errors.Add($"{path}: unsupported negative format '{format.NegativeFormat}'.");
        if (!string.IsNullOrWhiteSpace(format.Pattern) && !DatePatterns.Contains(format.Pattern))
            errors.Add($"{path}: unsupported date pattern '{format.Pattern}'.");
    }

    private static IEnumerable<PrintTemplateElement> Elements(PrintTemplateDocument document)
    {
        foreach (var element in document.Header.Concat(document.Sections).Concat(document.Footer))
        {
            yield return element;
            var children = element switch
            {
                PrintSectionElement section => section.Elements,
                PrintRowElement row => row.Elements,
                PrintColumnElement column => column.Elements,
                _ => []
            };
            foreach (var child in Descendants(children)) yield return child;
        }
    }

    private static IEnumerable<PrintTemplateElement> Descendants(IEnumerable<PrintTemplateElement> elements)
    {
        foreach (var element in elements)
        {
            yield return element;
            var children = element switch
            {
                PrintSectionElement section => section.Elements,
                PrintRowElement row => row.Elements,
                PrintColumnElement column => column.Elements,
                _ => []
            };
            foreach (var child in Descendants(children)) yield return child;
        }
    }

    private static IEnumerable<PrintFieldBinding> Bindings(PrintTemplateDocument document)
    {
        foreach (var element in Elements(document))
        {
            if (element.VisibleWhen != null) yield return element.VisibleWhen.Field;
            switch (element)
            {
                case PrintFieldElement field: yield return field.Binding; break;
                case PrintImageElement { Binding: not null } image: yield return image.Binding; break;
                case PrintTableElement table: yield return table.DataSource; break;
                case PrintSignatureElement signature: yield return signature.Binding; break;
                case PrintQrCodeElement qr: yield return qr.Binding; break;
                case PrintBarcodeElement barcode: yield return barcode.Binding; break;
                case PrintAttachmentElement { Binding: not null } attachment: yield return attachment.Binding; break;
            }
        }
    }
}
