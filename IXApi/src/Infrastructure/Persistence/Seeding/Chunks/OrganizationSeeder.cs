using IAX.IXApi.Infrastructure.Persistence;
using IAX.IXApi.Modules.Identity.Authentication;
using IAX.IXApi.Modules.Identity.Users;
using IAX.IXApi.Modules.Identity.Roles;
using IAX.IXApi.Modules.Identity.Impersonation;
using IAX.IXApi.Modules.Organization.Departments;
using IAX.IXApi.Modules.Organization.Genders;
using IAX.IXApi.Modules.Organization.Nationalities;
using IAX.IXApi.Modules.Organization.Occupations;
using IAX.IXApi.Modules.Organization.Employees;
using IAX.IXApi.Modules.Organization.ManagementLevels;
using IAX.IXApi.Modules.Organization.HcmWorkerManagers;
using IAX.IXApi.Modules.Organization.Showrooms;
using IAX.IXApi.Modules.Organization.Features.HcmWorkerCategory;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using System.Linq;

namespace IAX.IXApi.Infrastructure.Persistence.Seeding.Chunks
{
    public class OrganizationSeeder : ISeeder
    {
        public async Task SeedAsync(ApplicationDbContext db, RoleManager<AspNetRole> roles, UserManager<AspNetUser> users, CancellationToken ct)
        {
            var sysUser = await users.FindByNameAsync("sys");
            var createdBy = sysUser?.Id ?? "sys";

            #region Nationality
            if (!await db.Nationalities.IgnoreQueryFilters().AnyAsync(n => n.RecId == 1, ct))
            {
                var nationalities = new (short Id, string Name)[]
                {
                    (1, "سعودي"),
                    (2, "مصري"),
                    (3, "يمني"),
                    (4, "سوداني"),
                    (5, "سوري"),
                    (6, "اردني"),
                    (7, "فلسطيني"),
                    (8, "لبناني"),
                    (9, "بنجلاديشي"),
                    (10, "هندي"),
                    (11, "باكستاني"),
                    (12, "مغربي"),
                    (13, "تونسي"),
                    (14, "فلبيني"),
                    (15, "اندونيسي"),
                    (16, "ليبي"),
                    (17, "نيبالي"),
                    (18, "كويتي"),
                    (19, "اماراتي"),
                    (20, "عماني"),
                    (21, "بحريني"),
                    (22, "قطري"),
                    (23, "تشادي"),
                    (24, "ارتيري"),
                    (25, "موريتاني"),
                    (26, "صومالي"),
                    (27, "موريشيوسى"),
                    (28, "جزائري"),
                    (29, "قبائل نازحة"),
                    (30, "افغاني"),
                    (33, "تركي"),
                    (421, "أرجنتيني"),
                    (472, "سويدي"),
                };

                db.Nationalities.AddRange(nationalities.Select(n => new Nationality
                {
                    RecId = n.Id,
                    Code = "NAT" + n.Id,
                    Name = n.Name,
                    IsActive = true,
                    CreatedBy = createdBy,
                    OwnerAccountId = createdBy
                }));

                await db.Database.OpenConnectionAsync(ct);
                try
                {
                    await db.Database.ExecuteSqlRawAsync("SET IDENTITY_INSERT OrgNationalities ON", ct);
                    await db.SaveChangesAsync(ct);
                    await db.Database.ExecuteSqlRawAsync("SET IDENTITY_INSERT OrgNationalities OFF", ct);
                }
                finally
                {
                    await db.Database.CloseConnectionAsync();
                }
            }
            #endregion

            #region Department
            if (!await db.Departments.IgnoreQueryFilters().AnyAsync(d => d.RecId == 1, ct))
            {
                var departments = new (short Id, string Name)[]
                {
                    (1, "الإدارة العليا"),
                    (2, "الإدارة المالية"),
                    (3, "إدارة الموارد البشرية"),
                    (4, "إدارة تقنية المعلومات"),
                    (5, "إدارة اللوجيسيتك"),
                    (6, "إدارة التسويق"),
                    (7, "إدارة المستودعات"),
                    (31, "إدارة المشتريات"),
                    (41, "إدارة المبيعات - قطاع التجزئة"),
                    (42, "إدارة الوكالات"),
                    (43, "إدارة المعارض"),
                    (45, "إدارة الإنشاءات"),
                    (46, "إدارة الأقسام"),
                    (47, "فروع الوكالات"),
                    (48, "شئون الموظفين"),
                    (49, "المنهي خدماتهم"),
                    (50, "إدارة المشتريات_دوامين"),
                    (100, "درعة الاستثمارية-عقار"),
                    (101, "درعة الاسثمارية-منجرة"),
                    (102, "إدارة الشؤون القانونية"),
                    (103, "إدارة التدقيق والرقابة والجودة"),
                    (104, "إدارة مجمع قرية درعة السكني"),
                    (105, "إدارة التدريب والتطوير-موظف تحت التدريب"),
                    (106, "إدارة التدريب والتطوير"),
                };

                db.Departments.AddRange(departments.Select(d => new Department
                {
                    RecId = d.Id,
                    Code = "DEP" + d.Id,
                    Name = d.Name,
                    IsActive = true,
                    CreatedBy = createdBy,
                    OwnerAccountId = createdBy
                }));

                await db.Database.OpenConnectionAsync(ct);
                try
                {
                    await db.Database.ExecuteSqlRawAsync("SET IDENTITY_INSERT Departments ON", ct);
                    await db.SaveChangesAsync(ct);
                    await db.Database.ExecuteSqlRawAsync("SET IDENTITY_INSERT Departments OFF", ct);
                }
                finally
                {
                    await db.Database.CloseConnectionAsync();
                }
            }
            #endregion

            #region Occupation
            if (!await db.Occupations.IgnoreQueryFilters().AnyAsync(o => o.RecId == 1, ct))
            {
                var occupations = new (short Id, string Name)[]
                {
                    (1, "رئيس مجلس الإدارة"),
                    (2, "مدير ادارة"),
                    (3, "نائب مدير ادارة"),
                    (4, "مدير قسم"),
                    (5, "سكرتير"),
                    (6, "منسق"),
                    (22, "مدير مالي"),
                    (23, "رئيس حسابات"),
                    (24, "مسئول تطوير برامج"),
                    (25, "محاسب"),
                    (26, "مزارع"),
                    (41, "مراقب حركة المخزون"),
                    (42, "موظف جرد"),
                    (43, "موزع"),
                    (44, "امين مستودع"),
                    (46, "عامل مستودع"),
                    (48, "سائق"),
                    (49, "حارس"),
                    (60, "ميكانيكي"),
                    (64, "مشرف توطين"),
                    (70, "مندوب مبيعات"),
                    (82, "نائب مدير المعارض"),
                    (86, "كاشير"),
                    (87, "بائع مشترك"),
                    (88, "بائع وطني"),
                    (89, "بائعة"),
                    (90, "بائع من ذوي الاحتياجات الخاصة"),
                    (91, "اختصاصي تسويق"),
                    (92, "منشط مبيعات"),
                    (94, "مدير مبيعات-تجزئة"),
                    (101, "مشرف تسويق"),
                    (104, "مصمم"),
                    (105, "مراقب عام"),
                    (107, "مساعد مراقب نجار"),
                    (109, "مساعد حداد"),
                    (110, "مساعد نجار"),
                    (111, "سباك"),
                    (113, "مدير موقع"),
                    (121, "مبرمج حاسب آلي"),
                    (122, "فني صيانة"),
                    (123, "مدخل بيانات"),
                    (141, "مسؤول مشتريات"),
                    (145, "مندوب مشتريات خارجي"),
                    (146, "مندوب مشتريات داخلي"),
                    (161, "مدير مكتب"),
                    (162, "موظف اداري"),
                    (165, "عامل بوفية"),
                    (166, "عامل"),
                    (167, "معقب عام"),
                    (169, "مدير مشتريات"),
                    (171, "محلل بيانات"),
                    (172, "منسق إداري"),
                    (175, "مسئول أرشيف"),
                    (180, "مشرف حسابات"),
                    (181, "نائب الرئيس التنفيذي"),
                    (182, "الرئيس التنفيذي"),
                    (184, "نائب رئيس مجلس الإدارة"),
                    (185, "العضو المنتدب"),
                    (187, "مدير تدريب"),
                    (191, "فني تركيب"),
                    (193, "مشرف قسم"),
                    (196, "مشرف استئجار"),
                    (197, "مخلص جمركي"),
                    (198, "موظف استقبال"),
                    (199, "مدير العمليات"),
                    (201, "مدير منطقة"),
                    (202, "عامل بناء"),
                    (204, "مشرف انشاءات"),
                    (206, "مدير عام المشتريات الخارجية"),
                    (207, "نائب مدير عام المشتريات الخارجية"),
                    (208, "مدير عام مشتريات الأطقم والهدايا"),
                    (214, "مدير مشتريات الزيوت العطرية"),
                    (217, "نائب مدير قسم مشتريات التجميل"),
                    (228, "مصمم التجميل"),
                    (230, "مدير قسم العطور"),
                    (233, "نجار"),
                    (236, "مشرف قسم التخليص الجمركي"),
                    (250, "مشرف عمليات إدارة الموارد البشرية"),
                    (251, "مسئول توظيف"),
                    (252, "مدير استئجار"),
                    (253, "حداد"),
                    (254, "مبلط"),
                    (255, "دهان"),
                    (256, "مسئول علاقات إجتماعية"),
                    (257, "كهربائي"),
                    (258, "مدير البرامج والشبكات"),
                    (260, "رئيس حسابات التدقيق الداخلي"),
                    (261, "مشرف حسابات العمولات"),
                    (262, "مشرف الجرود"),
                    (267, "مدير الحسابات"),
                    (269, "محاسب بنوك"),
                    (270, "مشرف الترحيل"),
                    (271, "عامل تبريد وتكييف"),
                    (273, "فني تكييف وتبريد"),
                    (278, "مدرب ميداني"),
                    (280, "مشرف قسم المخزون"),
                    (286, "موظف تعبئة"),
                    (287, "سائق معدات ثقيلة"),
                    (288, "منسق عقود"),
                    (289, "مشرفة معارض-تجزئة"),
                    (291, "مراقب إنتاج"),
                    (298, "مشرف حسابات الخزينة والبنوك"),
                    (302, "مدير عمليات الاستئجار"),
                    (304, "مشرف معدات"),
                    (305, "مشرف مشتريات"),
                    (306, "مراقب عمال"),
                    (309, "رسام عام"),
                    (314, "فني ديكور"),
                    (315, "مسئول الأهداف الشهرية"),
                    (317, "مدير قسم التحليل والمتابعة"),
                    (318, "مسئول عرض"),
                    (319, "مدير قسم العناية بالجسم"),
                    (320, "موظف شحن"),
                    (321, "مصمم جرافيك"),
                    (322, "سائق بوكلين"),
                    (323, "معلم جبس"),
                    (324, "عامل فايبر جلاس"),
                    (326, "مدير قسم الهدايا"),
                    (328, "مفاوض"),
                    (329, "مساح"),
                    (333, "مدير قسم مشتريات الانشاءات"),
                    (334, "مساعد مدير مشتريات"),
                    (335, "سائق فوركلفت"),
                    (337, "مشرف قسم الإكسسوار"),
                    (339, "مراقب ميداني"),
                    (340, "مدير إدارة الاملاك"),
                    (342, "بنشري"),
                    (344, "منسق الخطة التشغيلية"),
                    (345, "منسق صيانة"),
                    (346, "مدير قسم العود"),
                    (347, "مدير تدريب ميداني"),
                    (348, "مهندس معماري"),
                    (349, "مهندس"),
                    (350, "مهندس  كهرباء"),
                    (351, "مهندس  ميكانيكا"),
                    (352, "مصمم معماري"),
                    (353, "مشغل ماكينة"),
                    (354, "موظف خدمة عملاء"),
                    (355, "مدير العرض الموحد"),
                    (356, "فني ابنية"),
                    (357, "مسئول شؤون قانونية"),
                    (358, "مساعد دهان"),
                    (362, "طباخ منزلي"),
                    (363, "سائق كرين"),
                    (364, "مهندس زراعي"),
                    (365, "حداد مسلح"),
                    (368, "مساعد مدير مبيعات"),
                    (369, "مدقق معاملات"),
                };

                db.Occupations.AddRange(occupations.Select(o => new Occupation
                {
                    RecId = o.Id,
                    Code = "OCC" + o.Id,
                    Name = o.Name,
                    IsActive = true,
                    CreatedBy = createdBy,
                    OwnerAccountId = createdBy
                }));

                await db.Database.OpenConnectionAsync(ct);
                try
                {
                    await db.Database.ExecuteSqlRawAsync("SET IDENTITY_INSERT Occupations ON", ct);
                    await db.SaveChangesAsync(ct);
                    await db.Database.ExecuteSqlRawAsync("SET IDENTITY_INSERT Occupations OFF", ct);
                }
                finally
                {
                    await db.Database.CloseConnectionAsync();
                }
            }
            #endregion

            #region Gender
            if (!await db.Genders.IgnoreQueryFilters().AnyAsync(g => g.RecId == 1, ct))
            {
                var genders = new[]
                {
                    new Gender { RecId = 1, Code = "M", Name = "Male", Description = null, IsActive = true, IsDeleted = false, CreatedBy = createdBy, OwnerAccountId = createdBy },
                    new Gender { RecId = 2, Code = "F", Name = "Female", Description = null, IsActive = true, IsDeleted = false, CreatedBy = createdBy, OwnerAccountId = createdBy }
                };

                await db.Genders.AddRangeAsync(genders, ct);

                await db.Database.OpenConnectionAsync(ct);
                try
                {
                    await db.Database.ExecuteSqlRawAsync("SET IDENTITY_INSERT Genders ON", ct);
                    await db.SaveChangesAsync(ct);
                    await db.Database.ExecuteSqlRawAsync("SET IDENTITY_INSERT Genders OFF", ct);
                }
                finally
                {
                    await db.Database.CloseConnectionAsync();
                }
            }
            #endregion

            #region ManagementLevel
            if (!await db.ManagementLevels.IgnoreQueryFilters().AnyAsync(m => m.RecId == 1, ct))
            {
                var levels = new[]
                {
                    new ManagementLevel { RecId = 1, Code = "ML1", Name = "Supervisor", Level = 1, IsActive = true, IsDeleted = false, CreatedBy = createdBy, OwnerAccountId = createdBy },
                    new ManagementLevel { RecId = 2, Code = "ML2", Name = "Area Manager", Level = 2, IsActive = true, IsDeleted = false, CreatedBy = createdBy, OwnerAccountId = createdBy },
                    new ManagementLevel { RecId = 3, Code = "ML3", Name = "Region Manager", Level = 3, IsActive = true, IsDeleted = false, CreatedBy = createdBy, OwnerAccountId = createdBy },
                    new ManagementLevel { RecId = 4, Code = "ML4", Name = "General Manager", Level = 4, IsActive = true, IsDeleted = false, CreatedBy = createdBy, OwnerAccountId = createdBy }
                };

                await db.ManagementLevels.AddRangeAsync(levels, ct);
                await db.SaveChangesAsync(ct);
            }
            #endregion


            #region Employee
            if (!await db.HcmWorkers.IgnoreQueryFilters().AnyAsync(x => x.RecId == 1, ct))
            {
                // FK targets reference the seeded legacy reference data by their explicit IDs:
                //   Dept 4 = إدارة تقنية المعلومات, Dept 1 = الإدارة العليا
                //   Occ 121 = مبرمج حاسب آلي, Occ 1 = رئيس مجلس الإدارة
                //   Gender 1 = ذكر, Nationality 1 = سعودي
                var employees = new[]
                {
                    new IAX.IXApi.Modules.Organization.Employees.Entities.HcmWorker
                    {
                        RecId = 1,
                        PersonnelNumber = "EMP001",
                        DepartmentId = 4,
                        OccupationId = 121,
                        GenderId = 1,
                        NationalityId = 1,
                        HireDate = DateTime.Parse("2020-01-01"),
                        BirthDate = DateTime.Parse("1990-01-01"),
                        IsActive = true,
                        IsDeleted = false,
                        CreatedBy = createdBy,
                        OwnerAccountId = createdBy
                    },
                    new IAX.IXApi.Modules.Organization.Employees.Entities.HcmWorker
                    {
                        RecId = 2,
                        PersonnelNumber = "EMP002",
                        DepartmentId = 1,
                        OccupationId = 1,
                        GenderId = 1,
                        NationalityId = 1,
                        HireDate = DateTime.Parse("2021-06-01"),
                        BirthDate = DateTime.Parse("1992-05-15"),
                        IsActive = true,
                        IsDeleted = false,
                        CreatedBy = createdBy,
                        OwnerAccountId = createdBy
                    }
                };

                await db.HcmWorkers.AddRangeAsync(employees, ct);

                await db.Database.OpenConnectionAsync(ct);
                try
                {
                    await db.Database.ExecuteSqlRawAsync("SET IDENTITY_INSERT HcmWorker ON", ct);
                    await db.SaveChangesAsync(ct);
                    await db.Database.ExecuteSqlRawAsync("SET IDENTITY_INSERT HcmWorker OFF", ct);
                }
                finally
                {
                    await db.Database.CloseConnectionAsync();
                }
            }
            #endregion

            #region Showroom
            if (!await db.Showrooms.IgnoreQueryFilters().AnyAsync(s => s.RecId == 101, ct))
            {
                var showrooms = new[]
                {
                    new Showroom
                    {
                        RecId = 101, Code = "SHR001", Name = "Riyadh Main Showroom", DepartmentId = 1, Location = "Riyadh - King Fahd Rd",
                        IsActive = true, IsDeleted = false, CreatedBy = createdBy, OwnerAccountId = createdBy
                    },
                    new Showroom
                    {
                        RecId = 102, Code = "SHR002", Name = "Jeddah Showroom", DepartmentId = 1, Location = "Jeddah - Tahlia St",
                        IsActive = true, IsDeleted = false, CreatedBy = createdBy, OwnerAccountId = createdBy
                    }
                };

                await db.Showrooms.AddRangeAsync(showrooms, ct);

                await db.Database.OpenConnectionAsync(ct);
                try
                {
                    await db.Database.ExecuteSqlRawAsync("SET IDENTITY_INSERT OrgEntities ON", ct);
                    await db.SaveChangesAsync(ct);
                    await db.Database.ExecuteSqlRawAsync("SET IDENTITY_INSERT OrgEntities OFF", ct);
                }
                finally
                {
                    await db.Database.CloseConnectionAsync();
                }
            }
            #endregion

            #region Sellers (employees assigned to showrooms)
            if (!await db.HcmWorkers.IgnoreQueryFilters().AnyAsync(x => x.RecId == 3, ct))
            {
                // Sellers belong to one showroom each (Employee.ShowroomId). Showrooms 101/102 seeded above.
                var sellers = new[]
                {
                    new IAX.IXApi.Modules.Organization.Employees.Entities.HcmWorker
                    {
                        RecId = 3, PersonnelNumber = "EMP003",
                        DepartmentId = 4, OccupationId = 121, GenderId = 1, NationalityId = 1,
                        HireDate = DateTime.Parse("2022-02-01"), BirthDate = DateTime.Parse("1995-03-10"),
                        ShowroomId = 101, IsActive = true, IsDeleted = false, CreatedBy = createdBy, OwnerAccountId = createdBy
                    },
                    new IAX.IXApi.Modules.Organization.Employees.Entities.HcmWorker
                    {
                        RecId = 4, PersonnelNumber = "EMP004",
                        DepartmentId = 4, OccupationId = 121, GenderId = 2, NationalityId = 1,
                        HireDate = DateTime.Parse("2022-08-15"), BirthDate = DateTime.Parse("1997-11-22"),
                        ShowroomId = 101, IsActive = true, IsDeleted = false, CreatedBy = createdBy, OwnerAccountId = createdBy
                    },
                    new IAX.IXApi.Modules.Organization.Employees.Entities.HcmWorker
                    {
                        RecId = 5, PersonnelNumber = "EMP005",
                        DepartmentId = 4, OccupationId = 121, GenderId = 1, NationalityId = 1,
                        HireDate = DateTime.Parse("2023-01-10"), BirthDate = DateTime.Parse("1996-07-05"),
                        ShowroomId = 102, IsActive = true, IsDeleted = false, CreatedBy = createdBy, OwnerAccountId = createdBy
                    }
                };

                await db.HcmWorkers.AddRangeAsync(sellers, ct);

                await db.Database.OpenConnectionAsync(ct);
                try
                {
                    await db.Database.ExecuteSqlRawAsync("SET IDENTITY_INSERT HcmWorker ON", ct);
                    await db.SaveChangesAsync(ct);
                    await db.Database.ExecuteSqlRawAsync("SET IDENTITY_INSERT HcmWorker OFF", ct);
                }
                finally
                {
                    await db.Database.CloseConnectionAsync();
                }
            }
            #endregion

            #region ManagerHierarchy (dynamic EmployeeManager assignments)
            if (!await db.HcmWorkerManagers.IgnoreQueryFilters().AnyAsync(m => m.EmployeeId == 1, ct))
            {
                // Levels seeded above: 1=Supervisor, 2=Area Manager, 3=Region Manager, 4=General Manager.
                var managerLinks = new[]
                {
                    // Admin (1) reports up to the General Manager (2).
                    new HcmWorkerManager { EmployeeId = 1, ManagementLevelId = 4, ManagerId = 2 },
                    // Khalid (3) is supervised by Admin (1) and ultimately the GM (2).
                    new HcmWorkerManager { EmployeeId = 3, ManagementLevelId = 1, ManagerId = 1 },
                    new HcmWorkerManager { EmployeeId = 3, ManagementLevelId = 4, ManagerId = 2 },
                    // Sara (4) and Faisal (5) are supervised by Khalid (3).
                    new HcmWorkerManager { EmployeeId = 4, ManagementLevelId = 1, ManagerId = 3 },
                    new HcmWorkerManager { EmployeeId = 5, ManagementLevelId = 1, ManagerId = 3 }
                };

                await db.HcmWorkerManagers.AddRangeAsync(managerLinks, ct);
                await db.SaveChangesAsync(ct);
            }
            #endregion

            #region OrganizationEntity user links
            // Link the existing admin accounts to their employee records, and add a showroom account
            // to demonstrate the polymorphic AspNetUser.OrganizationEntityId (employee OR showroom).
            await LinkUserToHcmWorkerAsync(db, users, "sys", 1, ct);
            await LinkUserToHcmWorkerAsync(db, users, "omar", 2, ct);

            var showroomUser = await users.FindByNameAsync("riyadh.showroom");
            if (showroomUser is null)
            {
                showroomUser = new AspNetUser
                {
                    Id = Guid.NewGuid().ToString(),
                    UserName = "riyadh.showroom",
                    Email = "riyadh.showroom@example.com",
                    EmailConfirmed = true,
                    OrganizationEntityId = 101, // linked to the Riyadh showroom
                };
                var res = await users.CreateAsync(showroomUser, "123");
                if (res.Succeeded)
                {
                    var role = await roles.FindByNameAsync("Admin");
                    if (role != null) await users.AddToRoleAsync(showroomUser, "Admin");
                }
            }
            #endregion

            #region EmployeeCategory
            if (!await db.HcmWorkerCategories.IgnoreQueryFilters().AnyAsync(c => c.RecId == 2, ct))
            {
                var categories = new (long RecId, string Name)[]
                {
                    (2, "الكل"),
                    (3, "الإدارة العليا"),
                    (4, "الإدارة المالية"),
                    (5, "إدارة الموارد البشرية"),
                    (6, "إدارة تقنية المعلومات"),
                    (7, "إدارة اللوجيسيتك"),
                    (8, "إدارة التسويق"),
                    (9, "إدارة المستودعات"),
                    (10, "إدارة المشتريات"),
                    (11, "إدارة المبيعات - قطاع التجزئة"),
                    (12, "إدارة الوكالات"),
                    (13, "إدارة المعارض"),
                    (14, "إدارة الإنشاءات"),
                    (15, "إدارة الأقسام"),
                    (16, "فروع الوكالات"),
                    (17, "شئون الموظفين"),
                    (18, "إدارة المشتريات_دوامين"),
                    (19, "درعة الاستثمارية-عقار"),
                    (20, "درعة الاسثمارية-منجرة"),
                    (21, "إدارة الشؤون القانونية"),
                    (22, "إدارة التدقيق والرقابة والجودة"),
                    (23, "إدارة مجمع قرية درعة السكني"),
                    (24, "اختصاصي تسويق"),
                    (25, "الرئيس التنفيذي"),
                    (26, "العضو المنتدب"),
                    (27, "امين مستودع"),
                    (28, "بائع مشترك"),
                    (29, "بائع من ذوي الاحتياجات الخاصة"),
                    (30, "بائع وطني"),
                    (31, "بائعة"),
                    (32, "بنشري"),
                    (33, "حارس"),
                    (34, "حداد"),
                    (35, "حداد مسلح"),
                    (36, "دهان"),
                    (37, "رسام عام"),
                    (38, "رئيس حسابات"),
                    (39, "رئيس حسابات التدقيق الداخلي"),
                    (40, "رئيس مجلس الإدارة"),
                    (41, "سائق"),
                    (42, "سائق بوكلين"),
                    (43, "سائق فوركلفت"),
                    (44, "سائق كرين"),
                    (45, "سائق معدات ثقيلة"),
                    (46, "سباك"),
                    (47, "سكرتير"),
                    (48, "طباخ منزلي"),
                    (49, "عامل"),
                    (50, "عامل بناء"),
                    (51, "عامل بوفية"),
                    (52, "عامل تبريد وتكييف"),
                    (53, "عامل فايبر جلاس"),
                    (54, "عامل مستودع"),
                    (55, "فني ابنية"),
                    (56, "فني تركيب"),
                    (57, "فني تكييف وتبريد"),
                    (58, "فني ديكور"),
                    (59, "فني صيانة"),
                    (60, "كاشير"),
                    (61, "كهربائي"),
                    (62, "مبرمج حاسب آلي"),
                    (63, "مبلط"),
                    (64, "محاسب"),
                    (65, "محاسب بنوك"),
                    (66, "محلل بيانات"),
                    (67, "مخلص جمركي"),
                    (68, "مدخل بيانات"),
                    (69, "مدرب ميداني"),
                    (70, "مدقق معاملات"),
                    (71, "مدير ادارة"),
                    (72, "مدير إدارة الاملاك"),
                    (73, "مدير استئجار"),
                    (74, "مدير البرامج والشبكات"),
                    (75, "مدير الحسابات"),
                    (76, "مدير العرض الموحد"),
                    (77, "مدير العمليات"),
                    (78, "مدير تدريب"),
                    (79, "مدير تدريب ميداني"),
                    (80, "مدير عام المشتريات الخارجية"),
                    (81, "مدير عام مشتريات الأطقم والهدايا"),
                    (82, "مدير عمليات الاستئجار"),
                    (83, "مدير قسم"),
                    (84, "مدير قسم التحليل والمتابعة"),
                    (85, "مدير قسم العطور"),
                    (86, "مدير قسم العناية بالجسم"),
                    (87, "مدير قسم العود"),
                    (88, "مدير قسم الهدايا"),
                    (89, "مدير قسم مشتريات الانشاءات"),
                    (90, "مدير مالي"),
                    (91, "مدير مبيعات-تجزئة"),
                    (92, "مدير مشتريات"),
                    (93, "مدير مشتريات الزيوت العطرية"),
                    (94, "مدير مكتب"),
                    (95, "مدير منطقة"),
                    (96, "مدير موقع"),
                    (97, "مراقب إنتاج"),
                    (98, "مراقب حركة المخزون"),
                    (99, "مراقب عام"),
                    (100, "مراقب عمال"),
                    (101, "مراقب ميداني"),
                    (102, "مزارع"),
                    (103, "مساح"),
                    (104, "مساعد حداد"),
                    (105, "مساعد دهان"),
                    (106, "مساعد مدير مبيعات"),
                    (107, "مساعد مدير مشتريات"),
                    (108, "مساعد مراقب نجار"),
                    (109, "مساعد نجار"),
                    (110, "مسؤول مشتريات"),
                    (111, "مسئول أرشيف"),
                    (112, "مسئول الأهداف الشهرية"),
                    (113, "مسئول تطوير برامج"),
                    (114, "مسئول توظيف"),
                    (115, "مسئول شؤون قانونية"),
                    (116, "مسئول عرض"),
                    (117, "مسئول علاقات إجتماعية"),
                    (118, "مشرف استئجار"),
                    (119, "مشرف الترحيل"),
                    (120, "مشرف الجرود"),
                    (121, "مشرف انشاءات"),
                    (122, "مشرف تسويق"),
                    (123, "مشرف توطين"),
                    (124, "مشرف حسابات"),
                    (125, "مشرف حسابات الخزينة والبنوك"),
                    (126, "مشرف حسابات العمولات"),
                    (127, "مشرف عمليات إدارة الموارد البشرية"),
                    (128, "مشرف قسم"),
                    (129, "مشرف قسم الإكسسوار"),
                    (130, "مشرف قسم التخليص الجمركي"),
                    (131, "مشرف قسم المخزون"),
                    (132, "مشرف مشتريات"),
                    (133, "مشرف معدات"),
                    (134, "مشرفة معارض-تجزئة"),
                    (135, "مشغل ماكينة"),
                    (136, "مصمم"),
                    (137, "مصمم التجميل"),
                    (138, "مصمم جرافيك"),
                    (139, "مصمم معماري"),
                    (140, "معقب عام"),
                    (141, "معلم جبس"),
                    (142, "مفاوض"),
                    (143, "مندوب مبيعات"),
                    (144, "مندوب مشتريات خارجي"),
                    (145, "مندوب مشتريات داخلي"),
                    (146, "منسق"),
                    (147, "منسق إداري"),
                    (148, "منسق الخطة التشغيلية"),
                    (149, "منسق صيانة"),
                    (150, "منسق عقود"),
                    (151, "منشط مبيعات"),
                    (152, "مهندس"),
                    (153, "مهندس  كهرباء"),
                    (154, "مهندس  ميكانيكا"),
                    (155, "مهندس زراعي"),
                    (156, "مهندس معماري"),
                    (157, "موزع"),
                    (158, "موظف اداري"),
                    (159, "موظف استقبال"),
                    (160, "موظف تعبئة"),
                    (161, "موظف جرد"),
                    (162, "موظف خدمة عملاء"),
                    (163, "موظف شحن"),
                    (164, "ميكانيكي"),
                    (165, "نائب الرئيس التنفيذي"),
                    (166, "نائب رئيس مجلس الإدارة"),
                    (167, "نائب مدير ادارة"),
                    (168, "نائب مدير المعارض"),
                    (169, "نائب مدير عام المشتريات الخارجية"),
                    (170, "نائب مدير قسم مشتريات التجميل"),
                    (171, "نجار"),
                    (172, "خارجي"),
                    (173, "مشرفين المعارض"),
                    (174, "مدراء المعارض"),
                    (175, "المدراء الاقليميين"),
                    (176, "المدير الاول"),
                    (177, "المدير الثاني"),
                    (178, "المدير الثالث"),
                    (179, "المدير الرابع"),
                    (180, "مجموعة مراجعة زيارة استلام معرض"),
                    (181, "قسم التصاميم"),
                    (182, "ادارة عملاء درعة"),
                    (183, "إدارة مصنع العود"),
                    (184, "زيارة الرقابة البيعية"),
                    (185, "زيارة الرقابة البيعية"),
                    (186, "موظفي ادارة التدريب والتطوير - اختبار"),
                    (187, "إدارة التدريب والتطوير"),
                    (189, "المدير الاول - الموظفين"),
                    (190, "المدير الثاني - الموظفين"),
                    (191, "المدير الثالث - الموظفين"),
                    (192, "المدير الرابع - الموظفين"),
                    (193, "الهيكل الوظيفي-موظفين"),
                    (194, "إدارة التجارة الإلكترونية"),
                    (196, "إدارة التخطيط الإستراتيجي"),
                    (197, "إدارة الإستئجار"),
                    (198, "الارشفة الالكترونية-منسقين-مجموعة IT"),
                    (199, "الارشفة الالكترونية-منسقين-مدير البرمجة"),
                    (200, "مراجعة زيارة تجربة"),
                    (201, "أمين مستودع الارشفة الالكترونية"),
                    (202, "أمين مستودع الارشفة الالكترونية"),
                    (203, "الارشفة الالكترونية-منسقين-الموارد البشرية"),
                    (204, "الارشفة الالكترونية-منسقين-الموارد البشرية"),
                    (205, "الارشفة الالكترونية-منسقين-ادارة الاستئجار"),
                    (206, "الارشفة الالكترونية-منسقين-ادارة الاستئجار"),
                    (207, "الارشفة الالكترونية-مطلعين-مدير الموارد البشرية"),
                    (208, "الارشفة الالكترونية-مطلعين-مدير الموارد البشرية"),
                    (209, "الارشفة الالكترونية - منسقين- المجمع السكني"),
                    (210, "الارشفة الالكترونية-منسقين- الجرد"),
                    (211, "الارشفة الالكترونية-منسقين-الإدارة المالية"),
                    (212, "الارشفة الالكترونية-مطلعين-الادارة المالية"),
                    (213, "الارشفة الالكترونية-مطلعين-الادارة المالية"),
                    (214, "الارشفة الالكترونية-مطلعين-الادارة المالية"),
                    (215, "الارشفة الالكترونية-مطلعين-الادارة المالية"),
                    (216, "الارشفة الالكترونية-مطلعين-ادارة الاستئجار"),
                    (217, "الارشفة الالكترونية-مطلعين-الجرد"),
                    (218, "الزيارات الميدانية-منسقين-مصنع ادوات التجميل"),
                    (219, "الزيارات الميدانية-منسقين-زيارة مستودعات التجميل"),
                    (220, "الارشفة الالكترونية-منسقين-ادارة الشئون القانونية"),
                    (221, "الزيارات الميدانية-منسقين-زيارة استلام ديكور"),
                    (222, "الزيارات الميدانية-منسقين-تدقيق مشرف معارض"),
                    (223, "الارشفة الالكترونية-منسقين-ادرة المشتريات"),
                    (224, "الزيارات الميدانية-منسقين-تقييم مشرف منطقة"),
                    (225, "الارشفة الالكترونية-منسقين-الادارة العليا"),
                    (226, "الارشفة الالكترونية-الادارة العليا-بيانات جمركية"),
                    (227, "منسقين-زيارة عمل صيانة دورية للمعارض"),
                    (228, "الزيارات الميدانية-منسقين-زيارة تنفيذ معرض"),
                    (229, "الارشفة الالكترونية-منسقين-معاملة التخليص الجمركي"),
                    (230, "الارشفة الالكترونية-منسقين-قيود مشتريات"),
                    (231, "الارشفة الالكترونية-منسقين-العقارية"),
                    (232, "تقييم الاداء الوظيفي فئة موظف"),
                    (233, "الزيارات الميدانية-منسقين-زيارة مدير منطقة لمشرف"),
                    (234, "الزيارات الميدانية-منسقين-برنامج فكرة"),
                    (235, "الزيارات الميدانية-منسقين-الرقابة البيعية"),
                    (236, "تدقيق مشرف صيانة"),
                    (237, "قائمة تفقد - الشقق السكنية"),
                    (238, "مراجعة قائمة تفقد - الشقق السكنية"),
                    (239, "منسقين صيانة  سكن"),
                    (240, "بلاغات الانشاءات"),
                    (241, "الارشفة الالكترونية-مطلعين-الادارة العليا"),
                    (242, "الارشفة الالكترونية-مطلعين-ادرة المشتريات"),
                    (243, "الارشفة الالكترونية-مطلعين-ادارة الشئون القانونية"),
                    (244, "الارشفة الالكترونية-مطلعين-عام"),
                    (245, "الزيارات الميدانية-منسقين-زيارة مختصرة"),
                    (246, "منسقين-نموذج فحص شهري للمركبات"),
                    (247, "مقدمين بلاغ -شكاوى المؤجرين"),
                    (248, "البلاغات والشكاوى-منسقين-ملاحظات الملاك"),
                    (249, "الارشفة الالكترونية-منسقين-ارشفة تقنية المعلومات"),
                    (250, "الزيارات الميدانية-منسقين-تسليم معرض للمبيعات"),
                    (251, "البلاغات والشكاوى-منسقين-شكاوى المؤجرين"),
                    (252, "المشاركين في زيارة تدقيق مشرف"),
                    (253, "البلاغات والشكاوى-مقدمين صيانة سكن"),
                    (254, "إدارة الشؤون الإدارية"),
                    (255, "الزيارات الميدانية-منسقين-تقييم بائع"),
                    (256, "الزيارات الميدانية-منسقين-مدير اقليم لمدير منطقة"),
                    (257, "مقدمين بلاغ صيانة معارض الحرمين"),
                    (258, "تقديم بلاغ صيانة سكن- ادارة"),
                    (259, "الزيارات الميدانية-منسقين-مراجعه حالة المعرض"),
                    (260, "البلاغات والشكاوى-منسقين-بلاغ العرض الموحد"),
                    (261, "البلاغات والشكاوى-مقدمين بلاغ العرض الموحد"),
                    (262, "مقدمين بلاغات جهات حكومية"),
                };

                db.HcmWorkerCategories.AddRange(categories.Select(c => new HcmWorkerCategory
                {
                    RecId = c.RecId,
                    Code = "UC" + c.RecId,
                    Name = c.Name,
                    ForAll = c.RecId == 2,
                    Manager1 = c.RecId == 176,
                    Manager2 = c.RecId == 177,
                    Manager3 = c.RecId == 178,
                    Manager4 = c.RecId == 179,
                    IsActive = true,
                    CreatedBy = createdBy,
                    OwnerAccountId = createdBy
                }));

                await db.Database.OpenConnectionAsync(ct);
                try
                {
                    await db.Database.ExecuteSqlRawAsync("SET IDENTITY_INSERT OrgEmployeeCategories ON", ct);
                    await db.SaveChangesAsync(ct);
                    await db.Database.ExecuteSqlRawAsync("SET IDENTITY_INSERT OrgEmployeeCategories OFF", ct);
                }
                finally
                {
                    await db.Database.CloseConnectionAsync();
                }
            }
            #endregion

            #region EmployeeCategoryGroup
            if (!await db.EmployeeCategoryGroups.IgnoreQueryFilters().AnyAsync(ct))
            {
                // (CategoriesGroupID, UserCategoriesID, DepartmentID, OccupationID, UserGroupID)
                var links = new (long RecId, long CategoryId, short? DepartmentId, short? OccupationId, long? UserGroupId)[]
                {
                    (1, 3, 1, null, null),
                    (2, 4, 2, null, null),
                    (3, 5, 3, null, null),
                    (4, 6, 4, null, null),
                    (5, 7, 5, null, null),
                    (6, 8, 6, null, null),
                    (7, 9, 7, null, null),
                    (8, 10, 31, null, null),
                    (9, 11, 41, null, null),
                    (10, 12, 42, null, null),
                    (11, 13, 43, null, null),
                    (12, 14, 45, null, null),
                    (13, 15, 46, null, null),
                    (14, 16, 47, null, null),
                    (15, 17, 48, null, null),
                    (16, 18, 50, null, null),
                    (17, 19, 100, null, null),
                    (18, 20, 101, null, null),
                    (19, 21, 102, null, null),
                    (20, 22, 103, null, null),
                    (21, 23, 104, null, null),
                    (22, 24, null, 91, null),
                    (23, 25, null, 182, null),
                    (24, 26, null, 185, null),
                    (25, 27, null, 44, null),
                    (26, 28, null, 87, null),
                    (27, 29, null, 90, null),
                    (28, 30, null, 88, null),
                    (29, 31, null, 89, null),
                    (30, 32, null, 342, null),
                    (31, 33, null, 49, null),
                    (32, 34, null, 253, null),
                    (33, 35, null, 365, null),
                    (34, 36, null, 255, null),
                    (35, 37, null, 309, null),
                    (36, 38, null, 23, null),
                    (37, 39, null, 260, null),
                    (38, 40, null, 1, null),
                    (39, 41, null, 48, null),
                    (40, 42, null, 322, null),
                    (41, 43, null, 335, null),
                    (42, 44, null, 363, null),
                    (43, 45, null, 287, null),
                    (44, 46, null, 111, null),
                    (45, 47, null, 5, null),
                    (46, 48, null, 362, null),
                    (47, 49, null, 166, null),
                    (48, 50, null, 202, null),
                    (49, 51, null, 165, null),
                    (50, 52, null, 271, null),
                    (51, 53, null, 324, null),
                    (52, 54, null, 46, null),
                    (53, 55, null, 356, null),
                    (54, 56, null, 191, null),
                    (55, 57, null, 273, null),
                    (56, 58, null, 314, null),
                    (57, 59, null, 122, null),
                    (58, 60, null, 86, null),
                    (59, 61, null, 257, null),
                    (60, 62, null, 121, null),
                    (61, 63, null, 254, null),
                    (62, 64, null, 25, null),
                    (63, 65, null, 269, null),
                    (64, 66, null, 171, null),
                    (65, 67, null, 197, null),
                    (66, 68, null, 123, null),
                    (67, 69, null, 278, null),
                    (68, 70, null, 369, null),
                    (69, 71, null, 2, null),
                    (70, 72, null, 340, null),
                    (71, 73, null, 252, null),
                    (72, 74, null, 258, null),
                    (73, 75, null, 267, null),
                    (74, 76, null, 355, null),
                    (75, 77, null, 199, null),
                    (76, 78, null, 187, null),
                    (77, 79, null, 347, null),
                    (78, 80, null, 206, null),
                    (79, 81, null, 208, null),
                    (80, 82, null, 302, null),
                    (81, 83, null, 4, null),
                    (82, 84, null, 317, null),
                    (83, 85, null, 230, null),
                    (84, 86, null, 319, null),
                    (85, 87, null, 346, null),
                    (86, 88, null, 326, null),
                    (87, 89, null, 333, null),
                    (88, 90, null, 22, null),
                    (89, 91, null, 94, null),
                    (90, 92, null, 169, null),
                    (91, 93, null, 214, null),
                    (92, 94, null, 161, null),
                    (93, 95, null, 201, null),
                    (94, 96, null, 113, null),
                    (95, 97, null, 291, null),
                    (96, 98, null, 41, null),
                    (97, 99, null, 105, null),
                    (98, 100, null, 306, null),
                    (99, 101, null, 339, null),
                    (100, 102, null, 26, null),
                    (101, 103, null, 329, null),
                    (102, 104, null, 109, null),
                    (103, 105, null, 358, null),
                    (104, 106, null, 368, null),
                    (105, 107, null, 334, null),
                    (106, 108, null, 107, null),
                    (107, 109, null, 110, null),
                    (108, 110, null, 141, null),
                    (109, 111, null, 175, null),
                    (110, 112, null, 315, null),
                    (111, 113, null, 24, null),
                    (112, 114, null, 251, null),
                    (113, 115, null, 357, null),
                    (114, 116, null, 318, null),
                    (115, 117, null, 256, null),
                    (116, 118, null, 196, null),
                    (117, 119, null, 270, null),
                    (118, 120, null, 262, null),
                    (119, 121, null, 204, null),
                    (120, 122, null, 101, null),
                    (121, 123, null, 64, null),
                    (122, 124, null, 180, null),
                    (123, 125, null, 298, null),
                    (124, 126, null, 261, null),
                    (125, 127, null, 250, null),
                    (126, 128, null, 193, null),
                    (127, 129, null, 337, null),
                    (128, 130, null, 236, null),
                    (129, 131, null, 280, null),
                    (130, 132, null, 305, null),
                    (131, 133, null, 304, null),
                    (132, 134, null, 289, null),
                    (133, 135, null, 353, null),
                    (134, 136, null, 104, null),
                    (135, 137, null, 228, null),
                    (136, 138, null, 321, null),
                    (137, 139, null, 352, null),
                    (138, 140, null, 167, null),
                    (139, 141, null, 323, null),
                    (140, 142, null, 328, null),
                    (141, 143, null, 70, null),
                    (142, 144, null, 145, null),
                    (143, 145, null, 146, null),
                    (144, 146, null, 6, null),
                    (145, 147, null, 172, null),
                    (146, 148, null, 344, null),
                    (147, 149, null, 345, null),
                    (148, 150, null, 288, null),
                    (149, 151, null, 92, null),
                    (150, 152, null, 349, null),
                    (151, 153, null, 350, null),
                    (152, 154, null, 351, null),
                    (153, 155, null, 364, null),
                    (154, 156, null, 348, null),
                    (155, 157, null, 43, null),
                    (156, 158, null, 162, null),
                    (157, 159, null, 198, null),
                    (158, 160, null, 286, null),
                    (159, 161, null, 42, null),
                    (160, 162, null, 354, null),
                    (161, 163, null, 320, null),
                    (162, 164, null, 60, null),
                    (163, 165, null, 181, null),
                    (164, 166, null, 184, null),
                    (165, 167, null, 3, null),
                    (166, 168, null, 82, null),
                    (167, 169, null, 207, null),
                    (168, 170, null, 217, null),
                    (169, 171, null, 233, null),
                    (170, 180, null, null, 116),
                    (171, 181, null, null, 123),
                    (176, 182, 108, null, null),
                    (177, 183, 107, null, null),
                    (178, 184, null, null, 129),
                    (179, 185, null, null, 129),
                    (180, 186, null, null, 137),
                    (181, 187, 106, null, null),
                    (182, 194, 109, null, null),
                    (183, 196, 110, null, null),
                    (184, 197, 111, null, null),
                    (185, 198, null, null, 168),
                    (186, 199, null, null, 169),
                    (187, 200, null, null, 174),
                    (188, 201, null, null, 175),
                    (189, 202, null, null, 175),
                    (190, 203, null, null, 178),
                    (191, 204, null, null, 178),
                    (192, 205, null, null, 180),
                    (193, 206, null, null, 180),
                    (194, 207, null, null, 181),
                    (195, 208, null, null, 181),
                    (196, 209, null, null, 179),
                    (197, 210, null, null, 182),
                    (198, 211, null, null, 183),
                    (199, 212, null, null, 187),
                    (200, 213, null, null, 187),
                    (201, 214, null, null, 187),
                    (202, 215, null, null, 187),
                    (203, 216, null, null, 188),
                    (204, 217, null, null, 190),
                    (205, 218, null, null, 209),
                    (206, 219, null, null, 210),
                    (207, 220, null, null, 224),
                    (208, 221, null, null, 230),
                    (209, 222, null, null, 238),
                    (210, 223, null, null, 240),
                    (211, 224, null, null, 253),
                    (212, 225, null, null, 262),
                    (213, 226, null, null, 263),
                    (214, 227, null, null, 264),
                    (215, 228, null, null, 266),
                    (216, 229, null, null, 267),
                    (217, 230, null, null, 280),
                    (218, 231, null, null, 281),
                    (219, 232, null, null, 284),
                    (220, 233, null, null, 294),
                    (221, 234, null, null, 297),
                    (222, 235, null, null, 309),
                    (223, 236, null, null, 298),
                    (224, 237, null, null, 310),
                    (225, 238, null, null, 312),
                    (226, 239, null, null, 311),
                    (227, 240, null, null, 315),
                    (228, 241, null, null, 317),
                    (229, 242, null, null, 318),
                    (230, 243, null, null, 319),
                    (231, 244, null, null, 320),
                    (232, 245, null, null, 321),
                    (233, 246, null, null, 323),
                    (234, 247, null, null, 325),
                    (235, 248, null, null, 326),
                    (236, 249, null, null, 327),
                    (237, 250, null, null, 329),
                    (238, 251, null, null, 325),
                    (239, 252, null, null, 332),
                    (240, 253, null, null, 334),
                    (241, 254, 113, null, null),
                    (242, 255, null, null, 335),
                    (243, 256, null, null, 345),
                    (244, 257, null, null, 346),
                    (245, 258, null, null, 347),
                    (246, 259, null, null, 349),
                    (247, 260, null, null, 348),
                    (248, 261, null, null, 350),
                    (249, 262, null, null, 351),
                };

                var deptIds = (await db.Departments.IgnoreQueryFilters().Select(d => d.RecId).ToListAsync(ct)).ToHashSet();
                var occIds = (await db.Occupations.IgnoreQueryFilters().Select(o => o.RecId).ToListAsync(ct)).ToHashSet();
                var catIds = (await db.HcmWorkerCategories.IgnoreQueryFilters().Select(c => c.RecId).ToListAsync(ct)).ToHashSet();
                var grpIds = (await db.HcmWorkerGroups.IgnoreQueryFilters().Select(g => g.RecId).ToListAsync(ct)).ToHashSet();

                var toInsert = links
                    .Where(l => catIds.Contains(l.CategoryId)
                        && (l.DepartmentId == null || deptIds.Contains(l.DepartmentId.Value))
                        && (l.OccupationId == null || occIds.Contains(l.OccupationId.Value))
                        && (l.UserGroupId == null || grpIds.Contains(l.UserGroupId.Value)))
                    .Select(l => new HcmWorkerCategoryGroup
                    {
                        RecId = l.RecId,
                        UserCategoriesID = l.CategoryId,
                        DepartmentID = l.DepartmentId,
                        OccupationID = l.OccupationId,
                        UserGroupID = l.UserGroupId,
                        IsActive = true,
                        CreatedBy = createdBy,
                        OwnerAccountId = createdBy
                    })
                    .ToList();

                if (toInsert.Count > 0)
                {
                    db.EmployeeCategoryGroups.AddRange(toInsert);

                    await db.Database.OpenConnectionAsync(ct);
                    try
                    {
                        await db.Database.ExecuteSqlRawAsync("SET IDENTITY_INSERT EmployeeCategoryGroups ON", ct);
                        await db.SaveChangesAsync(ct);
                        await db.Database.ExecuteSqlRawAsync("SET IDENTITY_INSERT EmployeeCategoryGroups OFF", ct);
                    }
                    finally
                    {
                        await db.Database.CloseConnectionAsync();
                    }
                }
            }
            #endregion
        }

        private static async Task LinkUserToHcmWorkerAsync(ApplicationDbContext db, UserManager<AspNetUser> users, string userName, long workerId, CancellationToken ct)
        {
            var user = await users.FindByNameAsync(userName);
            if (user != null)
            {
                var worker = await db.HcmWorkers.FindAsync(new object[] { workerId }, ct);
                if (worker != null && worker.UserId != user.Id)
                {
                    worker.UserId = user.Id;
                    await db.SaveChangesAsync(ct);
                }
            }
        }

        /// <summary>Sets a user's polymorphic OrganizationEntity link if it is not already assigned.</summary>
        private static async Task LinkUserToOrganizationEntityAsync(UserManager<AspNetUser> users, string userName, long orgEntityId, CancellationToken ct)
        {
            var user = await users.FindByNameAsync(userName);
            if (user != null && user.OrganizationEntityId != orgEntityId)
            {
                user.OrganizationEntityId = orgEntityId;
                await users.UpdateAsync(user);
            }
        }
    }
}



