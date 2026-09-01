using IAX.IXApi.Infrastructure.Persistence;
using IAX.IXApi.Modules.Workflow.Activities;
using IAX.IXApi.Modules.Workflow.Categories;
using IAX.IXApi.Modules.Workflow.Controls;
using IAX.IXApi.Modules.Workflow.Operators;
using IAX.IXApi.Modules.Workflow.Performers;
using IAX.IXApi.Modules.Workflow.Priorities;
using IAX.IXApi.Modules.Workflow.ProcessTypes;
using IAX.IXApi.Modules.Workflow.Variables;
using Microsoft.EntityFrameworkCore;

namespace IAX.IXApi.Infrastructure.Persistence.Seeding.Chunks;

public sealed partial class WfProcessSeedData
{
    private static async Task SeedMasterDataAsync(
        ApplicationDbContext db,
        string owner,
        CancellationToken ct)
    {
        await AddMissingCategoriesAsync(db, owner, ct);
        await AddMissingPrioritiesAsync(db, owner, ct);
        await AddMissingProcessTypesAsync(db, owner, ct);
        await AddMissingActivityTypesAsync(db, owner, ct);
        await AddMissingPerformerTypesAsync(db, owner, ct);
        await AddMissingPerformersAsync(db, owner, ct);
        await AddMissingOperatorsAsync(db, owner, ct);
        await AddMissingDataTypesAsync(db, owner, ct);
        await AddMissingControlsAsync(db, owner, ct);
    }

    private static async Task AddMissingCategoriesAsync(
        ApplicationDbContext db,
        string owner,
        CancellationToken ct)
    {
        var rows = new[]
        {
            new WfCategory { RecId = 1, Code = "CAT1", Name = "معاملات إدارة الموارد البشرية", SortOrder = 1, IsActive = true, CreatedBy = owner, OwnerAccountId = owner },
            new WfCategory { RecId = 2, Code = "CAT2", Name = "معاملات إدارة المبيعات", Description = "معاملات إدارة المبيعات", SortOrder = 2, IsActive = true, CreatedBy = owner, OwnerAccountId = owner },
            new WfCategory { RecId = 3, Code = "CAT3", Name = "معاملات إدارة العمل عن بعد", SortOrder = 3, IsActive = true, CreatedBy = owner, OwnerAccountId = owner },
            new WfCategory { RecId = 4, Code = "CAT4", Name = "إدارة تقنية المعلومات", SortOrder = 4, IsActive = true, CreatedBy = owner, OwnerAccountId = owner },
            new WfCategory { RecId = 6, Code = "CAT6", Name = "معاملات الإدارة المالية", Description = "معاملات الإدارة المالية", SortOrder = 6, IsActive = true, CreatedBy = owner, OwnerAccountId = owner },
            new WfCategory { RecId = 7, Code = "CAT7", Name = "معاملات إدارة الجودة", SortOrder = 7, IsActive = true, CreatedBy = owner, OwnerAccountId = owner },
            new WfCategory { RecId = 9, Code = "CAT9", Name = "معاملات إدارة الامتياز", SortOrder = 9, IsActive = true, CreatedBy = owner, OwnerAccountId = owner },
            new WfCategory { RecId = 10, Code = "CAT10", Name = "معاملات إدارة جرد الفروع", SortOrder = 10, IsActive = true, CreatedBy = owner, OwnerAccountId = owner },
            new WfCategory { RecId = 11, Code = "CAT11", Name = "معاملات إدارة الإستئجار", SortOrder = 11, IsActive = true, CreatedBy = owner, OwnerAccountId = owner },
            new WfCategory { RecId = 12, Code = "CAT12", Name = "معاملات إدارة الإنشاءات", Description = "معاملات إدارة الإنشاءات", SortOrder = 12, IsActive = true, CreatedBy = owner, OwnerAccountId = owner },
            new WfCategory { RecId = 13, Code = "CAT13", Name = "معاملات إدارة المشتريات", Description = "معاملات إدارة المشتريات", SortOrder = 13, IsActive = true, CreatedBy = owner, OwnerAccountId = owner },
            new WfCategory { RecId = 14, Code = "CAT14", Name = "معاملات إدراة المعارض", SortOrder = 14, IsActive = true, CreatedBy = owner, OwnerAccountId = owner },
            new WfCategory { RecId = 15, Code = "CAT15", Name = "معاملات إدارة التسويق", SortOrder = 15, IsActive = true, CreatedBy = owner, OwnerAccountId = owner },
            new WfCategory { RecId = 16, Code = "CAT16", Name = "معاملات خدمة العملاء", Description = "معاملات خدمة العملاء", SortOrder = 16, IsActive = true, CreatedBy = owner, OwnerAccountId = owner },
            new WfCategory { RecId = 17, Code = "CAT17", Name = "معاملات إدارة الشؤون القانونية", SortOrder = 17, IsActive = true, CreatedBy = owner, OwnerAccountId = owner },
            new WfCategory { RecId = 18, Code = "CAT18", Name = "معاملات اداره التشغيل والتصنيع", Description = "معامللت خاصة بادارة التصنيع", SortOrder = 18, IsActive = true, CreatedBy = owner, OwnerAccountId = owner },
            new WfCategory { RecId = 19, Code = "CAT19", Name = "الزيارات الإلكترونية", SortOrder = 19, IsActive = true, CreatedBy = owner, OwnerAccountId = owner },
            new WfCategory { RecId = 20, Code = "CAT20", Name = "التقييمات الإلكترونية", SortOrder = 20, IsActive = true, CreatedBy = owner, OwnerAccountId = owner },
            new WfCategory { RecId = 21, Code = "CAT21", Name = "معاملات إدارة نيش", SortOrder = 21, IsActive = true, CreatedBy = owner, OwnerAccountId = owner },
            new WfCategory { RecId = 22, Code = "CAT22", Name = "الإدارة العليا", SortOrder = 22, IsActive = true, CreatedBy = owner, OwnerAccountId = owner },
        };

        var existingIds = await db.WfCategories.IgnoreQueryFilters()
            .Select(x => x.RecId).ToListAsync(ct);
        var missing = rows.Where(x => !existingIds.Contains(x.RecId)).ToArray();
        if (missing.Length == 0)
            return;

        await db.WfCategories.AddRangeAsync(missing, ct);
        await SaveIdentityRowsAsync(db, "WfCategories", ct);
    }

    private static async Task AddMissingPrioritiesAsync(
        ApplicationDbContext db,
        string owner,
        CancellationToken ct)
    {
        var rows = new[]
        {
            new WfPriority { RecId = 1, Code = "LOW", Name = "Low", Description = "Low Priority", SortOrder = 1, IsActive = true, CreatedBy = owner, OwnerAccountId = owner },
            new WfPriority { RecId = 2, Code = "MED", Name = "Medium", Description = "Medium Priority", SortOrder = 2, IsActive = true, CreatedBy = owner, OwnerAccountId = owner },
            new WfPriority { RecId = 3, Code = "HIGH", Name = "High", Description = "High Priority", SortOrder = 3, IsActive = true, CreatedBy = owner, OwnerAccountId = owner },
        };

        var existingIds = await db.WfPriorities.IgnoreQueryFilters()
            .Select(x => x.RecId).ToListAsync(ct);
        var missing = rows.Where(x => !existingIds.Contains(x.RecId)).ToArray();
        if (missing.Length == 0)
            return;

        await db.WfPriorities.AddRangeAsync(missing, ct);
        await SaveIdentityRowsAsync(db, "WfPriorities", ct);
    }

    private static async Task AddMissingActivityTypesAsync(
        ApplicationDbContext db,
        string owner,
        CancellationToken ct)
    {
        var rows = new[]
        {
            new WfActivityType { RecId = 1, Code = "PARTIAL", Name = "مرحلة جزئية", Description = "مرحلة جزئية", SortOrder = 1, IsActive = true, CreatedBy = owner, OwnerAccountId = owner },
            // Legacy ActivityTypeId 0 is normalized to 2 because zero-key rows are sentinels.
            new WfActivityType { RecId = 2, Code = "NORMAL", Name = "مرحلة عادية", Description = "مرحلة عادية", SortOrder = 2, IsActive = true, CreatedBy = owner, OwnerAccountId = owner },
        };

        var existingIds = await db.WfActivityTypes.IgnoreQueryFilters()
            .Select(x => x.RecId).ToListAsync(ct);
        var missing = rows.Where(x => !existingIds.Contains(x.RecId)).ToArray();
        if (missing.Length == 0)
            return;

        await db.WfActivityTypes.AddRangeAsync(missing, ct);
        await SaveIdentityRowsAsync(db, "WfActivityTypes", ct);
    }

    private static async Task AddMissingPerformerTypesAsync(
        ApplicationDbContext db,
        string owner,
        CancellationToken ct)
    {
        var rows = new[]
        {
            new WfPerformerType { RecId = 1, Code = "RELATIONAL", Name = "Relational", SortOrder = 1, IsActive = true, CreatedBy = owner, OwnerAccountId = owner },
            new WfPerformerType { RecId = 3, Code = "LEGACY", Name = "Legacy Performer", SortOrder = 3, IsActive = true, CreatedBy = owner, OwnerAccountId = owner },
        };

        var existingIds = await db.WfPerformerTypes.IgnoreQueryFilters()
            .Select(x => x.RecId).ToListAsync(ct);
        var missing = rows.Where(x => !existingIds.Contains(x.RecId)).ToArray();
        if (missing.Length == 0)
            return;

        await db.WfPerformerTypes.AddRangeAsync(missing, ct);
        await SaveIdentityRowsAsync(db, "WfPerformerType", ct);
    }

    private static async Task AddMissingPerformersAsync(
        ApplicationDbContext db,
        string owner,
        CancellationToken ct)
    {
        var rows = new[]
        {
            new WfPerformer { RecId = 12, Code = "PERF12", Name = "مقدم الطلب", PerformerTypeId = 3, IsApplicant = true, IsEmployee = true, IsActive = true, CreatedBy = owner, OwnerAccountId = owner },
            new WfPerformer { RecId = 13, Code = "PERF13", Name = "المدير الاول للمقدم الطلب", PerformerTypeId = 3, IsManager1 = true, IsActive = true, CreatedBy = owner, OwnerAccountId = owner },
            new WfPerformer { RecId = 14, Code = "PERF14", Name = "المدير الثاني للمقدم الطلب", PerformerTypeId = 3, IsManager2 = true, IsActive = true, CreatedBy = owner, OwnerAccountId = owner },
            new WfPerformer { RecId = 15, Code = "PERF15", Name = "المدير الثالث للمقدم الطلب", PerformerTypeId = 3, IsManager3 = true, IsActive = true, CreatedBy = owner, OwnerAccountId = owner },
            new WfPerformer { RecId = 16, Code = "PERF16", Name = "المدير الثالث للمقدم الطلب", PerformerTypeId = 3, IsManager4 = true, IsActive = true, CreatedBy = owner, OwnerAccountId = owner },
        };

        var existingIds = await db.WfPerformers.IgnoreQueryFilters()
            .Select(x => x.RecId).ToListAsync(ct);
        var missing = rows.Where(x => !existingIds.Contains(x.RecId)).ToArray();
        if (missing.Length == 0)
            return;

        await db.WfPerformers.AddRangeAsync(missing, ct);
        await SaveIdentityRowsAsync(db, "WfPerformers", ct);
    }

    private static async Task AddMissingOperatorsAsync(
        ApplicationDbContext db,
        string owner,
        CancellationToken ct)
    {
        var rows = new[]
        {
            new WfOperator { RecId = 1, Code = "GT", Name = ">", Description = ">", SortOrder = 1, IsActive = true, CreatedBy = owner, OwnerAccountId = owner },
            new WfOperator { RecId = 2, Code = "LT", Name = "<", Description = "<", SortOrder = 2, IsActive = true, CreatedBy = owner, OwnerAccountId = owner },
            new WfOperator { RecId = 3, Code = "GTE", Name = ">=", Description = ">=", SortOrder = 3, IsActive = true, CreatedBy = owner, OwnerAccountId = owner },
            new WfOperator { RecId = 4, Code = "LTE", Name = "<=", Description = "<=", SortOrder = 4, IsActive = true, CreatedBy = owner, OwnerAccountId = owner },
            new WfOperator { RecId = 5, Code = "EQ", Name = "=", Description = "=", SortOrder = 5, IsActive = true, CreatedBy = owner, OwnerAccountId = owner },
            new WfOperator { RecId = 6, Code = "NEQ", Name = "<>", Description = "<>", SortOrder = 6, IsActive = true, CreatedBy = owner, OwnerAccountId = owner },
            new WfOperator { RecId = 7, Code = "BETWEEN", Name = "Between", Description = "Between", SortOrder = 7, IsActive = true, CreatedBy = owner, OwnerAccountId = owner },
        };

        var existingIds = await db.WfOperators.IgnoreQueryFilters()
            .Select(x => x.RecId).ToListAsync(ct);
        var missing = rows.Where(x => !existingIds.Contains(x.RecId)).ToArray();
        if (missing.Length == 0)
            return;

        await db.WfOperators.AddRangeAsync(missing, ct);
        await SaveIdentityRowsAsync(db, "WfOperators", ct);
    }

    private static async Task AddMissingDataTypesAsync(
        ApplicationDbContext db,
        string owner,
        CancellationToken ct)
    {
        var rows = new[]
        {
            new WfDataType { RecId = 1, Code = "INT", Name = "Integre", Description = "Integre", SortOrder = 1, IsActive = true, CreatedBy = owner, OwnerAccountId = owner },
            new WfDataType { RecId = 2, Code = "STR", Name = "String", Description = "String", SortOrder = 2, IsActive = true, CreatedBy = owner, OwnerAccountId = owner },
            new WfDataType { RecId = 3, Code = "DT", Name = "Date/Time", Description = "Date/Time", SortOrder = 3, IsActive = true, CreatedBy = owner, OwnerAccountId = owner },
            new WfDataType { RecId = 4, Code = "BOOL", Name = "True/False", Description = "True/False", SortOrder = 4, IsActive = true, CreatedBy = owner, OwnerAccountId = owner },
        };

        var existingIds = await db.WfDataTypes.IgnoreQueryFilters()
            .Select(x => x.RecId).ToListAsync(ct);
        var missing = rows.Where(x => !existingIds.Contains(x.RecId)).ToArray();
        if (missing.Length == 0)
            return;

        await db.WfDataTypes.AddRangeAsync(missing, ct);
        await SaveIdentityRowsAsync(db, "WfDataTypes", ct);
    }

    private static async Task AddMissingControlsAsync(
        ApplicationDbContext db,
        string owner,
        CancellationToken ct)
    {
        var rows = new[]
        {
            new WfControl { RecId = 1, Code = "number", Name = "مربع رقمي", Description = "مربع رقمي", ControlType = "TextBox", SortOrder = 1, IsActive = true, CreatedBy = owner, OwnerAccountId = owner },
            new WfControl { RecId = 2, Code = "text", Name = "مربع نصي", Description = "مربع نصي", ControlType = "TextBox", SortOrder = 2, IsActive = true, CreatedBy = owner, OwnerAccountId = owner },
            new WfControl { RecId = 3, Code = "textarea", Name = "نص طويل", Description = "نص طويل", ControlType = "TextBox", SortOrder = 3, IsActive = true, CreatedBy = owner, OwnerAccountId = owner },
            new WfControl { RecId = 4, Code = "date", Name = "تاريخ", Description = "تاريخ", ControlType = "Calendar", SortOrder = 4, IsActive = true, CreatedBy = owner, OwnerAccountId = owner },
            new WfControl { RecId = 5, Code = "select", Name = "قائمة منسدلة (تعبأ من قاعدة البيانات)", Description = "قائمة منسدلة (تعبأ من قاعدة البيانات)", ControlType = "DropDownList", SortOrder = 5, IsActive = true, CreatedBy = owner, OwnerAccountId = owner },
            new WfControl { RecId = 6, Code = "select", Name = "قائمة منسدلة (تعبأ يدويا)", Description = "قائمة منسدلة (تعبأ يدويا)", ControlType = "DropDownList", SortOrder = 6, IsActive = true, CreatedBy = owner, OwnerAccountId = owner },
            new WfControl { RecId = 7, Code = "checkbox", Name = "مربع إختيار", Description = "مربع إختيار", ControlType = "CheckBox", SortOrder = 7, IsActive = true, CreatedBy = owner, OwnerAccountId = owner },
            new WfControl { RecId = 9, Code = "table", Name = "جدول", Description = "جدول", ControlType = "Table", SortOrder = 9, IsActive = true, CreatedBy = owner, OwnerAccountId = owner },
            new WfControl { RecId = 10, Code = "label", Name = "نص للقراءة فقط", Description = "نص للقراءة فقط", ControlType = "Label", SortOrder = 10, IsActive = true, CreatedBy = owner, OwnerAccountId = owner },
            new WfControl { RecId = 11, Code = "radio", Name = "قائمة اختيار فردي", Description = "قائمة اختيار فردي", ControlType = "RadioButtonList", SortOrder = 11, IsActive = true, CreatedBy = owner, OwnerAccountId = owner },
            new WfControl { RecId = 12, Code = "search", Name = "بحث في الموظفين", Description = "بحث في الموظفين", ControlType = "ComboBox", SortOrder = 12, IsActive = true, CreatedBy = owner, OwnerAccountId = owner },
            new WfControl { RecId = 13, Code = "time", Name = "وقت", Description = "وقت", ControlType = "TextBox", SortOrder = 13, IsActive = true, CreatedBy = owner, OwnerAccountId = owner },
            new WfControl { RecId = 16, Code = "url", Name = "رابط", Description = "رابط", ControlType = "TextBox", SortOrder = 16, IsActive = true, CreatedBy = owner, OwnerAccountId = owner },
            new WfControl { RecId = 17, Code = "file", Name = "ملف", Description = "ملف", ControlType = "File", SortOrder = 17, IsActive = true, CreatedBy = owner, OwnerAccountId = owner },
            new WfControl { RecId = 18, Code = "showroom", Name = "معرض", Description = "معرض", ControlType = "Showroom", SortOrder = 18, IsActive = true, CreatedBy = owner, OwnerAccountId = owner },
            new WfControl { RecId = 19, Code = "EmployeeID", Name = "رقم وظيفي", Description = "رقم وظيفي", ControlType = "EmployeeID", SortOrder = 19, IsActive = true, CreatedBy = owner, OwnerAccountId = owner },
            new WfControl { RecId = 20, Code = "Signature", Name = "توقيع", Description = "توقيع", ControlType = "Signature", SortOrder = 20, IsActive = true, CreatedBy = owner, OwnerAccountId = owner },
            new WfControl { RecId = 21, Code = "Location", Name = "الموقع", Description = "الموقع", ControlType = "Location", SortOrder = 21, IsActive = true, CreatedBy = owner, OwnerAccountId = owner },
            new WfControl { RecId = 22, Code = "Advertiser", Name = "معلن", Description = "معلن", ControlType = "Advertiser", SortOrder = 22, IsActive = true, CreatedBy = owner, OwnerAccountId = owner },
            new WfControl { RecId = 23, Code = "CheckBoxList", Name = "قائمة اختيار متعددة", Description = "قائمة اختيار متعددة", ControlType = "CheckBoxList", SortOrder = 23, IsActive = true, CreatedBy = owner, OwnerAccountId = owner },
        };

        var existingIds = await db.WfControls.IgnoreQueryFilters()
            .Select(x => x.RecId).ToListAsync(ct);
        var missing = rows.Where(x => !existingIds.Contains(x.RecId)).ToArray();
        if (missing.Length == 0)
            return;

        await db.WfControls.AddRangeAsync(missing, ct);
        await SaveIdentityRowsAsync(db, "WfControls", ct);
    }

    private static async Task AddMissingProcessTypesAsync(
        ApplicationDbContext db,
        string owner,
        CancellationToken ct)
    {
        var rows = new[]
        {
            new WfProcessType { RecId = 1, Code = "STD", Name = "Standard", IsActive = true, CreatedBy = owner, OwnerAccountId = owner },
            new WfProcessType { RecId = 2, Code = "REV", Name = "Review", IsActive = true, CreatedBy = owner, OwnerAccountId = owner },
            new WfProcessType { RecId = 3, Code = "APP", Name = "Approval", IsActive = true, CreatedBy = owner, OwnerAccountId = owner },
        };

        var existingIds = await db.WfProcessTypes.IgnoreQueryFilters()
            .Select(x => x.RecId).ToListAsync(ct);
        var missing = rows.Where(x => !existingIds.Contains(x.RecId)).ToArray();
        if (missing.Length == 0)
            return;

        await db.WfProcessTypes.AddRangeAsync(missing, ct);
        await SaveIdentityRowsAsync(db, "WfProcessTypes", ct);
    }

    private static async Task SaveIdentityRowsAsync(
        ApplicationDbContext db,
        string tableName,
        CancellationToken ct)
    {
        var (enableIdentityInsert, disableIdentityInsert) = tableName switch
        {
            "WfCategories" => ("SET IDENTITY_INSERT [WfCategories] ON", "SET IDENTITY_INSERT [WfCategories] OFF"),
            "WfPriorities" => ("SET IDENTITY_INSERT [WfPriorities] ON", "SET IDENTITY_INSERT [WfPriorities] OFF"),
            "WfProcessTypes" => ("SET IDENTITY_INSERT [WfProcessTypes] ON", "SET IDENTITY_INSERT [WfProcessTypes] OFF"),
            "WfActivityTypes" => ("SET IDENTITY_INSERT [WfActivityTypes] ON", "SET IDENTITY_INSERT [WfActivityTypes] OFF"),
            "WfPerformerType" => ("SET IDENTITY_INSERT [WfPerformerType] ON", "SET IDENTITY_INSERT [WfPerformerType] OFF"),
            "WfPerformers" => ("SET IDENTITY_INSERT [WfPerformers] ON", "SET IDENTITY_INSERT [WfPerformers] OFF"),
            "WfOperators" => ("SET IDENTITY_INSERT [WfOperators] ON", "SET IDENTITY_INSERT [WfOperators] OFF"),
            "WfDataTypes" => ("SET IDENTITY_INSERT [WfDataTypes] ON", "SET IDENTITY_INSERT [WfDataTypes] OFF"),
            "WfControls" => ("SET IDENTITY_INSERT [WfControls] ON", "SET IDENTITY_INSERT [WfControls] OFF"),
            _ => throw new ArgumentOutOfRangeException(nameof(tableName), tableName, "Unsupported identity table."),
        };

        await db.Database.OpenConnectionAsync(ct);
        try
        {
            await db.Database.ExecuteSqlRawAsync(enableIdentityInsert, ct);
            await db.SaveChangesAsync(ct);
            await db.Database.ExecuteSqlRawAsync(disableIdentityInsert, ct);
        }
        finally
        {
            await db.Database.CloseConnectionAsync();
        }
    }
}

