using IAX.IXApi.Infrastructure.Persistence.SeedingERPV2.Core.Contracts;
using IAX.IXApi.Infrastructure.Persistence.SeedingERPV2.Core.Execution;
using IAX.IXApi.Infrastructure.Persistence.SeedingERPV2.Core.Models;
using IAX.IXApi.Modules.Finance.Common;
using IAX.IXApi.Modules.Finance.Entities;
using IAX.IXApi.Modules.Finance.Foundation.Departments;
using IAX.IXApi.Modules.Finance.Foundation.Genders;
using IAX.IXApi.Modules.Finance.Foundation.HcmWorkers;
using IAX.IXApi.Modules.Finance.Foundation.Nationalities;
using IAX.IXApi.Modules.Finance.Foundation.Occupations;
using IAX.IXApi.Shared.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace IAX.IXApi.Infrastructure.Persistence.SeedingERPV2.Modules.Foundation;

/// <summary>
/// One-time import of the legacy organization: departments, occupations, genders, nationalities and employees,
/// each keeping its source id so references from other imported data stay valid. An existing row with the
/// same id takes the source values, as an import should. Every employee gets a party and a worker; a
/// duplicated employee code is made unique with the employee id.
/// </summary>
public sealed class LegacyEmployeeImportSeeder(LegacyImportSource source) : IBaselineSeeder
{
    public const string SeederId = "Finance.Foundation.LegacyEmployees";
    public string Id => SeederId;
    public IReadOnlyCollection<string> Dependencies => [BaselineCompanySeeder.SeederId];
    public BaselineSeederKind Kind => BaselineSeederKind.LegacyImport;

    private const string Query = """
        SELECT
          JSON_QUERY((SELECT DepartmentId AS Id, COALESCE(NULLIF(DepartmentName, N''), DepartmentNameAR) AS Name, DepartmentNameAR AS NameAr,
                              COALESCE(DescriptionAR, Description) AS Description, Activated AS Active FROM dbo.Departments FOR JSON PATH)) AS Departments,
          JSON_QUERY((SELECT OccupationId AS Id, COALESCE(NULLIF(OccupationName, N''), OccupationNameAR) AS Name, OccupationNameAR AS NameAr,
                              COALESCE(DescriptionAR, Description) AS Description, Activated AS Active FROM dbo.Occupations FOR JSON PATH)) AS Occupations,
          JSON_QUERY((SELECT GenderId AS Id, COALESCE(NULLIF(GenderNameAR, N''), GenderName) AS Name,
                              COALESCE(DescriptionAR, Description) AS Description FROM dbo.Genders FOR JSON PATH)) AS Genders,
          JSON_QUERY((SELECT NationalityId AS Id, COALESCE(NULLIF(NationalityNameAR, N''), NationalityName) AS Name,
                              COALESCE(DescriptionAR, Description) AS Description, Activated AS Active FROM dbo.Nationalities FOR JSON PATH)) AS Nationalities,
          JSON_QUERY((SELECT EmployeeId AS Id, EmployeeCode AS Code, EmployeeName AS Name, EmployeeNameAR AS NameAr,
                              DepartmentId, OccupationId, GenderId, NationalityId, CreatedDate AS CreatedAt,
                              CONVERT(nvarchar(450), CreatedBy) AS CreatedBy, Activated AS Active FROM dbo.Employees FOR JSON PATH)) AS Employees
        FOR JSON PATH, WITHOUT_ARRAY_WRAPPER;
        """;

    public async Task SeedAsync(SeedContext c, CancellationToken ct)
    {
        var data = await source.ReadAsync<Export>(Query, "legacy-employees.json", ct);
        await LookupsAsync(c, data.Departments, x => x.RecId, (row, id) => new HcmDepartment { RecId = id, Code = $"DEP{id}", NameAlias = Text(row.NameAr ?? row.Name, 60) },
            (target, row) => { if (string.IsNullOrWhiteSpace(target.NameAlias)) target.NameAlias = Text(row.NameAr ?? row.Name, 60); }, ct);
        await LookupsAsync(c, data.Occupations, x => x.RecId, (row, id) => new HcmOccupation { RecId = id, Code = $"OCC{id}", NameAlias = Text(row.NameAr ?? row.Name, 60) },
            (target, row) => { if (string.IsNullOrWhiteSpace(target.NameAlias)) target.NameAlias = Text(row.NameAr ?? row.Name, 60); }, ct);
        await LookupsAsync(c, data.Genders.Select(x => x with { Active = true }).ToArray(), x => x.RecId,
            (row, id) => new IAX.IXApi.Modules.Finance.Foundation.Genders.Gender { RecId = (byte)id, Code = $"GEN{id}" }, (_, _) => { }, ct);
        await LookupsAsync(c, data.Nationalities, x => x.RecId, (row, id) => new HcmNationality { RecId = id, Code = $"NAT{id}" }, (_, _) => { }, ct);
        await EmployeesAsync(c, data.Employees, ct);
    }

    /// <summary>Inserts a lookup under its source id, or refreshes the name, description and status of the existing one.</summary>
    private static async Task LookupsAsync<T, TKey>(SeedContext c, Lookup[] rows, Func<T, TKey> key, Func<Lookup, short, T> create,
        Action<T, Lookup> refresh, CancellationToken ct) where T : MasterEntity<TKey> where TKey : struct
    {
        var existing = (await c.Db.Set<T>().IgnoreQueryFilters().ToListAsync(ct)).ToDictionary(x => Convert.ToInt64(key(x)));
        foreach (var row in rows)
        {
            if (!existing.TryGetValue(row.Id, out var target))
            {
                target = create(row, row.Id);
                target.CreatedBy = c.Actor;
                target.OwnerAccountId = c.Actor;
                c.Db.Set<T>().Add(target);
            }
            else refresh(target, row);
            target.Name = Text(row.Name, 255);
            target.Description = Text(row.Description, 1000);
            target.IsActive = row.Active;
            target.IsDeleted = false;
        }
        await SeedIdentityWriter.SaveAsync<T>(c, ct);
    }

    private static async Task EmployeesAsync(SeedContext c, Employee[] rows, CancellationToken ct)
    {
        var duplicates = rows.GroupBy(x => x.Code ?? "", StringComparer.OrdinalIgnoreCase).Where(x => x.Count() > 1)
            .Select(x => x.Key).ToHashSet(StringComparer.OrdinalIgnoreCase);
        var workers = await c.Db.Set<HcmWorker>().IgnoreQueryFilters().ToDictionaryAsync(x => x.RecId, ct);
        var allParties = await c.Db.Set<DirPartyTable>().IgnoreQueryFilters().ToListAsync(ct);
        var parties = allParties.Where(x => x.HcmWorker != null).GroupBy(x => x.HcmWorker!.Value).ToDictionary(x => x.Key, x => x.First());
        var partiesById = allParties.ToDictionary(x => x.RecId);

        foreach (var row in rows)
        {
            var code = EmployeeCode(row, duplicates);
            if (!parties.TryGetValue(row.Id, out var party) && workers.TryGetValue(row.Id, out var linked))
                partiesById.TryGetValue(linked.Person, out party);
            if (party == null)
            {
                party = new DirPartyTable
                {
                    NameAlias = Text(row.NameAr, 60) ?? code, LanguageId = "ar-sa", AddressBookNames = "",
                    CreatedAt = row.CreatedAt, CreatedBy = row.CreatedBy ?? c.Actor, OwnerAccountId = c.Actor
                };
                c.Db.Add(party);
            }
            else if (string.IsNullOrWhiteSpace(party.NameAlias)) party.NameAlias = Text(row.NameAr, 60) ?? code;
            party.PartyNumber = code;
            party.Name = Text(row.Name, 255) ?? code;
            party.RFullName = Text(row.NameAr, 255);
            party.HcmWorker = row.Id;
            party.IsActive = row.Active ? NoYes.Yes : NoYes.No;
            parties[row.Id] = party;
        }
        await c.Db.SaveChangesAsync(ct);

        foreach (var row in rows)
        {
            if (!workers.TryGetValue(row.Id, out var worker))
            {
                worker = new HcmWorker { RecId = row.Id, CreatedAt = row.CreatedAt, CreatedBy = row.CreatedBy ?? c.Actor, OwnerAccountId = c.Actor };
                c.Db.Add(worker);
            }
            worker.PersonnelNumber = EmployeeCode(row, duplicates);
            worker.Person = parties[row.Id].RecId;
            worker.GenderId = row.GenderId;
            worker.NationalityId = row.NationalityId;
            worker.IsActive = row.Active;
            worker.IsDeleted = false;
        }
        await SeedIdentityWriter.SaveAsync<HcmWorker>(c, ct);
    }

    private static string? Text(string? value, int max) => string.IsNullOrWhiteSpace(value) ? null : value.Length <= max ? value : value[..max];

    private static string EmployeeCode(Employee row, HashSet<string> duplicates)
    {
        var raw = string.IsNullOrWhiteSpace(row.Code) ? $"EMP{row.Id}" : row.Code.Trim();
        if (!duplicates.Contains(row.Code ?? "")) return Text(raw, 25)!;
        var suffix = $"-{row.Id}";
        return Text(raw, 25 - suffix.Length)! + suffix;
    }

    private sealed record Export(Lookup[] Departments, Lookup[] Occupations, Lookup[] Genders, Lookup[] Nationalities, Employee[] Employees);
    private sealed record Lookup(short Id, string? Name, string? NameAr, string? Description, bool Active);
    private sealed record Employee(long Id, string? Code, string? Name, string? NameAr, short DepartmentId, short OccupationId, byte GenderId,
        short NationalityId, DateTime CreatedAt, string? CreatedBy, bool Active);
}
