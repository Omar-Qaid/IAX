using IAX.IXApi.Modules.Workflow.PrintTemplates;
using IAX.IXApi.Modules.Workflow.Requests;

namespace IAX.IXApi.Infrastructure.Persistence.Seeding.Chunks;

public sealed partial class WfProcessSeedData
{
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

