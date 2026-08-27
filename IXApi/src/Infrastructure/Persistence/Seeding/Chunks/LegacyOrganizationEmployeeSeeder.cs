using System.Reflection;
using System.Text.Json;
using IAX.IXApi.Modules.Finance.Common;
using IAX.IXApi.Modules.Finance.Entities;
using IAX.IXApi.Modules.Identity.Roles;
using IAX.IXApi.Modules.Identity.Users;
using IAX.IXApi.Modules.Organization.Departments;
using IAX.IXApi.Modules.Organization.Employees.Entities;
using IAX.IXApi.Modules.Organization.Genders;
using IAX.IXApi.Modules.Organization.Nationalities;
using IAX.IXApi.Modules.Organization.Occupations;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using OrganizationGender = IAX.IXApi.Modules.Organization.Genders.Gender;

namespace IAX.IXApi.Infrastructure.Persistence.Seeding.Chunks;

/// <summary>All organization lookups and employees extracted from db_a8e163_aljazerasoftfp.</summary>
public sealed class LegacyOrganizationEmployeeSeeder : ISeeder
{
    private const string ResourceSuffix="Persistence.Seeding.Data.LegacyOrganizationEmployeeData.json";

    public async Task SeedAsync(ApplicationDbContext db,RoleManager<AspNetRole> roles,UserManager<AspNetUser> users,CancellationToken ct)
    {
        _=roles;
        var owner=(await users.FindByNameAsync("sys"))?.Id??"sys";
        var data=await ReadAsync(ct);
        await UpsertDepartmentsAsync(db,data.Departments,owner,ct);
        await UpsertOccupationsAsync(db,data.Occupations,owner,ct);
        await UpsertGendersAsync(db,data.Genders,owner,ct);
        await UpsertNationalitiesAsync(db,data.Nationalities,owner,ct);
        await UpsertEmployeesAsync(db,data.Employees,owner,ct);
    }

    private static async Task UpsertDepartmentsAsync(ApplicationDbContext db,LookupShort[] rows,string owner,CancellationToken ct)
    {
        var existing=await db.Departments.IgnoreQueryFilters().ToDictionaryAsync(x=>x.RecId,ct);
        foreach(var row in rows){if(existing.TryGetValue(row.Id,out var value)){Apply(value,row.Name,row.Description,row.Active);}else db.Departments.Add(new Department{RecId=row.Id,Code=$"DEP{row.Id}",Name=Text(row.Name,255),Description=Text(row.Description,1000),IsActive=row.Active,CreatedBy=owner,OwnerAccountId=owner});}
        await SaveWithOptionalIdentityAsync(db,"Departments",ct);
    }
    private static async Task UpsertOccupationsAsync(ApplicationDbContext db,LookupShort[] rows,string owner,CancellationToken ct)
    {
        var existing=await db.Occupations.IgnoreQueryFilters().ToDictionaryAsync(x=>x.RecId,ct);
        foreach(var row in rows){if(existing.TryGetValue(row.Id,out var value)){Apply(value,row.Name,row.Description,row.Active);}else db.Occupations.Add(new Occupation{RecId=row.Id,Code=$"OCC{row.Id}",Name=Text(row.Name,255),Description=Text(row.Description,1000),IsActive=row.Active,CreatedBy=owner,OwnerAccountId=owner});}
        await SaveWithOptionalIdentityAsync(db,"Occupations",ct);
    }
    private static async Task UpsertGendersAsync(ApplicationDbContext db,LookupByte[] rows,string owner,CancellationToken ct)
    {
        var existing=await db.Genders.IgnoreQueryFilters().ToDictionaryAsync(x=>x.RecId,ct);
        foreach(var row in rows){if(existing.TryGetValue(row.Id,out var value)){Apply(value,row.Name,row.Description,true);}else db.Genders.Add(new OrganizationGender{RecId=row.Id,Code=$"GEN{row.Id}",Name=Text(row.Name,255),Description=Text(row.Description,1000),IsActive=true,CreatedBy=owner,OwnerAccountId=owner});}
        await SaveWithOptionalIdentityAsync(db,"Genders",ct);
    }
    private static async Task UpsertNationalitiesAsync(ApplicationDbContext db,LookupShort[] rows,string owner,CancellationToken ct)
    {
        var existing=await db.Nationalities.IgnoreQueryFilters().ToDictionaryAsync(x=>x.RecId,ct);
        foreach(var row in rows){if(existing.TryGetValue(row.Id,out var value)){Apply(value,row.Name,row.Description,row.Active);}else db.Nationalities.Add(new Nationality{RecId=row.Id,Code=$"NAT{row.Id}",Name=Text(row.Name,255),Description=Text(row.Description,1000),IsActive=row.Active,CreatedBy=owner,OwnerAccountId=owner});}
        await SaveWithOptionalIdentityAsync(db,"OrgNationalities",ct);
    }

    private static async Task UpsertEmployeesAsync(ApplicationDbContext db,Employee[] rows,string owner,CancellationToken ct)
    {
        var duplicateCodes=rows.GroupBy(x=>x.Code??"",StringComparer.OrdinalIgnoreCase).Where(x=>x.Count()>1).Select(x=>x.Key).ToHashSet(StringComparer.OrdinalIgnoreCase);
        var parties=(await db.DirPartyTables.IgnoreQueryFilters().Where(x=>x.HcmWorker!=null).ToListAsync(ct)).GroupBy(x=>x.HcmWorker!.Value).ToDictionary(x=>x.Key,x=>x.First());
        foreach(var row in rows)
        {
            var code=EmployeeCode(row,duplicateCodes);
            if(parties.TryGetValue(row.Id,out var party)){party.PartyNumber=code;party.Name=Text(row.Name,255)??code;party.NameAlias=code;party.RFullName=Text(row.NameAr,255);party.HcmWorker=row.Id;party.IsActive=row.Active?NoYes.Yes:NoYes.No;}
            else{party=new DirPartyTable{PartyNumber=code,Name=Text(row.Name,255)??code,NameAlias=code,RFullName=Text(row.NameAr,255),LanguageId="ar-sa",AddressBookNames="",HcmWorker=row.Id,IsActive=row.Active?NoYes.Yes:NoYes.No,CreatedAt=row.CreatedAt,CreatedBy=row.CreatedBy??owner,OwnerAccountId=owner};db.DirPartyTables.Add(party);parties[row.Id]=party;}
        }
        await db.SaveChangesAsync(ct);

        var existing=await db.HcmWorkers.IgnoreQueryFilters().ToDictionaryAsync(x=>x.RecId,ct);
        foreach(var row in rows)
        {
            var code=EmployeeCode(row,duplicateCodes);
            if(existing.TryGetValue(row.Id,out var worker)){worker.PersonnelNumber=code;worker.Person=parties[row.Id].RecId;worker.DepartmentId=row.DepartmentId;worker.OccupationId=row.OccupationId;worker.GenderId=row.GenderId;worker.NationalityId=row.NationalityId;worker.IsActive=row.Active;worker.IsDeleted=false;}
            else db.HcmWorkers.Add(new HcmWorker{RecId=row.Id,PersonnelNumber=code,Person=parties[row.Id].RecId,DepartmentId=row.DepartmentId,OccupationId=row.OccupationId,GenderId=row.GenderId,NationalityId=row.NationalityId,IsActive=row.Active,CreatedAt=row.CreatedAt,CreatedBy=row.CreatedBy??owner,OwnerAccountId=owner});
        }
        await SaveWithOptionalIdentityAsync(db,"HcmWorker",ct);
    }

    private static void Apply<T>(IAX.IXApi.Shared.Domain.Entities.MasterEntity<T> target,string? name,string? description,bool active){target.Name=Text(name,255);target.Description=Text(description,1000);target.IsActive=active;target.IsDeleted=false;}
    private static string? Text(string? value,int max)=>string.IsNullOrWhiteSpace(value)?null:(value.Length<=max?value:value[..max]);
    private static string EmployeeCode(Employee row,HashSet<string> duplicates){var raw=string.IsNullOrWhiteSpace(row.Code)?$"EMP{row.Id}":row.Code.Trim();if(!duplicates.Contains(row.Code??""))return Text(raw,25)!;var suffix=$"-{row.Id}";return Text(raw,25-suffix.Length)!+suffix;}

    private static async Task SaveWithOptionalIdentityAsync(ApplicationDbContext db,string table,CancellationToken ct)
    {
        var hasAdded=db.ChangeTracker.Entries().Any(x=>x.State==EntityState.Added&&string.Equals(x.Metadata.GetTableName(),table,StringComparison.OrdinalIgnoreCase));
        if(!hasAdded){await db.SaveChangesAsync(ct);return;}
        await db.Database.OpenConnectionAsync(ct);
        try{await db.Database.ExecuteSqlRawAsync("SET IDENTITY_INSERT "+table+" ON",ct);await db.SaveChangesAsync(ct);await db.Database.ExecuteSqlRawAsync("SET IDENTITY_INSERT "+table+" OFF",ct);}
        finally{await db.Database.CloseConnectionAsync();}
    }

    private static async Task<Data> ReadAsync(CancellationToken ct){var assembly=typeof(LegacyOrganizationEmployeeSeeder).Assembly;var name=assembly.GetManifestResourceNames().Single(x=>x.EndsWith(ResourceSuffix,StringComparison.Ordinal));await using var stream=assembly.GetManifestResourceStream(name)??throw new InvalidOperationException($"Missing resource {name}");return await JsonSerializer.DeserializeAsync<Data>(stream,new JsonSerializerOptions{PropertyNameCaseInsensitive=true},ct)??throw new InvalidOperationException("Invalid organization employee seed resource.");}
    private sealed class Data{public LookupShort[] Departments{get;set;}=[];public LookupShort[] Occupations{get;set;}=[];public LookupByte[] Genders{get;set;}=[];public LookupShort[] Nationalities{get;set;}=[];public Employee[] Employees{get;set;}=[];}
    private sealed class LookupShort{public short Id{get;set;}public string? Name{get;set;}public string? Description{get;set;}public bool Active{get;set;}}
    private sealed class LookupByte{public byte Id{get;set;}public string? Name{get;set;}public string? Description{get;set;}}
    private sealed class Employee{public long Id{get;set;}public string? Code{get;set;}public string? Name{get;set;}public string? NameAr{get;set;}public short DepartmentId{get;set;}public short OccupationId{get;set;}public byte GenderId{get;set;}public short NationalityId{get;set;}public DateTime CreatedAt{get;set;}public string? CreatedBy{get;set;}public bool Active{get;set;}}
}
