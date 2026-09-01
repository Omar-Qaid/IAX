using IAX.IXApi.Modules.Workflow.PrintTemplates;
using IAX.IXApi.Modules.Workflow.Requests;
using IAX.IXApi.Modules.Workflow.Steps;
using System.Text.Json;

namespace IAX.IXApi.Infrastructure.Persistence.Seeding.Chunks;

public sealed partial class WfProcessSeedData
{
    private static PrintTemplateDocument BuildPaymentRequestPrintTemplate(
        IReadOnlyDictionary<string, WfRequestControl> controls,
        IReadOnlyDictionary<string, WfStep> steps) => new()
    {
        Language = "ar",
        Direction = "rtl",
        Page = new PrintTemplatePage
        {
            Size = "A4",
            Orientation = "portrait",
            Margins = new PrintTemplateMargins { Top = 9, Right = 10, Bottom = 9, Left = 10 },
        },
        Header =
        [
            new PrintRowElement
            {
                Id = "payment-header",
                Style = new() { MarginBottom = 4, KeepTogether = true },
                Elements =
                [
                    new PrintColumnElement
                    {
                        Id = "payment-logo-column",
                        Span = 1,
                        Elements =
                        [
                            new PrintImageElement
                            {
                                Id = "payment-company-logo",
                                SourceType = "companyLogo",
                                AltText = "شعار الشركة",
                                Style = new() { Height = 58, ObjectFit = "contain", Alignment = "start" },
                            },
                        ],
                    },
                    new PrintColumnElement
                    {
                        Id = "payment-heading-column",
                        Span = 3,
                        Elements =
                        [
                            new PrintTextElement
                            {
                                Id = "payment-title",
                                Value = "طلب الصرف",
                                Style = new() { FontSize = 19, FontWeight = 700, Alignment = "center", MarginBottom = 2 },
                            },
                            new PrintTextElement
                            {
                                Id = "payment-type-cash",
                                Value = "☑ العهدة النقدية    ☐ الاتفاقيات والعقود والموردين",
                                VisibleWhen = RequestValueCondition(controls["PAYMENT_REQUEST_TYPE"], "CashAdvance"),
                                Style = new() { FontSize = 10, FontWeight = 700, Alignment = "center" },
                            },
                            new PrintTextElement
                            {
                                Id = "payment-type-contracts",
                                Value = "☐ العهدة النقدية    ☑ الاتفاقيات والعقود والموردين",
                                VisibleWhen = RequestValueCondition(controls["PAYMENT_REQUEST_TYPE"], "ContractsVendors"),
                                Style = new() { FontSize = 10, FontWeight = 700, Alignment = "center" },
                            },
                        ],
                    },
                ],
            },
        ],
        Sections =
        [
            new PrintSectionElement
            {
                Id = "payment-reference",
                Columns = 2,
                Style = new() { MarginBottom = 5, KeepTogether = true },
                Elements =
                [
                    RequestField("payment-date", "التاريخ", controls["PAYMENT_REQUEST_DATE"], "date"),
                    SystemField("payment-number", "الرقم", "requestNumber", "text", "#c62828"),
                ],
            },
            new PrintTextElement
            {
                Id = "payment-addressee",
                Value = "المكرم / مدير الإدارة المالية\nالسلام عليكم ورحمة الله وبركاته، وبعد\nنأمل التكرم بصرف المبلغ حسب البيانات التالية:",
                Style = new() { FontSize = 10, FontWeight = 600, Alignment = "start", MarginBottom = 5, Padding = 2 },
            },
            new PrintSectionElement
            {
                Id = "payment-origin",
                Columns = 2,
                Style = new() { MarginBottom = 6, KeepTogether = true },
                Elements =
                [
                    RequestField("payment-site", "الموقع / إدارة الشركة", controls["PAYMENT_SITE"]),
                    RequestField("payment-department", "القسم الإداري", controls["PAYMENT_DEPARTMENT"]),
                ],
            },
            new PrintTableElement
            {
                Id = "payment-details-table",
                DataSource = RequestBinding(controls["PAYMENT_DETAILS"]),
                RepeatHeader = true,
                Style = new() { MarginBottom = 2, BorderWidth = 1, BorderColor = "#555555", KeepTogether = true, FontSize = 9 },
                Columns =
                [
                    new() { Id = "payment-col-sequence", Label = "م", Field = "sequence", Width = 5 },
                    new() { Id = "payment-col-beneficiary", Label = "اسم المستفيد", Field = "beneficiary", Width = 20 },
                    new() { Id = "payment-col-invoice-number", Label = "رقم الفاتورة", Field = "invoice_number", Width = 14 },
                    new() { Id = "payment-col-invoice-amount", Label = "قيمة الفاتورة", Field = "invoice_amount", Width = 14, Format = MoneyFormat() },
                    new() { Id = "payment-col-vat", Label = "الضريبة", Field = "vat", Width = 10, Format = MoneyFormat() },
                    new() { Id = "payment-col-total", Label = "الإجمالي", Field = "total", Width = 14, Format = MoneyFormat() },
                    new() { Id = "payment-col-statement", Label = "بيان التحويل / الصرف", Field = "payment_statement", Width = 23 },
                ],
            },
            new PrintFieldElement
            {
                Id = "payment-grand-total",
                Label = "الإجمالي",
                Binding = RequestBinding(controls["PAYMENT_GRAND_TOTAL"]),
                Format = MoneyFormat(),
                Style = new()
                {
                    MarginBottom = 8,
                    BorderWidth = 1,
                    BorderColor = "#555555",
                    Color = "#c62828",
                    BackgroundColor = "#eeeeee",
                    FontWeight = 700,
                    KeepTogether = true,
                },
            },
            new PrintTextElement
            {
                Id = "payment-audit-label",
                Value = "مسؤول المتابعة والتدقيق",
                Style = new() { FontSize = 10, FontWeight = 700, Alignment = "start", MarginBottom = 10 },
            },
            new PrintDividerElement
            {
                Id = "payment-audit-signature-line",
                Style = new() { MarginBottom = 12, BorderColor = "#555555" },
            },
            new PrintRowElement
            {
                Id = "payment-requester-auditor-row",
                Style = new() { MarginBottom = 12, KeepTogether = true },
                Elements =
                [
                    PaymentApprovalCell("payment-requester", "الموظف", "submittedBy"),
                    PaymentApprovalCell("payment-auditor", steps["PAYMENT_AUDIT_STEP"].NameAlias ?? "مسؤول المتابعة والتدقيق"),
                ],
            },
            new PrintRowElement
            {
                Id = "payment-finance-approvals-row",
                Style = new() { MarginBottom = 14, KeepTogether = true },
                Elements =
                [
                    PaymentApprovalCell("payment-finance-manager", steps["PAYMENT_FINANCE_MANAGER_STEP"].NameAlias ?? "مدير الإدارة المالية"),
                    PaymentApprovalCell("payment-accounting-review", steps["PAYMENT_ACCOUNTING_REVIEW_STEP"].NameAlias ?? "مدير أول المحاسبة والمراجعة (المكلف)"),
                    PaymentApprovalCell("payment-accountant", steps["PAYMENT_ACCOUNTANT_STEP"].NameAlias ?? "المحاسب"),
                ],
            },
            new PrintDividerElement
            {
                Id = "payment-final-approval-divider",
                Style = new() { MarginBottom = 10, BorderColor = "#555555" },
            },
            new PrintRowElement
            {
                Id = "payment-final-approvals-row",
                Style = new() { KeepTogether = true },
                Elements =
                [
                    PaymentApprovalCell("payment-ceo", $"يعتمد\n{steps["PAYMENT_CEO_STEP"].NameAlias ?? "الرئيس التنفيذي"}"),
                    PaymentApprovalCell("payment-executive-director", steps["PAYMENT_EXECUTIVE_DIRECTOR_STEP"].NameAlias ?? "المدير التنفيذي للموارد البشرية والخدمات المساندة"),
                ],
            },
        ],
        Footer =
        [
            new PrintDividerElement { Id = "payment-footer-divider" },
            new PrintRowElement
            {
                Id = "payment-footer",
                Elements =
                [
                    new PrintColumnElement
                    {
                        Id = "payment-footer-code-column",
                        Elements =
                        [
                            new PrintTextElement { Id = "payment-footer-code", Value = "FIN-PAYMENT-REQUEST", Style = new() { FontSize = 7, Alignment = "start" } },
                        ],
                    },
                    new PrintColumnElement
                    {
                        Id = "payment-footer-page-column",
                        Elements =
                        [
                            new PrintPageNumberElement { Id = "payment-page-number", Style = new() { FontSize = 7, Alignment = "end" } },
                        ],
                    },
                ],
            },
        ],
        MissingFieldBehavior = "empty",
    };

    private static PrintTemplateDocument BuildClearancePrintTemplate(
        IReadOnlyDictionary<string, WfRequestControl> controls) => new()
    {
        Language = "ar",
        Direction = "rtl",
        Page = A4Page(),
        Header =
        [
            new PrintImageElement { Id = "clearance-logo", SourceType = "companyLogo", AltText = "شعار الجهة", Style = new() { Height = 42, ObjectFit = "contain", Alignment = "start" } },
            new PrintTextElement { Id = "clearance-title", Value = "براءة ذمة", Style = TitleStyle() },
        ],
        Sections =
        [
            new PrintSectionElement
            {
                Id = "clearance-reference",
                Columns = 2,
                Elements =
                [
                    RequestField("clearance-date", "التاريخ", controls["CLEARANCE_DATE"], "date"),
                    RequestField("clearance-number", "الرقم", controls["CLEARANCE_REFERENCE"]),
                ],
            },
            new PrintSectionElement
            {
                Id = "clearance-employee",
                Title = "بيانات الموظف",
                Columns = 2,
                Elements =
                [
                    RequestField("clearance-employee-name", "اسم الموظف", controls["CLEARANCE_EMPLOYEE_NAME"]),
                    RequestField("clearance-employee-number", "الرقم الوظيفي", controls["CLEARANCE_EMPLOYEE_NUMBER"]),
                    RequestField("clearance-job", "الوظيفة", controls["CLEARANCE_JOB_TITLE"]),
                    RequestField("clearance-department", "الإدارة / القسم", controls["CLEARANCE_DEPARTMENT"]),
                    RequestField("clearance-employment-date", "تاريخ التعيين", controls["CLEARANCE_EMPLOYMENT_DATE"], "date"),
                    RequestField("clearance-last-date", "آخر يوم عمل", controls["CLEARANCE_LAST_WORKING_DATE"], "date"),
                ],
            },
            new PrintTableElement
            {
                Id = "clearance-approvals-table",
                DataSource = RequestBinding(controls["CLEARANCE_APPROVALS"]),
                RepeatHeader = true,
                Style = new() { MarginBottom = 8, BorderWidth = 1, BorderColor = "#222222", KeepTogether = true },
                Columns =
                [
                    new() { Id = "clearance-col-department", Label = "الإدارة", Field = "department", Width = 28 },
                    new() { Id = "clearance-col-name", Label = "الاسم", Field = "employeeName", Width = 18 },
                    new() { Id = "clearance-col-job", Label = "الوظيفة", Field = "jobTitle", Width = 16 },
                    new() { Id = "clearance-col-signature", Label = "التوقيع", Field = "signature", Width = 14 },
                    new() { Id = "clearance-col-date", Label = "التاريخ", Field = "date", Width = 12 },
                    new() { Id = "clearance-col-notes", Label = "ملاحظات", Field = "notes", Width = 12 },
                ],
            },
        ],
        Footer =
        [
            new PrintDividerElement { Id = "clearance-footer-divider" },
            new PrintTextElement { Id = "clearance-footer-code", Value = "HR-F-002 - Rev. (01)", Style = new() { FontSize = 8, Alignment = "end" } },
        ],
    };

    private static PrintTemplateDocument BuildKeyHandoverPrintTemplate(
        IReadOnlyDictionary<string, WfRequestControl> controls) => new()
    {
        Language = "ar",
        Direction = "rtl",
        Page = A4Page(),
        Header =
        [
            new PrintTextElement { Id = "keys-title", Value = "اتفاقية تسليم المفاتيح للمساعدة الإيجارية وتوزيع الشبكات", Style = TitleStyle() },
        ],
        Sections =
        [
            new PrintSectionElement
            {
                Id = "keys-case-information",
                Columns = 2,
                Elements =
                [
                    RequestField("keys-date", "التاريخ", controls["KEY_AGREEMENT_DATE"], "date"),
                    RequestField("keys-beneficiary", "المستفيد", controls["KEY_BENEFICIARY"]),
                    RequestField("keys-address", "العنوان", controls["KEY_ADDRESS"]),
                    RequestField("keys-case-number", "رقم الحالة / الفريق", controls["KEY_CASE_NUMBER"]),
                ],
            },
            new PrintTextElement { Id = "keys-agreement-copy", Value = "توثق هذه الاتفاقية تسليم المفاتيح للوحدة السكنية المحددة أدناه.", Style = new() { FontSize = 11, Alignment = "center", MarginBottom = 6 } },
            new PrintSectionElement
            {
                Id = "keys-location",
                Title = "موقع التسليم الأول",
                Columns = 3,
                Elements =
                [
                    RequestField("keys-property-address", "العنوان", controls["KEY_PROPERTY_ADDRESS"]),
                    RequestField("keys-city", "المدينة", controls["KEY_CITY"]),
                    RequestField("keys-state", "الولاية / المنطقة", controls["KEY_STATE"]),
                    RequestField("keys-postal", "الرمز البريدي", controls["KEY_POSTAL_CODE"]),
                ],
            },
            PartySection(
                "keys-deliverer",
                "مقدم بواسطة",
                controls["KEY_DELIVERER_NAME"],
                controls["KEY_DELIVERER_PHONE"],
                controls["KEY_DELIVERER_SIGNATURE"],
                controls["KEY_DELIVERER_DATE"],
                controls["KEY_DELIVERER_ROLE"]),
            PartySection(
                "keys-recipient",
                "مقدم إلى",
                controls["KEY_RECIPIENT_NAME"],
                controls["KEY_RECIPIENT_PHONE"],
                controls["KEY_RECIPIENT_SIGNATURE"],
                controls["KEY_RECIPIENT_DATE"],
                controls["KEY_RECIPIENT_ROLE"]),
            new PrintSectionElement
            {
                Id = "keys-dhs-employee",
                Title = "في حالة موظف إدارة الدعم",
                Columns = 2,
                Elements =
                [
                    RequestField("keys-dhs-name", "اسم الموظف", controls["KEY_DHS_EMPLOYEE_NAME"]),
                    RequestField("keys-dhs-title", "المسمى الوظيفي", controls["KEY_DHS_JOB_TITLE"]),
                ],
            },
        ],
        Footer =
        [
            new PrintDividerElement { Id = "keys-footer-divider" },
            new PrintDateElement { Id = "keys-print-date", Style = new() { FontSize = 8, Alignment = "end" } },
        ],
    };

    private static PrintSectionElement PartySection(
        string id,
        string title,
        WfRequestControl name,
        WfRequestControl phone,
        WfRequestControl signature,
        WfRequestControl date,
        WfRequestControl role) => new()
    {
        Id = id,
        Title = title,
        Columns = 2,
        Elements =
        [
            RequestField($"{id}-name", "الاسم (بأحرف واضحة)", name),
            RequestField($"{id}-phone", "رقم الهاتف", phone),
            new PrintSignatureElement { Id = $"{id}-signature", Label = "التوقيع", Binding = RequestBinding(signature), Style = new() { KeepTogether = true } },
            RequestField($"{id}-date", "التاريخ", date, "date"),
            RequestField($"{id}-role", "الصفة", role),
        ],
    };

    private static PrintColumnElement PaymentApprovalCell(string id, string title, string? systemSource = null)
    {
        List<PrintTemplateElement> elements =
        [
            new PrintTextElement
            {
                Id = $"{id}-title",
                Value = title,
                Style = new() { FontSize = 9, FontWeight = 700, Alignment = "center", MarginBottom = 9 },
            },
        ];
        if (systemSource is not null)
            elements.Add(SystemField($"{id}-name", "", systemSource, "text", null));
        elements.Add(new PrintDividerElement { Id = $"{id}-line", Style = new() { BorderColor = "#555555" } });
        return new PrintColumnElement
        {
            Id = id,
            Span = 1,
            Style = new() { Padding = 3, KeepTogether = true },
            Elements = elements,
        };
    }

    private static PrintFieldElement SystemField(
        string id,
        string label,
        string source,
        string format,
        string? color) => new()
    {
        Id = id,
        Label = label,
        Binding = new PrintFieldBinding { SourceType = "system", Source = source },
        Format = new PrintValueFormat { Type = format },
        Style = new() { MarginBottom = 4, BorderColor = "#777777", Color = color, KeepTogether = true },
    };

    private static PrintVisibilityCondition RequestValueCondition(WfRequestControl control, string value) => new()
    {
        Field = RequestBinding(control),
        Operator = "=",
        Value = JsonSerializer.SerializeToElement(value),
    };

    private static PrintValueFormat MoneyFormat() => new()
    {
        Type = "currency",
        Currency = "SAR",
        DecimalPlaces = 2,
        UseGrouping = true,
    };

    private static PrintFieldElement RequestField(
        string id,
        string label,
        WfRequestControl control,
        string format = "text") => new()
    {
        Id = id,
        Label = label,
        Binding = RequestBinding(control),
        Format = new PrintValueFormat { Type = format },
        Style = new() { MarginBottom = 4, BorderColor = "#777777", KeepTogether = true },
    };

    private static PrintFieldBinding RequestBinding(WfRequestControl control) => new()
    {
        SourceType = "requestControl",
        RequestControlId = control.RecId,
        ControlId = control.ControlId,
    };

    private static PrintTemplatePage A4Page() => new()
    {
        Size = "A4",
        Orientation = "portrait",
        Margins = new PrintTemplateMargins { Top = 12, Right = 12, Bottom = 12, Left = 12 },
    };

    private static PrintElementStyle TitleStyle() => new()
    {
        FontSize = 18,
        FontWeight = 700,
        Alignment = "center",
        MarginBottom = 10,
        BorderWidth = 1,
        BorderColor = "#333333",
        Padding = 6,
        KeepTogether = true,
    };
}
