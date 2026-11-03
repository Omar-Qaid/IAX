using IAX.IXApi.Modules.Workflow.PrintTemplates;
using IAX.IXApi.Modules.Workflow.Requests;

namespace IAX.IXApi.Infrastructure.Persistence.Seeding.Chunks;

public sealed partial class WfProcessSeedData
{
    private sealed record FormOptionSeed(string Value, string Name, string NameAlias);

    private sealed record FormControlSeed(
        byte ControlId,
        string Code,
        string Name,
        string NameAlias,
        string Description,
        bool Required = true,
        IReadOnlyList<FormOptionSeed>? Options = null);

    private sealed record FormStepSeed(
        string Code,
        string Name,
        string NameAlias,
        string PerformerCode,
        string PerformerName);

    private sealed record FormSeedDefinition(
        string Code,
        string Name,
        string NameAlias,
        string Description,
        short CategoryId,
        byte PriorityId,
        byte ProcessTypeId,
        byte SortOrder,
        IReadOnlyList<FormControlSeed> Controls,
        IReadOnlyList<FormStepSeed> Steps,
        string PrintTemplateCode,
        Func<IReadOnlyDictionary<string, WfRequestControl>, PrintTemplateDocument> BuildPrintTemplate);

    private static IReadOnlyList<FormSeedDefinition> AdditionalForms() =>
    [
        new FormSeedDefinition(
            Code: "EMPLOYEE_CLEARANCE",
            Name: "Employee Clearance",
            NameAlias: "براءة ذمة",
            Description: "Employee clearance form and departmental release approvals.",
            CategoryId: 1,
            PriorityId: 2,
            ProcessTypeId: 1,
            SortOrder: 2,
            Controls:
            [
                new(4, "CLEARANCE_DATE", "Date", "التاريخ", "Employee clearance form date"),
                new(2, "CLEARANCE_REFERENCE", "Reference number", "الرقم", "Form reference number"),
                new(2, "CLEARANCE_EMPLOYEE_NAME", "Employee name", "اسم الموظف", "Employee full name"),
                new(19, "CLEARANCE_EMPLOYEE_NUMBER", "Employee number", "الرقم الوظيفي", "Employee number"),
                new(2, "CLEARANCE_JOB_TITLE", "Job title", "الوظيفة", "Employee job title"),
                new(2, "CLEARANCE_DEPARTMENT", "Department / Section", "الإدارة / القسم", "Employee department or section"),
                new(4, "CLEARANCE_EMPLOYMENT_DATE", "Employment date", "تاريخ التعيين", "Employment start date"),
                new(4, "CLEARANCE_LAST_WORKING_DATE", "Last working day", "آخر يوم عمل", "Employee last working day"),
                new(9, "CLEARANCE_APPROVALS", "Clearance approvals", "اعتمادات براءة الذمة", "Department approvals and signatures",
                    Options:
                    [
                        new("department", "Department", "الإدارة"),
                        new("employeeName", "Name", "الاسم"),
                        new("jobTitle", "Job title", "الوظيفة"),
                        new("signature", "Signature", "التوقيع"),
                        new("date", "Date", "التاريخ"),
                        new("notes", "Notes", "ملاحظات"),
                    ]),
            ],
            Steps:
            [
                new("CLEARANCE_DIRECT_MANAGER", "Direct Manager Approval", "اعتماد المدير المباشر", "CLEARANCE_DIRECT_MANAGER", "Employee Clearance Direct Manager"),
                new("CLEARANCE_HR", "Human Resources Approval", "اعتماد الموارد البشرية", "CLEARANCE_HR", "Employee Clearance Human Resources"),
                new("CLEARANCE_ADMIN", "Administrative Manager Approval", "اعتماد المدير الإداري", "CLEARANCE_ADMIN", "Employee Clearance Administrative Manager"),
                new("CLEARANCE_PERSONNEL", "Personnel Affairs Approval", "اعتماد شؤون الموظفين", "CLEARANCE_PERSONNEL", "Employee Clearance Personnel Affairs"),
                new("CLEARANCE_FINANCE", "Finance Department Approval", "اعتماد الإدارة المالية", "CLEARANCE_FINANCE", "Employee Clearance Finance Department"),
                new("CLEARANCE_WAREHOUSE", "Warehouse Approval", "اعتماد المستودع", "CLEARANCE_WAREHOUSE", "Employee Clearance Warehouse"),
                new("CLEARANCE_SALES", "Sales Department Approval", "اعتماد إدارة المبيعات", "CLEARANCE_SALES", "Employee Clearance Sales Department"),
                new("CLEARANCE_SECURITY", "Industrial Security Approval", "اعتماد الأمن الصناعي", "CLEARANCE_SECURITY", "Employee Clearance Industrial Security"),
                new("CLEARANCE_SHOWROOMS", "Showrooms Approval", "اعتماد المعارض", "CLEARANCE_SHOWROOMS", "Employee Clearance Showrooms"),
            ],
            PrintTemplateCode: "EMPLOYEE_CLEARANCE_AR",
            BuildPrintTemplate: BuildClearancePrintTemplate),

        new FormSeedDefinition(
            Code: "RENTAL_ASSISTANCE_KEY_HANDOVER",
            Name: "Rental Assistance Key Handover Agreement",
            NameAlias: "اتفاقية تسليم المفاتيح للمساعدة الإيجارية وتوزيع الشبكات",
            Description: "Documents the handover of residential-unit keys between the property owner or representative and the beneficiary or support team.",
            CategoryId: 11,
            PriorityId: 2,
            ProcessTypeId: 1,
            SortOrder: 3,
            Controls:
            [
                new(4, "KEY_AGREEMENT_DATE", "Agreement date", "التاريخ", "Key handover agreement date"),
                new(2, "KEY_BENEFICIARY", "Beneficiary", "المستفيد", "Support-program beneficiary name"),
                new(3, "KEY_ADDRESS", "Address", "العنوان", "Beneficiary address"),
                new(2, "KEY_CASE_NUMBER", "Case / Team number", "رقم الحالة / الفريق", "Case or team number"),
                new(3, "KEY_PROPERTY_ADDRESS", "Handover location address", "عنوان موقع التسليم", "Residential-unit address"),
                new(2, "KEY_CITY", "City", "المدينة", "Residential-unit city"),
                new(2, "KEY_STATE", "State / Region", "الولاية / المنطقة", "State or region"),
                new(2, "KEY_POSTAL_CODE", "Postal code", "الرمز البريدي", "Postal code"),
                new(2, "KEY_DELIVERER_NAME", "Key deliverer name", "اسم مقدم المفاتيح", "Name of the person delivering the keys"),
                new(2, "KEY_DELIVERER_PHONE", "Key deliverer phone", "رقم هاتف مقدم المفاتيح", "Phone number"),
                new(20, "KEY_DELIVERER_SIGNATURE", "Key deliverer signature", "توقيع مقدم المفاتيح", "Key deliverer signature"),
                new(4, "KEY_DELIVERER_DATE", "Handover date", "تاريخ التسليم", "Key handover date"),
                new(23, "KEY_DELIVERER_ROLE", "Key deliverer role", "صفة مقدم المفاتيح", "Key deliverer role", Options:
                [
                    new("propertyOwner", "Property owner", "مالك العقار"),
                    new("broker", "Broker", "الوسيط"),
                    new("authorizedAgent", "Property owner's authorized agent", "الوكيل المعتمد لمالك العقار"),
                ]),
                new(2, "KEY_RECIPIENT_NAME", "Key recipient name", "اسم مستلم المفاتيح", "Key recipient name"),
                new(2, "KEY_RECIPIENT_PHONE", "Recipient phone", "رقم هاتف المستلم", "Key recipient phone number"),
                new(20, "KEY_RECIPIENT_SIGNATURE", "Recipient signature", "توقيع المستلم", "Key recipient signature"),
                new(4, "KEY_RECIPIENT_DATE", "Receipt date", "تاريخ الاستلام", "Key receipt date"),
                new(23, "KEY_RECIPIENT_ROLE", "Key recipient role", "صفة مستلم المفاتيح", "Key recipient role", Options:
                [
                    new("registeredPerson", "Registered person", "المسجل"),
                    new("dhsStaff", "Support department staff", "طاقم إدارة الدعم"),
                    new("housingOpportunityTeam", "Housing opportunity team", "فريق فرصة السكن"),
                ]),
                new(2, "KEY_DHS_EMPLOYEE_NAME", "Support employee name", "اسم موظف إدارة الدعم", "Completed when a support employee receives the keys", false),
                new(2, "KEY_DHS_JOB_TITLE", "Job title", "المسمى الوظيفي", "Support employee job title", false),
            ],
            Steps:
            [
                new("KEY_HANDOVER_REVIEW", "Key Handover Agreement Review", "مراجعة اتفاقية تسليم المفاتيح", "KEY_HANDOVER_REVIEWER", "Key Handover Agreement Reviewer"),
            ],
            PrintTemplateCode: "RENTAL_KEY_HANDOVER_AR",
            BuildPrintTemplate: BuildKeyHandoverPrintTemplate),
    ];
}
