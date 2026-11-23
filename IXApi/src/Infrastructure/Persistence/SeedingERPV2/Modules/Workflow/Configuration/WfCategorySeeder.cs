using IAX.IXApi.Infrastructure.Persistence.SeedingERPV2.Core.Contracts;
using IAX.IXApi.Infrastructure.Persistence.SeedingERPV2.Core.Models;
using IAX.IXApi.Modules.Workflow.Categories;

namespace IAX.IXApi.Infrastructure.Persistence.SeedingERPV2.Modules.Workflow;

/// <summary>The departments a process is filed under. A process stores the category identifier.</summary>
public sealed class WfCategorySeeder : IDataSeeder
{
    public string Id => "ERPV2.WfCategorySeeder";
    public IReadOnlyCollection<string> Dependencies => ["ERPV2.CompanyInfoSeeder"];

    public Task SeedAsync(SeedContext c, CancellationToken ct) => WorkflowReference.EnsureAsync(c,
    [
        new WfCategory { RecId = 1, Code = "CAT1", Name = "معاملات إدارة الموارد البشرية", SortOrder = 1, IsActive = true },
        new WfCategory { RecId = 2, Code = "CAT2", Name = "معاملات إدارة المبيعات", Description = "معاملات إدارة المبيعات", SortOrder = 2, IsActive = true },
        new WfCategory { RecId = 3, Code = "CAT3", Name = "معاملات إدارة العمل عن بعد", SortOrder = 3, IsActive = true },
        new WfCategory { RecId = 4, Code = "CAT4", Name = "إدارة تقنية المعلومات", SortOrder = 4, IsActive = true },
        new WfCategory { RecId = 6, Code = "CAT6", Name = "معاملات الإدارة المالية", Description = "معاملات الإدارة المالية", SortOrder = 6, IsActive = true },
        new WfCategory { RecId = 7, Code = "CAT7", Name = "معاملات إدارة الجودة", SortOrder = 7, IsActive = true },
        new WfCategory { RecId = 9, Code = "CAT9", Name = "معاملات إدارة الامتياز", SortOrder = 9, IsActive = true },
        new WfCategory { RecId = 10, Code = "CAT10", Name = "معاملات إدارة جرد الفروع", SortOrder = 10, IsActive = true },
        new WfCategory { RecId = 11, Code = "CAT11", Name = "معاملات إدارة الإستئجار", SortOrder = 11, IsActive = true },
        new WfCategory { RecId = 12, Code = "CAT12", Name = "معاملات إدارة الإنشاءات", Description = "معاملات إدارة الإنشاءات", SortOrder = 12, IsActive = true },
        new WfCategory { RecId = 13, Code = "CAT13", Name = "معاملات إدارة المشتريات", Description = "معاملات إدارة المشتريات", SortOrder = 13, IsActive = true },
        new WfCategory { RecId = 14, Code = "CAT14", Name = "معاملات إدراة المعارض", SortOrder = 14, IsActive = true },
        new WfCategory { RecId = 15, Code = "CAT15", Name = "معاملات إدارة التسويق", SortOrder = 15, IsActive = true },
        new WfCategory { RecId = 16, Code = "CAT16", Name = "معاملات خدمة العملاء", Description = "معاملات خدمة العملاء", SortOrder = 16, IsActive = true },
        new WfCategory { RecId = 17, Code = "CAT17", Name = "معاملات إدارة الشؤون القانونية", SortOrder = 17, IsActive = true },
        new WfCategory { RecId = 18, Code = "CAT18", Name = "معاملات اداره التشغيل والتصنيع", Description = "معامللت خاصة بادارة التصنيع", SortOrder = 18, IsActive = true },
        new WfCategory { RecId = 19, Code = "CAT19", Name = "الزيارات الإلكترونية", SortOrder = 19, IsActive = true },
        new WfCategory { RecId = 20, Code = "CAT20", Name = "التقييمات الإلكترونية", SortOrder = 20, IsActive = true },
        new WfCategory { RecId = 21, Code = "CAT21", Name = "معاملات إدارة نيش", SortOrder = 21, IsActive = true },
        new WfCategory { RecId = 22, Code = "CAT22", Name = "الإدارة العليا", SortOrder = 22, IsActive = true }
    ], ct);
}
