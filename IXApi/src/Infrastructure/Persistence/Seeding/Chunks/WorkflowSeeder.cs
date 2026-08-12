using IAX.IXApi.Infrastructure.Persistence;
using IAX.IXApi.Modules.Identity.Authentication;
using IAX.IXApi.Modules.Identity.Users;
using IAX.IXApi.Modules.Identity.Roles;
using IAX.IXApi.Modules.Identity.Impersonation;
using IAX.IXApi.Modules.Organization.Features.OrgEmployeeGroup;
using IAX.IXApi.Modules.Workflow.Activities;
using IAX.IXApi.Modules.Workflow.Steps;
using IAX.IXApi.Modules.Workflow.Categories;
using IAX.IXApi.Modules.Workflow.Controls;
using IAX.IXApi.Modules.Workflow.Operators;
using IAX.IXApi.Modules.Workflow.Priorities;
using IAX.IXApi.Modules.Workflow.Processes;
using IAX.IXApi.Modules.Workflow.Requests;
using IAX.IXApi.Modules.Workflow.Variables;
using IAX.IXApi.Modules.Workflow.Performers;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata;
using IAX.IXApi.Modules.Organization.Features.OrgEmployeeCategory;

namespace IAX.IXApi.Infrastructure.Persistence.Seeding.Chunks
{
    public class WorkflowSeeder : ISeeder
    {
        public async Task SeedAsync(ApplicationDbContext db, RoleManager<AspNetRole> roles, UserManager<AspNetUser> users, CancellationToken ct)
        {
            var sysUser = await users.FindByNameAsync("sys");
            var createdBy = sysUser?.Id ?? "sys";

            // Self-healing migration for existing databases to change legacy ID 0 to safe non-zero IDs
            await db.Database.OpenConnectionAsync(ct);
            try
            {
                var hasCat0 = await db.WfCategories.IgnoreQueryFilters().AnyAsync(x => x.RecId == 0, ct);
                if (hasCat0)
                {
                    await db.Database.ExecuteSqlRawAsync("UPDATE WfProcesses SET CategoryId = 5 WHERE CategoryId = 0", ct);
                    await db.Database.ExecuteSqlRawAsync("DELETE FROM WfCategories WHERE RecId = 0", ct);
                }

                var hasCat5 = await db.WfCategories.IgnoreQueryFilters().AnyAsync(x => x.RecId == 5, ct);
                if (!hasCat5)
                {
                    await db.Database.ExecuteSqlRawAsync("SET IDENTITY_INSERT WfCategories ON", ct);
                    await db.Database.ExecuteSqlRawAsync(@"
                        INSERT INTO WfCategories (RecId, Code, Name, NameAR, Description, DescriptionAR, IsActive, IsDeleted, SysField, SortOrder, CreatedBy, CreatedAt, OwnerAccountId, LastModifiedBy, LastModifiedAt, RecVersion, DataAreaId)
                        VALUES (5, 'FIN', N'معاملات منتهية', N'معاملات منتهية', N'معاملات منتهية', N'معاملات منتهية', 0, 0, 1, 0, 'sys', GETDATE(), 'sys', 'sys', GETDATE(), 1, 'dat')", ct);
                    await db.Database.ExecuteSqlRawAsync("SET IDENTITY_INSERT WfCategories OFF", ct);
                }

                var hasAct0 = await db.WfActivityTypes.IgnoreQueryFilters().AnyAsync(x => x.RecId == 0, ct);
                if (hasAct0)
                {
                    await db.Database.ExecuteSqlRawAsync("UPDATE WfActivities SET ActivityTypeId = 2 WHERE ActivityTypeId = 0", ct);
                    await db.Database.ExecuteSqlRawAsync("DELETE FROM WfActivityTypes WHERE RecId = 0", ct);
                }

                var hasAct2 = await db.WfActivityTypes.IgnoreQueryFilters().AnyAsync(x => x.RecId == 2, ct);
                if (!hasAct2)
                {
                    await db.Database.ExecuteSqlRawAsync("SET IDENTITY_INSERT WfActivityTypes ON", ct);
                    await db.Database.ExecuteSqlRawAsync(@"
                        INSERT INTO WfActivityTypes (RecId, Code, Name, NameAR, Description, DescriptionAR, IsActive, IsDeleted, SortOrder, CreatedBy, CreatedAt, OwnerAccountId, LastModifiedBy, LastModifiedAt, RecVersion, DataAreaId)
                        VALUES (2, 'NORMAL', N'مرحلة عادية', N'مرحلة عادية', N'مرحلة عادية', N'مرحلة عادية', 1, 0, 0, 'sys', GETDATE(), 'sys', 'sys', GETDATE(), 1, 'dat')", ct);
                    await db.Database.ExecuteSqlRawAsync("SET IDENTITY_INSERT WfActivityTypes OFF", ct);
                }
            }
            finally
            {
                await db.Database.CloseConnectionAsync();
            }

            #region WfCategory
            if (!await db.WfCategories.IgnoreQueryFilters().AnyAsync(x => x.RecId == 1, ct))
            {
                var categories = new List<WfCategory>
                {
                    new WfCategory { RecId = 5, Code = "FIN", Name = "معاملات منتهية", NameAR = "معاملات منتهية", Description = "معاملات منتهية", DescriptionAR = "معاملات منتهية", IsActive = false, IsDeleted = false, SysField = true, CreatedBy = createdBy, OwnerAccountId = createdBy },
                    new WfCategory { RecId = 1, Code = "HR", Name = "Human Resources Department", NameAR = "معاملات إدارة الموارد البشرية", Description = null, DescriptionAR = null, IsActive = true, IsDeleted = false, SysField = false, CreatedBy = createdBy, OwnerAccountId = createdBy },
                    new WfCategory { RecId = 2, Code = "SAL", Name = "Sales Department", NameAR = "معاملات إدارة المبيعات", Description = "Sales Department", DescriptionAR = "معاملات إدارة المبيعات", IsActive = true, IsDeleted = false, SysField = false, CreatedBy = createdBy, OwnerAccountId = createdBy },
                    new WfCategory { RecId = 3, Code = "REM", Name = "Remote work Department", NameAR = "معاملات إدارة العمل عن بعد", Description = null, DescriptionAR = null, IsActive = true, IsDeleted = false, SysField = false, CreatedBy = createdBy, OwnerAccountId = createdBy },
                    new WfCategory { RecId = 4, Code = "IT", Name = "IT Department", NameAR = "إدارة تقنية المعلومات", Description = null, DescriptionAR = null, IsActive = true, IsDeleted = false, SysField = false, CreatedBy = createdBy, OwnerAccountId = createdBy },
                    new WfCategory { RecId = 6, Code = "FINM", Name = "Financial management transactions", NameAR = "معاملات الإدارة المالية", Description = null, DescriptionAR = "معاملات الإدارة المالية", IsActive = true, IsDeleted = false, SysField = false, CreatedBy = createdBy, OwnerAccountId = createdBy },
                    new WfCategory { RecId = 7, Code = "QLTY", Name = "Quality Department", NameAR = "معاملات إدارة الجودة", Description = null, DescriptionAR = null, IsActive = true, IsDeleted = false, SysField = false, CreatedBy = createdBy, OwnerAccountId = createdBy },
                    new WfCategory { RecId = 9, Code = "FRAN", Name = "Franchise management", NameAR = "معاملات إدارة الامتياز", Description = null, DescriptionAR = null, IsActive = true, IsDeleted = false, SysField = false, CreatedBy = createdBy, OwnerAccountId = createdBy },
                    new WfCategory { RecId = 10, Code = "BINV", Name = "Branch inventory Department", NameAR = "معاملات إدارة جرد الفروع", Description = null, DescriptionAR = null, IsActive = true, IsDeleted = false, SysField = false, CreatedBy = createdBy, OwnerAccountId = createdBy },
                    new WfCategory { RecId = 11, Code = "RENT", Name = "Rental management", NameAR = "معاملات إدارة الإستئجار", Description = null, DescriptionAR = null, IsActive = true, IsDeleted = false, SysField = false, CreatedBy = createdBy, OwnerAccountId = createdBy },
                    new WfCategory { RecId = 12, Code = "CNST", Name = "Construction management", NameAR = "معاملات إدارة الإنشاءات", Description = null, DescriptionAR = "معاملات إدارة الإنشاءات", IsActive = true, IsDeleted = false, SysField = false, CreatedBy = createdBy, OwnerAccountId = createdBy },
                    new WfCategory { RecId = 13, Code = "PUR", Name = "Purchase Department", NameAR = "معاملات إدارة المشتريات", Description = null, DescriptionAR = "معاملات إدارة المشتريات", IsActive = true, IsDeleted = false, SysField = false, CreatedBy = createdBy, OwnerAccountId = createdBy },
                    new WfCategory { RecId = 14, Code = "BRCH", Name = "Branch management transactions", NameAR = "معاملات إدراة المعارض", Description = null, DescriptionAR = null, IsActive = true, IsDeleted = false, SysField = false, CreatedBy = createdBy, OwnerAccountId = createdBy },
                    new WfCategory { RecId = 15, Code = "MKTG", Name = "Marketing management transactions", NameAR = "معاملات إدارة التسويق ", Description = null, DescriptionAR = null, IsActive = true, IsDeleted = false, SysField = false, CreatedBy = createdBy, OwnerAccountId = createdBy },
                    new WfCategory { RecId = 16, Code = "CUST", Name = "Customer Service Transactions", NameAR = "معاملات خدمة العملاء", Description = "Customer Service Transactions", DescriptionAR = "معاملات خدمة العملاء", IsActive = true, IsDeleted = false, SysField = false, CreatedBy = createdBy, OwnerAccountId = createdBy },
                    new WfCategory { RecId = 17, Code = "LEGL", Name = "Transactions of the Legal Affairs Department", NameAR = "معاملات إدارة الشؤون القانونية", Description = null, DescriptionAR = null, IsActive = true, IsDeleted = false, SysField = false, CreatedBy = createdBy, OwnerAccountId = createdBy },
                    new WfCategory { RecId = 18, Code = "OPS", Name = "Operations and Manufacturing Management Transactions", NameAR = "معاملات اداره التشغيل والتصنيع", Description = "Operations and Manufacturing Management Transactions", DescriptionAR = "معامللت خاصة بادارة التصنيع", IsActive = true, IsDeleted = false, SysField = false, CreatedBy = createdBy, OwnerAccountId = createdBy },
                    new WfCategory { RecId = 19, Code = "EVIS", Name = "Electronic visits", NameAR = "الزيارات الإلكترونية", Description = null, DescriptionAR = null, IsActive = true, IsDeleted = false, SysField = false, CreatedBy = createdBy, OwnerAccountId = createdBy },
                    new WfCategory { RecId = 20, Code = "EASS", Name = "Online assessments", NameAR = "التقييمات الإلكترونية", Description = null, DescriptionAR = null, IsActive = true, IsDeleted = false, SysField = false, CreatedBy = createdBy, OwnerAccountId = createdBy },
                    new WfCategory { RecId = 21, Code = "NICH", Name = "Niche administration transactions", NameAR = "معاملات إدارة نيش", Description = null, DescriptionAR = null, IsActive = true, IsDeleted = false, SysField = false, CreatedBy = createdBy, OwnerAccountId = createdBy },
                    new WfCategory { RecId = 22, Code = "SNR", Name = "Senior management", NameAR = "الإدارة العليا", Description = null, DescriptionAR = null, IsActive = true, IsDeleted = false, SysField = false, CreatedBy = createdBy, OwnerAccountId = createdBy }
                };

                for (int i = 100; i < 400; i++)
                {
                    categories.Add(new WfCategory
                    {
                        RecId = (short)i,
                        Code = $"CAT_{i:000}",
                        Name = $"Category {i}",
                        NameAR = $"تصنيف رقم {i}",
                        Description = $"Description for Category {i}",
                        DescriptionAR = $"وصف للتصنيف رقم {i}",
                        IsActive = true,
                        IsDeleted = false,
                        SysField = false,
                        CreatedBy = createdBy,
                        OwnerAccountId = createdBy
                    });
                }

                var existingIds = await db.WfCategories.IgnoreQueryFilters().Select(c => c.RecId).ToListAsync(ct);
                var toAdd = categories.Where(c => !existingIds.Contains(c.RecId)).ToList();
                if (toAdd.Any())
                {
                    await db.WfCategories.AddRangeAsync(toAdd, ct);
                    await SeedWithIdentityInsertAsync(db, "WfCategories", ct);
                }
            }
            #endregion

            #region WfActivityTypes
            if (!await db.WfActivityTypes.IgnoreQueryFilters().AnyAsync(x => x.RecId == 1, ct))
            {
                var activityTypes = new[]
                {
                    new WfActivityType { RecId = 2, Code = "NORMAL", Name = "مرحلة عادية", NameAR = "مرحلة عادية", Description = "مرحلة عادية", DescriptionAR = "مرحلة عادية", IsActive = true, IsDeleted = false, SortOrder = 0, CreatedBy = createdBy, OwnerAccountId = createdBy },
                    new WfActivityType { RecId = 1, Code = "PARTIAL", Name = "مرحلة جزئية", NameAR = "مرحلة جزئية", Description = "مرحلة جزئية", DescriptionAR = "مرحلة جزئية", IsActive = true, IsDeleted = false, SortOrder = 1, CreatedBy = createdBy, OwnerAccountId = createdBy }
                };

                var existingIds = await db.WfActivityTypes.IgnoreQueryFilters().Select(x => x.RecId).ToListAsync(ct);
                var toAdd = activityTypes.Where(x => !existingIds.Contains(x.RecId)).ToList();
                if (toAdd.Any())
                {
                    await db.WfActivityTypes.AddRangeAsync(toAdd, ct);
                    await SeedWithIdentityInsertAsync(db, "WfActivityTypes", ct);
                }
            }
            #endregion

            #region WfPriority
            if (!await db.WfPriorities.IgnoreQueryFilters().AnyAsync(x => x.RecId == 1, ct))
            {
                var priorities = new[]
                {
                    new WfPriority { RecId = 1, Code = "LOW", Name = "Low", NameAR = "منخفض", Description = "Low Priority", DescriptionAR = "أولوية منخفضة", IsActive = true, IsDeleted = false, SortOrder = 1, CreatedBy = createdBy, OwnerAccountId = createdBy },
                    new WfPriority { RecId = 2, Code = "MED", Name = "Medium", NameAR = "متوسط", Description = "Medium Priority", DescriptionAR = "أولوية متوسطة", IsActive = true, IsDeleted = false, SortOrder = 2, CreatedBy = createdBy, OwnerAccountId = createdBy },
                    new WfPriority { RecId = 3, Code = "HIGH", Name = "High", NameAR = "عالي", Description = "High Priority", DescriptionAR = "أولوية عالية", IsActive = true, IsDeleted = false, SortOrder = 3, CreatedBy = createdBy, OwnerAccountId = createdBy }
                };

                var existingIds = await db.WfPriorities.IgnoreQueryFilters().Select(x => x.RecId).ToListAsync(ct);
                var toAdd = priorities.Where(x => !existingIds.Contains(x.RecId)).ToList();
                if (toAdd.Any())
                {
                    await db.WfPriorities.AddRangeAsync(toAdd, ct);
                    await SeedWithIdentityInsertAsync(db, "WfPriorities", ct);
                }
            }
            #endregion

            #region WfControls
            if (!await db.WfControls.IgnoreQueryFilters().AnyAsync(x => x.RecId == 1, ct))
            {
                var controls = new[]
                {
                    new WfControl { RecId = 1, Code = "number", Name = "Digits", NameAR = "مربع رقمي", ControlType = "TextBox", Description = "Digits", DescriptionAR = "مربع رقمي", IsActive = true, IsDeleted = false, SortOrder = 1, CreatedBy = createdBy, OwnerAccountId = createdBy },
                    new WfControl { RecId = 2, Code = "text", Name = "Text", NameAR = "مربع نصي", ControlType = "TextBox", Description = "Text", DescriptionAR = "مربع نصي", IsActive = true, IsDeleted = false, SortOrder = 2, CreatedBy = createdBy, OwnerAccountId = createdBy },
                    new WfControl { RecId = 3, Code = "textarea", Name = "Long text", NameAR = "نص طويل", ControlType = "TextBox", Description = "Long text", DescriptionAR = "نص طويل", IsActive = true, IsDeleted = false, SortOrder = 3, CreatedBy = createdBy, OwnerAccountId = createdBy },
                    new WfControl { RecId = 4, Code = "date", Name = "Date", NameAR = "تاريخ", ControlType = "Calendar", Description = "Date", DescriptionAR = "تاريخ", IsActive = true, IsDeleted = false, SortOrder = 4, CreatedBy = createdBy, OwnerAccountId = createdBy },
                    new WfControl { RecId = 5, Code = "select", Name = "Drop Down List (Fill From DataBase)", NameAR = "قائمة منسدلة (تعبأ من قاعدة البيانات)", ControlType = "DropDownList", Description = "Drop Down List (Fill From DataBase)", DescriptionAR = "قائمة منسدلة (تعبأ من قاعدة البيانات)", IsActive = true, IsDeleted = false, SortOrder = 5, CreatedBy = createdBy, OwnerAccountId = createdBy },
                    new WfControl { RecId = 6, Code = "select", Name = "Drop Down List (Fill Manually)", NameAR = "قائمة منسدلة (تعبأ يدويا)", ControlType = "DropDownList", Description = "Drop Down List (Fill Manually)", DescriptionAR = "قائمة منسدلة (تعبأ يدويا)", IsActive = true, IsDeleted = false, SortOrder = 6, CreatedBy = createdBy, OwnerAccountId = createdBy },
                    new WfControl { RecId = 7, Code = "checkbox", Name = "Check box", NameAR = "مربع إختيار", ControlType = "CheckBox", Description = "Check box", DescriptionAR = "مربع إختيار", IsActive = true, IsDeleted = false, SortOrder = 7, CreatedBy = createdBy, OwnerAccountId = createdBy },
                    new WfControl { RecId = 9, Code = "table", Name = "Table", NameAR = "جدول", ControlType = "Table", Description = "Table", DescriptionAR = "جدول", IsActive = true, IsDeleted = false, SortOrder = 9, CreatedBy = createdBy, OwnerAccountId = createdBy },
                    new WfControl { RecId = 10, Code = "label", Name = "Label", NameAR = "نص للقراءة فقط", ControlType = "Label", Description = "Label", DescriptionAR = "نص للقراءة فقط", IsActive = true, IsDeleted = false, SortOrder = 10, CreatedBy = createdBy, OwnerAccountId = createdBy },
                    new WfControl { RecId = 11, Code = "radio", Name = "RadioButtonList", NameAR = "قائمة اختيار فردي", ControlType = "RadioButtonList", Description = "RadioButtonList", DescriptionAR = "قائمة اختيار فردي", IsActive = true, IsDeleted = false, SortOrder = 11, CreatedBy = createdBy, OwnerAccountId = createdBy },
                    new WfControl { RecId = 12, Code = "search", Name = "EmployeeSearch", NameAR = "بحث في الموظفين", ControlType = "ComboBox", Description = "EmployeeSearch", DescriptionAR = "بحث في الموظفين", IsActive = true, IsDeleted = false, SortOrder = 12, CreatedBy = createdBy, OwnerAccountId = createdBy },
                    new WfControl { RecId = 13, Code = "time", Name = "Time", NameAR = "وقت", ControlType = "TextBox", Description = "Time", DescriptionAR = "وقت", IsActive = true, IsDeleted = false, SortOrder = 13, CreatedBy = createdBy, OwnerAccountId = createdBy },
                    new WfControl { RecId = 16, Code = "url", Name = "Url", NameAR = "رابط", ControlType = "TextBox", Description = "Url", DescriptionAR = "رابط", IsActive = true, IsDeleted = false, SortOrder = 16, CreatedBy = createdBy, OwnerAccountId = createdBy },
                    new WfControl { RecId = 17, Code = "file", Name = "File", NameAR = "ملف", ControlType = "File", Description = "File", DescriptionAR = "ملف", IsActive = true, IsDeleted = false, SortOrder = 17, CreatedBy = createdBy, OwnerAccountId = createdBy },
                    new WfControl { RecId = 18, Code = "showroom", Name = "Showroom", NameAR = "معرض", ControlType = "Showroom", Description = "Showroom", DescriptionAR = "معرض", IsActive = true, IsDeleted = false, SortOrder = 18, CreatedBy = createdBy, OwnerAccountId = createdBy },
                    new WfControl { RecId = 19, Code = "EmployeeID", Name = "EmployeeID", NameAR = "رقم وظيفي", ControlType = "EmployeeID", Description = "EmployeeID", DescriptionAR = "رقم وظيفي", IsActive = true, IsDeleted = false, SortOrder = 19, CreatedBy = createdBy, OwnerAccountId = createdBy },
                    new WfControl { RecId = 20, Code = "Signature", Name = "Signature", NameAR = "توقيع", ControlType = "Signature", Description = "Signature", DescriptionAR = "توقيع", IsActive = true, IsDeleted = false, SortOrder = 20, CreatedBy = createdBy, OwnerAccountId = createdBy },
                    new WfControl { RecId = 21, Code = "Location", Name = "Location", NameAR = "الموقع", ControlType = "Location", Description = "Location", DescriptionAR = "الموقع", IsActive = true, IsDeleted = false, SortOrder = 21, CreatedBy = createdBy, OwnerAccountId = createdBy },
                    new WfControl { RecId = 22, Code = "Advertiser", Name = "Advertiser", NameAR = "معلن", ControlType = "Advertiser", Description = "Advertiser", DescriptionAR = "معلن", IsActive = true, IsDeleted = false, SortOrder = 22, CreatedBy = createdBy, OwnerAccountId = createdBy },
                    new WfControl { RecId = 23, Code = "CheckBoxList", Name = "CheckBoxList", NameAR = "قائمة اختيار متعددة", ControlType = "CheckBoxList", Description = "CheckBoxList", DescriptionAR = "قائمة اختيار متعددة", IsActive = true, IsDeleted = false, SortOrder = 23, CreatedBy = createdBy, OwnerAccountId = createdBy }
                };

                var existingIds = await db.WfControls.IgnoreQueryFilters().Select(x => x.RecId).ToListAsync(ct);
                var toAdd = controls.Where(x => !existingIds.Contains(x.RecId)).ToList();
                if (toAdd.Any())
                {
                    await db.WfControls.AddRangeAsync(toAdd, ct);
                    await SeedWithIdentityInsertAsync(db, "WfControls", ct);
                }
            }
            #endregion

            #region WfProcessTypes
            {
                var processTypes = new[]
                {
                    new IAX.IXApi.Modules.Workflow.ProcessTypes.WfProcessType { RecId = 1, Code = "STD", Name = "Standard", NameAR = "قياسي", IsActive = true, IsDeleted = false, CreatedBy = createdBy, OwnerAccountId = createdBy },
                    new IAX.IXApi.Modules.Workflow.ProcessTypes.WfProcessType { RecId = 2, Code = "REV", Name = "Review", NameAR = "مراجعة", IsActive = true, IsDeleted = false, CreatedBy = createdBy, OwnerAccountId = createdBy },
                    new IAX.IXApi.Modules.Workflow.ProcessTypes.WfProcessType { RecId = 3, Code = "APP", Name = "Approval", NameAR = "اعتماد", IsActive = true, IsDeleted = false, CreatedBy = createdBy, OwnerAccountId = createdBy }
                };

                var existingIds = await db.WfProcessTypes.IgnoreQueryFilters().Select(x => x.RecId).ToListAsync(ct);
                var toAdd = processTypes.Where(x => !existingIds.Contains(x.RecId)).ToList();
                if (toAdd.Any())
                {
                    await db.WfProcessTypes.AddRangeAsync(toAdd, ct);
                    await SeedWithIdentityInsertAsync(db, "WfProcessTypes", ct);
                }
            }
            #endregion

            #region WfProcesses
            {
                var hrCatId = (await db.WfCategories.IgnoreQueryFilters().FirstOrDefaultAsync(c => c.Code == "HR", ct))?.RecId ?? 1;
                var itCatId = (await db.WfCategories.IgnoreQueryFilters().FirstOrDefaultAsync(c => c.Code == "IT", ct))?.RecId ?? 4;
                var purCatId = (await db.WfCategories.IgnoreQueryFilters().FirstOrDefaultAsync(c => c.Code == "PUR", ct))?.RecId ?? 13;
                var medPriorityId = (await db.WfPriorities.IgnoreQueryFilters().FirstOrDefaultAsync(p => p.Code == "MED", ct))?.RecId ?? 2;

                var processes = new[]
                {
                    new WfProcess { RecId = 591, Code = "DISC_PROC", Name = "Disciplinary Process", NameAR = "عملية رصد المخالفات", CategoryId = hrCatId, PriorityId = medPriorityId, ProcessTypeId = 1, IsActive = true, IsDeleted = false, CreatedBy = createdBy, OwnerAccountId = createdBy },
                    new WfProcess { RecId = 592, Code = "ONBOARDING", Name = "Employee Onboarding", NameAR = "تهيئة موظف جديد", CategoryId = hrCatId, PriorityId = medPriorityId, ProcessTypeId = 1, IsActive = true, IsDeleted = false, CreatedBy = createdBy, OwnerAccountId = createdBy },
                    new WfProcess { RecId = 593, Code = "LEAVE_REQ", Name = "Leave Request", NameAR = "طلب إجازة", CategoryId = hrCatId, PriorityId = medPriorityId, ProcessTypeId = 1, IsActive = true, IsDeleted = false, CreatedBy = createdBy, OwnerAccountId = createdBy },
                    new WfProcess { RecId = 594, Code = "PO_APPROV", Name = "Purchase Order Approval", NameAR = "اعتماد أمر شراء", CategoryId = purCatId, PriorityId = medPriorityId, ProcessTypeId = 1, IsActive = true, IsDeleted = false, CreatedBy = createdBy, OwnerAccountId = createdBy },
                    new WfProcess { RecId = 595, Code = "IT_TICKET", Name = "IT Support Ticket", NameAR = "طلب دعم تقني", CategoryId = itCatId, PriorityId = medPriorityId, ProcessTypeId = 1, IsActive = true, IsDeleted = false, CreatedBy = createdBy, OwnerAccountId = createdBy }
                };

                var existingIds = await db.WfProcesses.IgnoreQueryFilters().Select(x => x.RecId).ToListAsync(ct);
                var toAdd = processes.Where(x => !existingIds.Contains(x.RecId)).ToList();
                if (toAdd.Any())
                {
                    await db.WfProcesses.AddRangeAsync(toAdd, ct);
                    await SeedWithIdentityInsertAsync(db, "WfProcesses", ct);
                }
            }
            #endregion

            #region WfRequests
            if (!await db.WfRequests.IgnoreQueryFilters().AnyAsync(ct))
            {
                if (false)
                {
                    var hrCatId = (await db.WfCategories.IgnoreQueryFilters().FirstOrDefaultAsync(c => c.Code == "HR", ct))?.RecId ?? 1;
                    var medPriorityId = (await db.WfPriorities.IgnoreQueryFilters().FirstOrDefaultAsync(p => p.Code == "MED", ct))?.RecId ?? 2;

                    var mockProcess = new WfProcess
                    {
                        RecId = 591,
                        Code = "DISC_PROC",
                        Name = "Disciplinary Process",
                        NameAR = "عملية رصد المخالفات",
                        CategoryId = hrCatId,
                        PriorityId = medPriorityId,
                        ProcessTypeId = 1,
                        IsActive = true,
                        IsDeleted = false,
                        CreatedBy = createdBy,
                        OwnerAccountId = createdBy
                    };
                    await db.WfProcesses.AddAsync(mockProcess, ct);
                    await SeedWithIdentityInsertAsync(db, "WfProcesses", ct);
                }

                var defaultRequests = new[]
                {
                    new WfRequest
                    {
                        Code="1",
                        Name="Disciplinary action Empolyee",
                        NameAR="رصد مخالفة موظف",
                        ProcessId    = 591,
                        RequestDate  = DateTime.Parse("2024-10-26 09:50:59.437"),
                        IsFinished   = true,
                        CreatedBy    = createdBy,
                        CreatedAt    = DateTime.Parse("2024-10-26 09:50:59.437"),
                        OwnerAccountId = createdBy,
                        RequestDetails = "Initial disciplinary action request seeder"
                    }
                };
                await db.WfRequests.AddRangeAsync(defaultRequests, ct);
                await db.SaveChangesAsync(ct);
            }
            #endregion

            #region WfPerformerTypes
            if (!await db.Set<WfPerformerType>().AnyAsync(ct))
            {
                db.Set<WfPerformerType>().Add(new WfPerformerType
                {
                    RecId = 1,
                    Code = "RELATIONAL",
                    Name = "Relational",
                    NameAR = "علاقات وظيفية",
                    SortOrder = 1,
                    IsActive = true,
                    CreatedBy = "sys",
                    CreatedAt = DateTime.UtcNow,
                    OwnerAccountId = "sys",
                    DataAreaId = "dat"
                });
                await SeedWithIdentityInsertAsync(db, "WfPerformerType", ct);
            }
            #endregion

            #region WfPerformers
            var performers = new[]
            {
                new WfPerformer
                {
                        RecId = 1,
                        Code =  "1",
                        PerformerTypeId = 1,
                        RelatedField = null,
                        IsApplicant = true,
                        IsEmployee = false,
                        IsManager1 = false,
                        IsManager2 = false,
                        IsManager3 = false,
                        IsManager4 = false,
                        CreatedBy = "sys",
                        CreatedAt = DateTime.UtcNow,
                        OwnerAccountId = "sys",
                        IsActive = true,
                        NameAR = "مقدم الطلب",
                        Name = "applicant",
                      
                    },
                    new WfPerformer
                    {
                        RecId = 2,
                        Code = "2",
                        PerformerTypeId = 1,
                        RelatedField = null,
                        IsApplicant = false,
                        IsEmployee = false,
                        IsManager1 = true,
                        IsManager2 = false,
                        IsManager3 = false,
                        IsManager4 = false,
                        CreatedBy = "sys",
                        CreatedAt = DateTime.UtcNow,
                        OwnerAccountId = "sys",
                        IsActive = true,
                        NameAR = "المدير الأول",
                        Name = "Manager1",
                    
                    },
                    new WfPerformer
                    {
                        RecId = 3,
                        Code = "3",
                        PerformerTypeId = 1,
                        RelatedField = null,
                        IsApplicant = false,
                        IsEmployee = false,
                        IsManager1 = false,
                        IsManager2 = true,
                        IsManager3 = false,
                        IsManager4 = false,
                        CreatedBy = "sys",
                        CreatedAt = DateTime.UtcNow,
                        OwnerAccountId = "sys",
                        IsActive = true,
                        NameAR = "المدير الثاني",
                        Name = "Manager2",
                       
                    },
                    new WfPerformer
                    {
                        RecId = 4,
                        Code = "4",
                        PerformerTypeId = 1,
                        RelatedField = null,
                        IsApplicant = false,
                        IsEmployee = false,
                        IsManager1 = false,
                        IsManager2 = false,
                        IsManager3 = true,
                        IsManager4 = false,
                        CreatedBy = "sys",
                        CreatedAt = DateTime.UtcNow,
                        OwnerAccountId = "sys",
                        IsActive = true,
                        NameAR = "المدير الثالث",
                        Name = "Manager3",
                        
                    },
                    new WfPerformer
                    {
                        RecId = 5,
                        Code = "5",
                        PerformerTypeId = 1,
                        RelatedField = null,
                        IsApplicant = false,
                        IsEmployee = false,
                        IsManager1 = false,
                        IsManager2 = false,
                        IsManager3 = false,
                        IsManager4 = true,
                        CreatedBy = "sys",
                        CreatedAt = DateTime.UtcNow,
                        OwnerAccountId = "sys",
                        IsActive = true,
                        NameAR = "المدير الرابع",
                        Name = "Manager4",
                    
                    }
                };

            var existingPerformers = await db.WfPerformers.IgnoreQueryFilters().Select(x => x.RecId).ToListAsync(ct);
            var toAddPerformers = performers.Where(x => !existingPerformers.Contains(x.RecId)).ToList();
            if (toAddPerformers.Any())
            {
                await db.WfPerformers.AddRangeAsync(toAddPerformers, ct);
                await SeedWithIdentityInsertAsync(db, "WfPerformers", ct);
            }
            #endregion

            #region WfOperators
            if (!await db.WfOperators.IgnoreQueryFilters().AnyAsync(x => x.RecId == 1, ct))
            {
                var operators = new[]
                {
                    new WfOperator { RecId = 1, Code = "GT", Name = ">", NameAR = "أكبر من", Description = ">", DescriptionAR = "أكبر من", IsActive = true, IsDeleted = false, SortOrder = 1, CreatedBy = createdBy, OwnerAccountId = createdBy },
                    new WfOperator { RecId = 2, Code = "LT", Name = "<", NameAR = "أصغر من", Description = "<", DescriptionAR = "أصغر من", IsActive = true, IsDeleted = false, SortOrder = 2, CreatedBy = createdBy, OwnerAccountId = createdBy },
                    new WfOperator { RecId = 3, Code = "GTE", Name = ">=", NameAR = "أكبر من أو يساوي", Description = ">=", DescriptionAR = "أكبر من أو يساوي", IsActive = true, IsDeleted = false, SortOrder = 3, CreatedBy = createdBy, OwnerAccountId = createdBy },
                    new WfOperator { RecId = 4, Code = "LTE", Name = "<=", NameAR = "أصغر من أو يساوي", Description = "<=", DescriptionAR = "أصغر من أو يساوي", IsActive = true, IsDeleted = false, SortOrder = 4, CreatedBy = createdBy, OwnerAccountId = createdBy },
                    new WfOperator { RecId = 5, Code = "EQ", Name = "=", NameAR = "يساوي", Description = "=", DescriptionAR = "يساوي", IsActive = true, IsDeleted = false, SortOrder = 5, CreatedBy = createdBy, OwnerAccountId = createdBy },
                    new WfOperator { RecId = 6, Code = "NEQ", Name = "<>", NameAR = "لا يساوي", Description = "<>", DescriptionAR = "لا يساوي", IsActive = true, IsDeleted = false, SortOrder = 6, CreatedBy = createdBy, OwnerAccountId = createdBy },
                    new WfOperator { RecId = 7, Code = "BETWEEN", Name = "Between", NameAR = "بين", Description = "Between", DescriptionAR = "بين", IsActive = true, IsDeleted = false, SortOrder = 7, CreatedBy = createdBy, OwnerAccountId = createdBy }
                };

                var existingIds = await db.WfOperators.IgnoreQueryFilters().Select(x => x.RecId).ToListAsync(ct);
                var toAdd = operators.Where(x => !existingIds.Contains(x.RecId)).ToList();
                if (toAdd.Any())
                {
                    await db.WfOperators.AddRangeAsync(toAdd, ct);
                    await SeedWithIdentityInsertAsync(db, "WfOperators", ct);
                }
            }
            #endregion

            #region WfDataTypes
            if (!await db.WfDataTypes.IgnoreQueryFilters().AnyAsync(x => x.RecId == 1, ct))
            {
                var dataTypes = new[]
                {
                    new WfDataType { RecId = 1, Code = "INT", Name = "Integre", NameAR = "Integre", Description = "Integre", DescriptionAR = "Integre", IsActive = true, IsDeleted = false, SortOrder = 1, CreatedBy = createdBy, OwnerAccountId = createdBy },
                    new WfDataType { RecId = 2, Code = "STR", Name = "String", NameAR = "String", Description = "String", DescriptionAR = "String", IsActive = true, IsDeleted = false, SortOrder = 2, CreatedBy = createdBy, OwnerAccountId = createdBy },
                    new WfDataType { RecId = 3, Code = "DT", Name = "Date/Time", NameAR = "Date/Time", Description = "Date/Time", DescriptionAR = "Date/Time", IsActive = true, IsDeleted = false, SortOrder = 3, CreatedBy = createdBy, OwnerAccountId = createdBy },
                    new WfDataType { RecId = 4, Code = "BOOL", Name = "True/False", NameAR = "True/False", Description = "True/False", DescriptionAR = "True/False", IsActive = true, IsDeleted = false, SortOrder = 4, CreatedBy = createdBy, OwnerAccountId = createdBy }
                };

                var existingIds = await db.WfDataTypes.IgnoreQueryFilters().Select(x => x.RecId).ToListAsync(ct);
                var toAdd = dataTypes.Where(x => !existingIds.Contains(x.RecId)).ToList();
                if (toAdd.Any())
                {
                    await db.WfDataTypes.AddRangeAsync(toAdd, ct);
                    await SeedWithIdentityInsertAsync(db, "WfDataTypes", ct);
                }
            }
            #endregion

            #region WfVariables
            var variableProcessId = await db.WfProcesses
                .IgnoreQueryFilters()
                .Where(x => !x.IsDeleted)
                .OrderByDescending(x => x.RecId == 591)
                .ThenBy(x => x.RecId)
                .Select(x => (long?)x.RecId)
                .FirstOrDefaultAsync(ct);



            if (variableProcessId.HasValue)
            {
                var dataTypeIds = await db.WfDataTypes
                    .IgnoreQueryFilters()
                    .Where(x => !x.IsDeleted && x.Code != null)
                    .ToDictionaryAsync(x => x.Code!, x => x.RecId, StringComparer.OrdinalIgnoreCase, ct);

                var variableSeeds = new[]
                {
                    new { Code = "REQUESTER_NAME", Name = "Requester name", NameAR = "اسم مقدم الطلب", Description = "Name of the workflow requester", DescriptionAR = "اسم مقدم طلب سير العمل", DataTypeCode = "STR", SortOrder = (byte)1 },
                    new { Code = "REQUEST_DATE", Name = "Request date", NameAR = "تاريخ الطلب", Description = "Date the workflow request was created", DescriptionAR = "تاريخ إنشاء طلب سير العمل", DataTypeCode = dataTypeIds.ContainsKey("DATE") ? "DATE" : "DT", SortOrder = (byte)2 },
                    new { Code = "TOTAL_AMOUNT", Name = "Total amount", NameAR = "المبلغ الإجمالي", Description = "Total amount associated with the request", DescriptionAR = "إجمالي المبلغ المرتبط بالطلب", DataTypeCode = dataTypeIds.ContainsKey("NUM") ? "NUM" : "INT", SortOrder = (byte)3 },
                    new { Code = "IS_URGENT", Name = "Is urgent", NameAR = "طلب عاجل", Description = "Indicates whether the request is urgent", DescriptionAR = "يحدد ما إذا كان الطلب عاجلاً", DataTypeCode = "BOOL", SortOrder = (byte)4 }
                };

                var existingVariableCodes = await db.WfVariables
                    .IgnoreQueryFilters()
                    .Where(x => x.Code != null)
                    .Select(x => x.Code!)
                    .ToListAsync(ct);
                var existingCodeSet = existingVariableCodes.ToHashSet(StringComparer.OrdinalIgnoreCase);

                var variablesToAdd = variableSeeds
                    .Where(x => !existingCodeSet.Contains(x.Code) && dataTypeIds.ContainsKey(x.DataTypeCode))
                    .Select(x => new WfVariable
                    {
                        Code = x.Code,
                        Name = x.Name,
                        NameAR = x.NameAR,
                        Description = x.Description,
                        DescriptionAR = x.DescriptionAR,
                        DataTypeId = dataTypeIds[x.DataTypeCode],
                        ProcessId = variableProcessId.Value,
                        SortOrder = x.SortOrder,
                        IsActive = true,
                        IsDeleted = false,
                        CreatedBy = createdBy,
                        OwnerAccountId = createdBy
                    })
                    .ToList();

                if (variablesToAdd.Count > 0)
                {
                    await db.WfVariables.AddRangeAsync(variablesToAdd, ct);
                    await db.SaveChangesAsync(ct);
                }
            }
            #endregion

            #region WfSteps
            if (!await db.WfSteps.IgnoreQueryFilters().AnyAsync(x => x.RecId == 1, ct))
            {
                var steps = new[]
                {
                    new WfStep { RecId = 1, ProcessId = 591, Code = "STEP_1", Name = "Step 1", NameAR = "الخطوة الأولى", SortOrder = 1, Score = 0, AutoPassingHrs = 0, AllMandatory = false, SysField = false, IsActive = true, IsDeleted = false, CreatedBy = createdBy, OwnerAccountId = createdBy },
                    new WfStep { RecId = 2, ProcessId = 591, Code = "STEP_2", Name = "Step 2", NameAR = "الخطوة الثانية", SortOrder = 2, Score = 0, AutoPassingHrs = 0, AllMandatory = false, SysField = false, IsActive = true, IsDeleted = false, CreatedBy = createdBy, OwnerAccountId = createdBy }
                };

                var existingIds = await db.WfSteps.IgnoreQueryFilters().Select(x => x.RecId).ToListAsync(ct);
                var toAdd = steps.Where(x => !existingIds.Contains(x.RecId)).ToList();
                if (toAdd.Any())
                {
                    await db.WfSteps.AddRangeAsync(toAdd, ct);
                    await SeedWithIdentityInsertAsync(db, "WfSteps", ct);
                }
            }
            #endregion

            #region UserGroups
            if (!await db.OrgEmployeeGroups.AnyAsync(ct))
            {
                var userGroups = new[]
                {
                    new OrgEmployeeGroup { Code = "ALL",  Name = "جميع المستخدمين",          NameAR = "جميع المستخدمين" },
                    new OrgEmployeeGroup { Code = "HR",   Name = "إدارة الموارد البشرية",    NameAR = "إدارة الموارد البشرية" },
                    new OrgEmployeeGroup { Code = "IT",   Name = "إدارة تقنية المعلومات",    NameAR = "إدارة تقنية المعلومات" },
                    new OrgEmployeeGroup { Code = "FIN",  Name = "الإدارة المالية",           NameAR = "الإدارة المالية" },
                    new OrgEmployeeGroup { Code = "MGT",  Name = "الإدارة العليا",            NameAR = "الإدارة العليا" },
                    new OrgEmployeeGroup { Code = "OPS",  Name = "إدارة العمليات",            NameAR = "إدارة العمليات" },
                    new OrgEmployeeGroup { Code = "SALES",Name = "إدارة المبيعات",            NameAR = "إدارة المبيعات" },
                };
                await db.OrgEmployeeGroups.AddRangeAsync(userGroups, ct);
                await db.SaveChangesAsync(ct);
            }
            #endregion

            #region UserCategories
            if (!await db.OrgEmployeeCategories.AnyAsync(ct))
            {
                var hrGroup  = await db.OrgEmployeeGroups.FirstOrDefaultAsync(g => g.Code == "HR",  ct);
                var itGroup  = await db.OrgEmployeeGroups.FirstOrDefaultAsync(g => g.Code == "IT",  ct);
                var finGroup = await db.OrgEmployeeGroups.FirstOrDefaultAsync(g => g.Code == "FIN", ct);
                var mgtGroup = await db.OrgEmployeeGroups.FirstOrDefaultAsync(g => g.Code == "MGT", ct);

                var categories = new[]
                {
                    new OrgEmployeeCategory
                    {
                        Name      = "الكل",
                        NameAR    = "الكل",
                        ForAll    = true,
                        Manager1  = false, Manager2 = false, Manager3 = false, Manager4 = false,
                        IsActive  = true,
                    },
                    new OrgEmployeeCategory
                    {
                        Name      = "موظفو الموارد البشرية",
                        NameAR    = "موظفو الموارد البشرية",
                        ForAll    = false,
                        Manager1  = true, Manager2 = false, Manager3 = false, Manager4 = false,
                        IsActive  = true,
                    },
                    new OrgEmployeeCategory
                    {
                        Name      = "موظفو تقنية المعلومات",
                        NameAR    = "موظفو تقنية المعلومات",
                        ForAll    = false,
                        Manager1  = false, Manager2 = false, Manager3 = false, Manager4 = false,
                        IsActive  = true,
                    },
                    new OrgEmployeeCategory
                    {
                        Name      = "الإدارة المالية",
                        NameAR    = "الإدارة المالية",
                        ForAll    = false,
                        Manager1  = true, Manager2 = true, Manager3 = false, Manager4 = false,
                        IsActive  = true,
                    },
                    new OrgEmployeeCategory
                    {
                        Name      = "الإدارة العليا",
                        NameAR    = "الإدارة العليا",
                        ForAll    = false,
                        Manager1  = true, Manager2 = true, Manager3 = true, Manager4 = true,
                        IsActive  = true,
                    },
                };
                await db.OrgEmployeeCategories.AddRangeAsync(categories, ct);
                await db.SaveChangesAsync(ct);

                // Seed OrgEmployeeCategoryGroup linkages (UserGroupID moved from OrgEmployeeCategory to OrgEmployeeCategoryGroup)
                var groupLinks = new List<OrgEmployeeCategoryGroup>();
                if (hrGroup  != null) groupLinks.Add(new OrgEmployeeCategoryGroup { UserCategoriesID = categories[1].RecId, UserGroupID = hrGroup.RecId,  IsActive = true });
                if (itGroup  != null) groupLinks.Add(new OrgEmployeeCategoryGroup { UserCategoriesID = categories[2].RecId, UserGroupID = itGroup.RecId,  IsActive = true });
                if (finGroup != null) groupLinks.Add(new OrgEmployeeCategoryGroup { UserCategoriesID = categories[3].RecId, UserGroupID = finGroup.RecId, IsActive = true });
                if (mgtGroup != null) groupLinks.Add(new OrgEmployeeCategoryGroup { UserCategoriesID = categories[4].RecId, UserGroupID = mgtGroup.RecId, IsActive = true });
                if (groupLinks.Any())
                {
                    await db.OrgEmployeeCategoryGroups.AddRangeAsync(groupLinks, ct);
                    await db.SaveChangesAsync(ct);
                }
            }
            #endregion

            #region WfActivities
            if (!await db.WfActivities.IgnoreQueryFilters().AnyAsync(x => x.RecId == 2, ct))
            {
                var activities = new[]
                {
                    new WfActivity
                    {
                        RecId = 1,
                        Code = "ACT_1",
                        Name = "Activity 1",
                        NameAR = "النشاط الأول",
                        ActivityTypeId = 2, // Normal
                        StepId = 1,
                        PerformerId = 1, // Applicant
                        Score = 1,
                        IsActive = true,
                        IsDeleted = false,
                        CreatedBy = createdBy,
                        OwnerAccountId = createdBy
                    },
                    new WfActivity
                    {
                        RecId = 2,
                        Code = "ACT_2",
                        Name = "Activity 2",
                        NameAR = "النشاط الثاني",
                        ActivityTypeId = 1, // Partial
                        StepId = 1,
                        PerformerId = 2, // Manager1
                        Score = 3,
                        IsActive = true,
                        IsDeleted = false,
                        CreatedBy = createdBy,
                        OwnerAccountId = createdBy
                    }
                };

                var existingIds = await db.WfActivities.IgnoreQueryFilters().Select(x => x.RecId).ToListAsync(ct);
                var toAdd = activities.Where(x => !existingIds.Contains(x.RecId)).ToList();
                if (toAdd.Any())
                {
                    await db.WfActivities.AddRangeAsync(toAdd, ct);
                    await SeedWithIdentityInsertAsync(db, "WfActivities", ct);
                }
            }
            #endregion
        }

        private static async Task SeedWithIdentityInsertAsync(ApplicationDbContext db, string tableName, CancellationToken ct)
        {
            await db.Database.OpenConnectionAsync(ct);
            try
            {
                await db.Database.ExecuteSqlRawAsync("SET IDENTITY_INSERT " + tableName + " ON", ct);
                await db.SaveChangesAsync(ct);
                await db.Database.ExecuteSqlRawAsync("SET IDENTITY_INSERT " + tableName + " OFF", ct);
            }
            finally
            {
                await db.Database.CloseConnectionAsync();
            }
        }
    }
}


