using IAX.IXApi.Infrastructure.Persistence.SeedingERPV2.Core.Contracts;
using IAX.IXApi.Infrastructure.Persistence.SeedingERPV2.Core.Models;
using IAX.IXApi.Modules.Workflow.Controls;

namespace IAX.IXApi.Infrastructure.Persistence.SeedingERPV2.Modules.Workflow;

/// <summary>
/// The field types a request or activity form can be built from. Every saved form control stores the
/// identifier of its control type, so the identifiers are fixed.
/// </summary>
/// <remarks>
/// Identifier 19, the separate employee-number field, is seeded inactive: the employee search field
/// (identifier 12) is the single employee selector, and 19 stays in the catalog only so a form saved
/// against it still resolves. Rewriting those saved forms is a repair, not a seed, and stays with the
/// legacy seeder that owns it.
/// </remarks>
public sealed class WfControlSeeder : IDataSeeder
{
    public string Id => "ERPV2.WfControlSeeder";
    public IReadOnlyCollection<string> Dependencies => ["ERPV2.CompanyInfoSeeder"];

    public Task SeedAsync(SeedContext c, CancellationToken ct) => WorkflowReference.EnsureAsync(c,
    [
        new WfControl { RecId = 1, Code = "number", Name = "مربع رقمي", Description = "مربع رقمي", ControlType = "TextBox", SortOrder = 1, IsActive = true },
        new WfControl { RecId = 2, Code = "text", Name = "مربع نصي", Description = "مربع نصي", ControlType = "TextBox", SortOrder = 2, IsActive = true },
        new WfControl { RecId = 3, Code = "textarea", Name = "نص طويل", Description = "نص طويل", ControlType = "TextBox", SortOrder = 3, IsActive = true },
        new WfControl { RecId = 4, Code = "date", Name = "تاريخ", Description = "تاريخ", ControlType = "Calendar", SortOrder = 4, IsActive = true },
        new WfControl { RecId = 5, Code = "select", Name = "قائمة منسدلة (تعبأ من قاعدة البيانات)", Description = "قائمة منسدلة (تعبأ من قاعدة البيانات)", ControlType = "DropDownList", SortOrder = 5, IsActive = true },
        new WfControl { RecId = 6, Code = "select", Name = "قائمة منسدلة (تعبأ يدويا)", Description = "قائمة منسدلة (تعبأ يدويا)", ControlType = "DropDownList", SortOrder = 6, IsActive = true },
        new WfControl { RecId = 7, Code = "checkbox", Name = "مربع إختيار", Description = "مربع إختيار", ControlType = "CheckBox", SortOrder = 7, IsActive = true },
        new WfControl { RecId = 9, Code = "table", Name = "جدول", Description = "جدول", ControlType = "Table", SortOrder = 9, IsActive = true },
        new WfControl { RecId = 10, Code = "label", Name = "نص للقراءة فقط", Description = "نص للقراءة فقط", ControlType = "Label", SortOrder = 10, IsActive = true },
        new WfControl { RecId = 11, Code = "radio", Name = "قائمة اختيار فردي", Description = "قائمة اختيار فردي", ControlType = "RadioButtonList", SortOrder = 11, IsActive = true },
        new WfControl { RecId = 12, Code = "search", Name = "بحث في الموظفين", Description = "بحث في الموظفين", ControlType = "ComboBox", SortOrder = 12, IsActive = true },
        new WfControl { RecId = 13, Code = "time", Name = "وقت", Description = "وقت", ControlType = "TextBox", SortOrder = 13, IsActive = true },
        new WfControl { RecId = 16, Code = "url", Name = "رابط", Description = "رابط", ControlType = "TextBox", SortOrder = 16, IsActive = true },
        new WfControl { RecId = 17, Code = "file", Name = "ملف", Description = "ملف", ControlType = "File", SortOrder = 17, IsActive = true },
        new WfControl { RecId = 18, Code = "showroom", Name = "معرض", Description = "معرض", ControlType = "Showroom", SortOrder = 18, IsActive = true },
        new WfControl { RecId = 19, Code = "EmployeeID", Name = "رقم وظيفي", Description = "رقم وظيفي", ControlType = "EmployeeID", SortOrder = 19, IsActive = false },
        new WfControl { RecId = 20, Code = "Signature", Name = "توقيع", Description = "توقيع", ControlType = "Signature", SortOrder = 20, IsActive = true },
        new WfControl { RecId = 21, Code = "Location", Name = "الموقع", Description = "الموقع", ControlType = "Location", SortOrder = 21, IsActive = true },
        new WfControl { RecId = 22, Code = "Advertiser", Name = "معلن", Description = "معلن", ControlType = "Advertiser", SortOrder = 22, IsActive = true },
        new WfControl { RecId = 23, Code = "CheckBoxList", Name = "قائمة اختيار متعددة", Description = "قائمة اختيار متعددة", ControlType = "CheckBoxList", SortOrder = 23, IsActive = true }
    ], ct);
}
