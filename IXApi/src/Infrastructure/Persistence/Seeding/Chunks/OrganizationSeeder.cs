using IAX.IXApi.Infrastructure.Persistence;
using IAX.IXApi.Modules.Identity.Authentication;
using IAX.IXApi.Modules.Identity.Users;
using IAX.IXApi.Modules.Identity.Roles;
using IAX.IXApi.Modules.Identity.Impersonation;
using IAX.IXApi.Modules.Finance.Foundation.Genders;
using IAX.IXApi.Modules.Finance.Foundation.Nationalities;
using IAX.IXApi.Modules.Finance.Foundation.Occupations;
using IAX.IXApi.Modules.Finance.Foundation.HcmWorkers;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using System.Linq;

namespace IAX.IXApi.Infrastructure.Persistence.Seeding.Chunks
{
    /// <summary>
    /// Seeds the sanitized organization master data and employees imported from
    /// the supplied organization export.
    /// </summary>
    public sealed class OrganizationSeeder : ISeeder
    {
        private readonly OthersDBOrganizationEmployeeSeeder _importedDataSeeder;

        public OrganizationSeeder(string? seedDbConnectionString = null)
        {
            _importedDataSeeder = new OthersDBOrganizationEmployeeSeeder(seedDbConnectionString);
        }

        public Task SeedAsync(
            ApplicationDbContext db,
            RoleManager<AspNetRole> roles,
            UserManager<AspNetUser> users,
            CancellationToken ct) =>
            _importedDataSeeder.SeedAsync(db, roles, users, ct);
    }

    /// <summary>
    /// Retains the original synthetic organization dataset for explicit demo use.
    /// It is not part of the normal database seed pipeline.
    /// </summary>
    public class OrganizationDemoSeeder : ISeeder
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


            #region HcmWorker & Data Integrity
            // Fetch valid reference IDs for data integrity validation
            var validGenderIds = (await db.Genders.IgnoreQueryFilters().Select(g => g.RecId).ToListAsync(ct)).ToHashSet();
            var validNatIds = (await db.Nationalities.IgnoreQueryFilters().Select(n => n.RecId).ToListAsync(ct)).ToHashSet();

            byte defaultGenderId = validGenderIds.Contains(1) ? (byte)1 : validGenderIds.FirstOrDefault((byte)1);
            short defaultNatId = validNatIds.Contains(1) ? (short)1 : validNatIds.FirstOrDefault((short)1);

            // 1. If workers exist, enforce data integrity on existing records
            var existingWorkers = await db.HcmWorkers.IgnoreQueryFilters().ToListAsync(ct);
            if (existingWorkers.Count > 0)
            {
                bool modified = false;
                foreach (var worker in existingWorkers)
                {
                    if (worker.GenderId == 0 || !validGenderIds.Contains(worker.GenderId))
                    {
                        worker.GenderId = defaultGenderId;
                        modified = true;
                    }
                    if (worker.NationalityId == 0 || !validNatIds.Contains(worker.NationalityId))
                    {
                        worker.NationalityId = defaultNatId;
                        modified = true;
                    }
                }
                if (modified)
                {
                    await db.SaveChangesAsync(ct);
                }
            }

            // 2. Seed baseline workers if missing
            if (!await db.HcmWorkers.IgnoreQueryFilters().AnyAsync(x => x.RecId == 1, ct))
            {
                var workersToSeed = new[]
                {
                    new HcmWorker
                    {
                        RecId = 1, PersonnelNumber = "EMP001",
                        
                        GenderId = defaultGenderId,
                        HireDate = DateTime.Parse("2020-01-01"), BirthDate = DateTime.Parse("1990-01-01"),
                        IsActive = true, IsDeleted = false, CreatedBy = createdBy, OwnerAccountId = createdBy
                    },
                    new HcmWorker
                    {
                        RecId = 2, PersonnelNumber = "EMP002",
                     
                        GenderId = defaultGenderId,
                        HireDate = DateTime.Parse("2021-06-01"), BirthDate = DateTime.Parse("1992-05-15"),
                        IsActive = true, IsDeleted = false, CreatedBy = createdBy, OwnerAccountId = createdBy
                    },
                    new HcmWorker
                    {
                        RecId = 3, PersonnelNumber = "EMP003",
                        
                        GenderId = defaultGenderId,
                        HireDate = DateTime.Parse("2022-02-01"), BirthDate = DateTime.Parse("1995-03-10"),
                        IsActive = true, IsDeleted = false, CreatedBy = createdBy, OwnerAccountId = createdBy
                    },
                    new HcmWorker
                    {
                        RecId = 4, PersonnelNumber = "EMP004",
                        
                        GenderId = validGenderIds.Contains(2) ? (byte)2 : defaultGenderId,
                        HireDate = DateTime.Parse("2022-08-15"), BirthDate = DateTime.Parse("1997-11-22"),
                        IsActive = true, IsDeleted = false, CreatedBy = createdBy, OwnerAccountId = createdBy
                    },
                    new HcmWorker
                    {
                        RecId = 5, PersonnelNumber = "EMP005",
                      
                        GenderId = defaultGenderId,
                        HireDate = DateTime.Parse("2023-01-10"), BirthDate = DateTime.Parse("1996-07-05"),
                        IsActive = true, IsDeleted = false, CreatedBy = createdBy, OwnerAccountId = createdBy
                    }
                };

                await db.HcmWorkers.AddRangeAsync(workersToSeed, ct);
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

            

            #region OrganizationEntity user links
            await LinkUserToHcmWorkerAsync(db, users, "sys", 1, ct);
            await LinkUserToHcmWorkerAsync(db, users, "omar", 2, ct);
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
    }
}



