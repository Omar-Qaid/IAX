using IAX.IXApi.Modules.Workflow.PrintTemplates;
using IAX.IXApi.Modules.Workflow.Requests;

namespace IAX.IXApi.Infrastructure.Persistence.Seeding.Chunks;

public sealed partial class WfProcessSeedData
{
    private sealed record FormOptionSeed(string Value, string Name);

    private sealed record FormControlSeed(
        byte ControlId,
        string Code,
        string Name,
        string Description,
        bool Required = true,
        IReadOnlyList<FormOptionSeed>? Options = null);

    private sealed record FormStepSeed(
        string Code,
        string Name,
        string PerformerCode,
        string PerformerName);

    private sealed record FormSeedDefinition(
        string Code,
        string Name,
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
            Name: "براءة ذمة",
            Description: "نموذج براءة ذمة الموظف واعتماد إخلاء الطرف من الإدارات المعنية.",
            CategoryId: 1,
            PriorityId: 2,
            ProcessTypeId: 1,
            SortOrder: 2,
            Controls:
            [
                new(4, "CLEARANCE_DATE", "التاريخ", "تاريخ نموذج براءة الذمة"),
                new(2, "CLEARANCE_REFERENCE", "الرقم", "الرقم المرجعي للنموذج"),
                new(2, "CLEARANCE_EMPLOYEE_NAME", "اسم الموظف", "اسم الموظف الرباعي"),
                new(19, "CLEARANCE_EMPLOYEE_NUMBER", "الرقم الوظيفي", "الرقم الوظيفي للموظف"),
                new(2, "CLEARANCE_JOB_TITLE", "الوظيفة", "المسمى الوظيفي"),
                new(2, "CLEARANCE_DEPARTMENT", "الإدارة / القسم", "إدارة أو قسم الموظف"),
                new(4, "CLEARANCE_EMPLOYMENT_DATE", "تاريخ التعيين", "تاريخ بدء العمل"),
                new(4, "CLEARANCE_LAST_WORKING_DATE", "آخر يوم عمل", "آخر تاريخ عمل للموظف"),
                new(9, "CLEARANCE_APPROVALS", "اعتمادات براءة الذمة", "اعتمادات الإدارات والتوقيعات",
                    Options:
                    [
                        new("department", "الإدارة"),
                        new("employeeName", "الاسم"),
                        new("jobTitle", "الوظيفة"),
                        new("signature", "التوقيع"),
                        new("date", "التاريخ"),
                        new("notes", "ملاحظات"),
                    ]),
            ],
            Steps:
            [
                new("CLEARANCE_DIRECT_MANAGER", "اعتماد المدير المباشر", "CLEARANCE_DIRECT_MANAGER", "المدير المباشر - براءة ذمة"),
                new("CLEARANCE_HR", "اعتماد الموارد البشرية", "CLEARANCE_HR", "الموارد البشرية - براءة ذمة"),
                new("CLEARANCE_ADMIN", "اعتماد المدير الإداري", "CLEARANCE_ADMIN", "المدير الإداري - براءة ذمة"),
                new("CLEARANCE_PERSONNEL", "اعتماد شؤون الموظفين", "CLEARANCE_PERSONNEL", "شؤون الموظفين - براءة ذمة"),
                new("CLEARANCE_FINANCE", "اعتماد الإدارة المالية", "CLEARANCE_FINANCE", "الإدارة المالية - براءة ذمة"),
                new("CLEARANCE_WAREHOUSE", "اعتماد المستودع", "CLEARANCE_WAREHOUSE", "المستودع - براءة ذمة"),
                new("CLEARANCE_SALES", "اعتماد إدارة المبيعات", "CLEARANCE_SALES", "إدارة المبيعات - براءة ذمة"),
                new("CLEARANCE_SECURITY", "اعتماد الأمن الصناعي", "CLEARANCE_SECURITY", "الأمن الصناعي - براءة ذمة"),
                new("CLEARANCE_SHOWROOMS", "اعتماد المعارض", "CLEARANCE_SHOWROOMS", "المعارض - براءة ذمة"),
            ],
            PrintTemplateCode: "EMPLOYEE_CLEARANCE_AR",
            BuildPrintTemplate: BuildClearancePrintTemplate),

        new FormSeedDefinition(
            Code: "RENTAL_ASSISTANCE_KEY_HANDOVER",
            Name: "اتفاقية تسليم المفاتيح للمساعدة الإيجارية وتوزيع الشبكات",
            Description: "توثيق تسليم مفاتيح وحدة سكنية بين مالك العقار أو ممثله والمستفيد أو فريق الدعم.",
            CategoryId: 11,
            PriorityId: 2,
            ProcessTypeId: 1,
            SortOrder: 3,
            Controls:
            [
                new(4, "KEY_AGREEMENT_DATE", "التاريخ", "تاريخ اتفاقية تسليم المفاتيح"),
                new(2, "KEY_BENEFICIARY", "المستفيد", "اسم المستفيد من برنامج الدعم"),
                new(3, "KEY_ADDRESS", "العنوان", "عنوان المستفيد"),
                new(2, "KEY_CASE_NUMBER", "رقم الحالة / الفريق", "رقم الحالة أو الفريق"),
                new(3, "KEY_PROPERTY_ADDRESS", "عنوان موقع التسليم", "عنوان الوحدة السكنية"),
                new(2, "KEY_CITY", "المدينة", "مدينة الوحدة السكنية"),
                new(2, "KEY_STATE", "الولاية / المنطقة", "الولاية أو المنطقة"),
                new(2, "KEY_POSTAL_CODE", "الرمز البريدي", "الرمز البريدي"),
                new(2, "KEY_DELIVERER_NAME", "اسم مقدم المفاتيح", "اسم الشخص الذي سلم المفاتيح"),
                new(2, "KEY_DELIVERER_PHONE", "رقم هاتف مقدم المفاتيح", "رقم الهاتف"),
                new(20, "KEY_DELIVERER_SIGNATURE", "توقيع مقدم المفاتيح", "توقيع مقدم المفاتيح"),
                new(4, "KEY_DELIVERER_DATE", "تاريخ التسليم", "تاريخ تسليم المفاتيح"),
                new(23, "KEY_DELIVERER_ROLE", "صفة مقدم المفاتيح", "صفة مقدم المفاتيح", Options:
                [
                    new("propertyOwner", "مالك العقار"),
                    new("broker", "الوسيط"),
                    new("authorizedAgent", "الوكيل المعتمد لمالك العقار"),
                ]),
                new(2, "KEY_RECIPIENT_NAME", "اسم مستلم المفاتيح", "اسم مستلم المفاتيح"),
                new(2, "KEY_RECIPIENT_PHONE", "رقم هاتف المستلم", "رقم هاتف مستلم المفاتيح"),
                new(20, "KEY_RECIPIENT_SIGNATURE", "توقيع المستلم", "توقيع مستلم المفاتيح"),
                new(4, "KEY_RECIPIENT_DATE", "تاريخ الاستلام", "تاريخ استلام المفاتيح"),
                new(23, "KEY_RECIPIENT_ROLE", "صفة مستلم المفاتيح", "صفة مستلم المفاتيح", Options:
                [
                    new("registeredPerson", "المسجل"),
                    new("dhsStaff", "طاقم إدارة الدعم"),
                    new("housingOpportunityTeam", "فريق فرصة السكن"),
                ]),
                new(2, "KEY_DHS_EMPLOYEE_NAME", "اسم موظف إدارة الدعم", "يعبأ عند استلام موظف إدارة الدعم", false),
                new(2, "KEY_DHS_JOB_TITLE", "المسمى الوظيفي", "المسمى الوظيفي لموظف إدارة الدعم", false),
            ],
            Steps:
            [
                new("KEY_HANDOVER_REVIEW", "مراجعة اتفاقية تسليم المفاتيح", "KEY_HANDOVER_REVIEWER", "مراجع اتفاقية تسليم المفاتيح"),
            ],
            PrintTemplateCode: "RENTAL_KEY_HANDOVER_AR",
            BuildPrintTemplate: BuildKeyHandoverPrintTemplate),
    ];
}

