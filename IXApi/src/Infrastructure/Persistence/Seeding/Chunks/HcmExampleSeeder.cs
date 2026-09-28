using IAX.IXApi.Modules.Finance.Common;
using IAX.IXApi.Modules.Finance.Entities;
using IAX.IXApi.Modules.Finance.Foundation.Departments;
using IAX.IXApi.Modules.Finance.Foundation.HcmShowrooms;
using IAX.IXApi.Modules.Finance.Foundation.HcmWorkers;
using IAX.IXApi.Modules.Finance.Foundation.Nationalities;
using IAX.IXApi.Modules.Finance.Foundation.Occupations;
using IAX.IXApi.Modules.Finance.Foundation.WorkerOrganizationAssignments;
using IAX.IXApi.Modules.Finance.Foundation.WorkerShowroomAssignments;
using IAX.IXApi.Modules.Identity.Roles;
using IAX.IXApi.Modules.Identity.Users;
using IAX.IXApi.Shared.Domain.Entities;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using HcmGender = IAX.IXApi.Modules.Finance.Foundation.Genders.Gender;

namespace IAX.IXApi.Infrastructure.Persistence.Seeding.Chunks;

/// <summary>Company-scoped examples, separate from imported employee records.</summary>
public sealed class HcmExampleSeeder : ISeeder
{
    public const string Company = "dat";
    public static readonly DateOnly EffectiveFrom = new(2026, 1, 1);

    public Task SeedAsync(ApplicationDbContext db, RoleManager<AspNetRole> roles,
        UserManager<AspNetUser> users, CancellationToken ct) => SeedExamplesAsync(db, ct);

    public async Task SeedExamplesAsync(DbContext db, CancellationToken ct = default)
    {
        var sales = await LookupAsync<HcmDepartment, short>("DEMO-SALES", "Example Sales", "المبيعات التجريبية");
        var finance = await LookupAsync<HcmDepartment, short>("DEMO-FIN", "Example Finance", "المالية التجريبية");
        var gender = await LookupAsync<HcmGender, byte>("DEMO-MALE", "Male", "ذكر");
        var nationality = await LookupAsync<HcmNationality, short>("DEMO-SA", "Saudi", "سعودي");
        var seeds = new[]
        {
            (Code: "DEMO-DIRECTOR", Name: "Ahmed Al Harbi", Alias: "أحمد الحربي", Role: "DIRECTOR", Title: "General Manager", RoleAlias: "مدير عام", Department: sales, Manager: (string?)null),
            (Code: "DEMO-SALES-MGR", Name: "Khalid Al Ghamdi", Alias: "خالد الغامدي", Role: "SALES-MGR", Title: "Sales Manager", RoleAlias: "مدير مبيعات", Department: sales, Manager: (string?)"DEMO-DIRECTOR"),
            (Code: "DEMO-SUPERVISOR", Name: "Omar Al Zahrani", Alias: "عمر الزهراني", Role: "SUPERVISOR", Title: "Sales Supervisor", RoleAlias: "مشرف مبيعات", Department: sales, Manager: (string?)"DEMO-SALES-MGR"),
            (Code: "DEMO-SELLER-A", Name: "Faisal Al Qahtani", Alias: "فيصل القحطاني", Role: "SELLER", Title: "Seller", RoleAlias: "بائع", Department: sales, Manager: (string?)"DEMO-SUPERVISOR"),
            (Code: "DEMO-SELLER-B", Name: "Saad Al Otaibi", Alias: "سعد العتيبي", Role: "SELLER", Title: "Seller", RoleAlias: "بائع", Department: sales, Manager: (string?)"DEMO-SUPERVISOR"),
            (Code: "DEMO-FIN-MGR", Name: "Abdullah Al Dosari", Alias: "عبدالله الدوسري", Role: "FIN-MGR", Title: "Finance Manager", RoleAlias: "مدير مالي", Department: finance, Manager: (string?)"DEMO-DIRECTOR"),
            (Code: "DEMO-ACCOUNTANT", Name: "Yousef Al Shammari", Alias: "يوسف الشمري", Role: "ACCOUNTANT", Title: "Accountant", RoleAlias: "محاسب", Department: finance, Manager: (string?)"DEMO-FIN-MGR")
        };
        var workers = new Dictionary<string, HcmWorker>();
        var occupations = new Dictionary<string, HcmOccupation>();
        foreach (var seed in seeds)
        {
            var occupation = await LookupAsync<HcmOccupation, short>("DEMO-" + seed.Role, seed.Title, seed.RoleAlias);
            occupations[seed.Code] = occupation;
            var worker = await db.Set<HcmWorker>().IgnoreQueryFilters()
                .SingleOrDefaultAsync(x => x.DataAreaId == Company && x.PersonnelNumber == seed.Code, ct);
            if (worker == null)
            {
                var party = await PartyAsync(seed.Code, seed.Name, seed.Alias);
                worker = new HcmWorker
                {
                    DataAreaId = Company, PersonnelNumber = seed.Code, Person = party.RecId,
                    GenderId = gender.RecId, NationalityId = nationality.RecId,
                    HireDate = EffectiveFrom.ToDateTime(TimeOnly.MinValue)
                };
                db.Add(worker);
                await db.SaveChangesAsync(ct);
            }
            workers[seed.Code] = worker;
        }

        // The root has no manager; the required V1 manager FK must never point to itself.
        foreach (var seed in seeds.Where(x => x.Manager != null))
        {
            var worker = workers[seed.Code];
            var manager = workers[seed.Manager!];
            var occupation = occupations[seed.Code];
            if (!Available(worker) || !Available(manager) || !Available(seed.Department) || !Available(occupation)) continue;
            // Existing assignment history, including closed/deleted rows, remains user-owned.
            if (await db.Set<HcmWorkerOrganizationAssignmentV1>().IgnoreQueryFilters()
                .AnyAsync(x => x.DataAreaId == Company && x.HcmWorkerId == worker.RecId, ct)) continue;
            db.Add(new HcmWorkerOrganizationAssignmentV1
            {
                DataAreaId = Company, HcmWorkerId = worker.RecId, HcmManagerWorkerId = manager.RecId,
                DepartmentId = seed.Department.RecId, OccupationId = occupation.RecId,
                ValidFrom = EffectiveFrom, IsPrimary = true
            });
        }
        await db.SaveChangesAsync(ct);

        foreach (var suffix in new[] { "A", "B" })
        {
            var party = await PartyAsync("DEMO-SHOWROOM-" + suffix, "Example Showroom " + suffix,
                suffix == "A" ? "المعرض التجريبي أ" : "المعرض التجريبي ب");
            var showroom = await db.Set<HcmShowroom>().IgnoreQueryFilters()
                .SingleOrDefaultAsync(x => x.DataAreaId == Company && x.Party == party.RecId, ct);
            if (showroom == null)
            {
                showroom = new HcmShowroom
                {
                    DataAreaId = Company,
                    Party = party.RecId,
                    PersonnelNumber = "DEMO-SHOWROOM-" + suffix
                };
                db.Add(showroom);
                await db.SaveChangesAsync(ct);
            }
            else if (string.IsNullOrWhiteSpace(showroom.PersonnelNumber))
            {
                showroom.PersonnelNumber = "DEMO-SHOWROOM-" + suffix;
                await db.SaveChangesAsync(ct);
            }
            var worker = workers["DEMO-SELLER-" + suffix];
            if (!Available(worker) || !Available(showroom) || party.IsDeleted || party.IsActive == NoYes.No) continue;
            if (await db.Set<HcmWorkerShowroomAssignment>().IgnoreQueryFilters()
                .AnyAsync(x => x.DataAreaId == Company && x.HcmWorkerId == worker.RecId, ct)) continue;
            db.Add(new HcmWorkerShowroomAssignment
            {
                DataAreaId = Company, HcmWorkerId = worker.RecId, HcmShowroomId = showroom.RecId,
                ValidFrom = EffectiveFrom, IsPrimary = true
            });
        }
        await db.SaveChangesAsync(ct);

        async Task<T> LookupAsync<T, TKey>(string code, string name, string alias) where T : MasterEntity<TKey>, new()
        {
            var entity = await db.Set<T>().IgnoreQueryFilters()
                .SingleOrDefaultAsync(x => x.DataAreaId == Company && x.Code == code, ct);
            if (entity == null)
            {
                entity = new T { DataAreaId = Company, Code = code, Name = name, NameAlias = alias };
                db.Add(entity);
            }
            else if (string.IsNullOrWhiteSpace(entity.NameAlias)) entity.NameAlias = alias;
            await db.SaveChangesAsync(ct);
            return entity;
        }

        async Task<DirPartyTable> PartyAsync(string code, string name, string alias)
        {
            var party = await db.Set<DirPartyTable>().IgnoreQueryFilters().SingleOrDefaultAsync(x => x.PartyNumber == code, ct);
            if (party != null && party.DataAreaId != Company)
                throw new InvalidOperationException($"Example party number '{code}' belongs to another company.");
            if (party == null)
            {
                party = new DirPartyTable
                {
                    DataAreaId = Company, PartyNumber = code, Name = name, NameAlias = alias,
                    LanguageId = "ar-sa", AddressBookNames = "", IsActive = NoYes.Yes
                };
                db.Add(party);
            }
            else if (string.IsNullOrWhiteSpace(party.NameAlias)) party.NameAlias = alias;
            await db.SaveChangesAsync(ct);
            return party;
        }
    }

    private static bool Available<TKey>(BaseEntity<TKey> entity) => entity.IsActive && !entity.IsDeleted;
}
