using DocumentFormat.OpenXml.Spreadsheet;
using IAX.IXApi.Modules.Finance.Common;
using IAX.IXApi.Modules.Finance.Entities;
using IAX.IXApi.Modules.Finance.Foundation.Genders;
using IAX.IXApi.Modules.Finance.Foundation.HcmWorkers;
using IAX.IXApi.Modules.Finance.Foundation.Nationalities;
using IAX.IXApi.Modules.Finance.Foundation.Occupations;
using IAX.IXApi.Modules.Finance.Foundation.Departments;
using IAX.IXApi.Modules.Identity.Roles;
using IAX.IXApi.Modules.Identity.Users;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using System.Reflection;
using System.Text.Json;

namespace IAX.IXApi.Infrastructure.Persistence.Seeding.Chunks;

/// <summary>
/// Imports the sanitized organization export embedded with the application.
/// Authentication credentials and personal contact/identity fields are intentionally excluded.
/// </summary>
public sealed class OthersDBOrganizationEmployeeSeeder : OthersDBSeedData
{
    private const string ResourceSuffix="Persistence.Seeding.Data.LegacyOrganizationEmployeeData.json";

    public OthersDBOrganizationEmployeeSeeder(string? seedDbConnectionString = null)
        : base(seedDbConnectionString)
    {
    }

    public override async Task SeedAsync(ApplicationDbContext db,RoleManager<AspNetRole> roles,UserManager<AspNetUser> users,CancellationToken ct)
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
        var existing=await db.HcmDepartments.IgnoreQueryFilters().ToDictionaryAsync(x=>x.RecId,ct);
        foreach(var row in rows){if(existing.TryGetValue(row.Id,out var value)){Apply(value,row.Name,row.Description,row.Active);}else db.HcmDepartments.Add(new HcmDepartment{RecId=row.Id,Code=$"DEP{row.Id}",Name=Text(row.Name,255),Description=Text(row.Description,1000),IsActive=row.Active,CreatedBy=owner,OwnerAccountId=owner});}
        await SaveWithOptionalIdentityAsync(db,"HcmDepartments",ct);
    }

    private static async Task UpsertOccupationsAsync(ApplicationDbContext db,LookupShort[] rows,string owner,CancellationToken ct)
    {
        var existing=await db.HcmOccupations.IgnoreQueryFilters().ToDictionaryAsync(x=>x.RecId,ct);
        foreach(var row in rows){if(existing.TryGetValue(row.Id,out var value)){Apply(value,row.Name,row.Description,row.Active);}else db.HcmOccupations.Add(new HcmOccupation{RecId=row.Id,Code=$"OCC{row.Id}",Name=Text(row.Name,255),Description=Text(row.Description,1000),IsActive=row.Active,CreatedBy=owner,OwnerAccountId=owner});}
        await SaveWithOptionalIdentityAsync(db,"HcmOccupations",ct);
    }

    private static async Task UpsertGendersAsync(ApplicationDbContext db,LookupByte[] rows,string owner,CancellationToken ct)
    {
        var existing=await db.Genders.IgnoreQueryFilters().ToDictionaryAsync(x=>x.RecId,ct);
        foreach(var row in rows){if(existing.TryGetValue(row.Id,out var value)){Apply(value,row.Name,row.Description,true);}else db.Genders.Add(new Modules.Finance.Foundation.Genders.Gender { RecId=row.Id,Code=$"GEN{row.Id}",Name=Text(row.Name,255),Description=Text(row.Description,1000),IsActive=true,CreatedBy=owner,OwnerAccountId=owner});}
        await SaveWithOptionalIdentityAsync(db,"Genders",ct);
    }

    private static async Task UpsertNationalitiesAsync(ApplicationDbContext db,LookupShort[] rows,string owner,CancellationToken ct)
    {
        var existing=await db.HcmNationalities.IgnoreQueryFilters().ToDictionaryAsync(x=>x.RecId,ct);
        foreach(var row in rows){if(existing.TryGetValue(row.Id,out var value)){Apply(value,row.Name,row.Description,row.Active);}else db.HcmNationalities.Add(new HcmNationality{RecId=row.Id,Code=$"NAT{row.Id}",Name=Text(row.Name,255),Description=Text(row.Description,1000),IsActive=row.Active,CreatedBy=owner,OwnerAccountId=owner});}
        await SaveWithOptionalIdentityAsync(db,"HcmNationalities",ct);
    }

    private static async Task UpsertEmployeesAsync(ApplicationDbContext db,Employee[] rows,string owner,CancellationToken ct)
    {
        var duplicateCodes=rows.GroupBy(x=>x.Code??"",StringComparer.OrdinalIgnoreCase).Where(x=>x.Count()>1).Select(x=>x.Key).ToHashSet(StringComparer.OrdinalIgnoreCase);
        var existingWorkers=await db.HcmWorkers.IgnoreQueryFilters().ToDictionaryAsync(x=>x.RecId,ct);
        var allParties=await db.DirPartyTables.IgnoreQueryFilters().ToListAsync(ct);
        var parties=allParties.Where(x=>x.HcmWorker!=null).GroupBy(x=>x.HcmWorker!.Value).ToDictionary(x=>x.Key,x=>x.First());
        var partiesById=allParties.ToDictionary(x=>x.RecId);
        foreach(var row in rows)
        {
            var code=EmployeeCode(row,duplicateCodes);
            var hasParty=parties.TryGetValue(row.Id,out var party);
            if(!hasParty&&existingWorkers.TryGetValue(row.Id,out var linkedWorker))hasParty=partiesById.TryGetValue(linkedWorker.Person,out party);
            if(hasParty){party!.PartyNumber=code;party.Name=Text(row.Name,255)??code;party.NameAlias=Text(row.NameAr,60)??code;party.RFullName=Text(row.NameAr,255);party.HcmWorker=row.Id;party.IsActive=row.Active?NoYes.Yes:NoYes.No;}
            else{party=new DirPartyTable{PartyNumber=code,Name=Text(row.Name,255)??code,NameAlias=Text(row.NameAr,60)??code,RFullName=Text(row.NameAr,255),LanguageId="ar-sa",AddressBookNames="",HcmWorker=row.Id,IsActive=row.Active?NoYes.Yes:NoYes.No,CreatedAt=row.CreatedAt,CreatedBy=row.CreatedBy??owner,OwnerAccountId=owner};db.DirPartyTables.Add(party);parties[row.Id]=party;}
        }
        await db.SaveChangesAsync(ct);

        foreach(var row in rows)
        {
            var code=EmployeeCode(row,duplicateCodes);
            if(existingWorkers.TryGetValue(row.Id,out var worker)){worker.PersonnelNumber=code;worker.Person=parties[row.Id].RecId;worker.OccupationId=row.OccupationId;worker.GenderId=row.GenderId;worker.NationalityId=row.NationalityId;worker.IsActive=row.Active;worker.IsDeleted=false;}
            else db.HcmWorkers.Add(new HcmWorker{RecId=row.Id,PersonnelNumber=code,Person=parties[row.Id].RecId,OccupationId=row.OccupationId,GenderId=row.GenderId,NationalityId=row.NationalityId,IsActive=row.Active,CreatedAt=row.CreatedAt,CreatedBy=row.CreatedBy??owner,OwnerAccountId=owner});
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

    private async Task<Data> ReadAsync(CancellationToken ct)
    {
        return SeedDbConnectionString is null
            ? await ReadEmbeddedAsync(ct)
            : await ReadSeedDatabaseAsync(SeedDbConnectionString, ct);
    }

    private static async Task<Data> ReadSeedDatabaseAsync(
        string connectionString,
        CancellationToken ct)
    {
        const string sql = """
            SELECT
              JSON_QUERY((SELECT DepartmentId AS Id,
                                  COALESCE(NULLIF(DepartmentNameAR, N''), DepartmentName) AS Name,
                                  COALESCE(DescriptionAR, Description) AS Description,
                                  Activated AS Active
                           FROM dbo.Departments FOR JSON PATH)) AS Departments,
              JSON_QUERY((SELECT OccupationId AS Id,
                                  COALESCE(NULLIF(OccupationNameAR, N''), OccupationName) AS Name,
                                  COALESCE(DescriptionAR, Description) AS Description,
                                  Activated AS Active
                           FROM dbo.Occupations FOR JSON PATH)) AS Occupations,
              JSON_QUERY((SELECT GenderId AS Id,
                                  COALESCE(NULLIF(GenderNameAR, N''), GenderName) AS Name,
                                  COALESCE(DescriptionAR, Description) AS Description
                           FROM dbo.Genders FOR JSON PATH)) AS Genders,
              JSON_QUERY((SELECT NationalityId AS Id,
                                  COALESCE(NULLIF(NationalityNameAR, N''), NationalityName) AS Name,
                                  COALESCE(DescriptionAR, Description) AS Description,
                                  Activated AS Active
                           FROM dbo.Nationalities FOR JSON PATH)) AS Nationalities,
              JSON_QUERY((SELECT EmployeeId AS Id, EmployeeCode AS Code,
                                  EmployeeName AS Name, EmployeeNameAR AS NameAr,
                                  DepartmentId, OccupationId, GenderId, NationalityId,
                                  CreatedDate AS CreatedAt,
                                  CONVERT(nvarchar(450), CreatedBy) AS CreatedBy,
                                  Activated AS Active
                           FROM dbo.Employees FOR JSON PATH)) AS Employees
            FOR JSON PATH, WITHOUT_ARRAY_WRAPPER;
            """;

        var json = await ReadJsonAsync(connectionString, sql, ct);
        return JsonSerializer.Deserialize<Data>(json,
            new JsonSerializerOptions { PropertyNameCaseInsensitive = true })
            ?? throw new InvalidOperationException(
                "SeedDbConnString returned invalid organization seed data.");
    }

    private static async Task<Data> ReadEmbeddedAsync(CancellationToken ct)
    {
        var assembly=typeof(OthersDBOrganizationEmployeeSeeder).Assembly;
        var name=assembly.GetManifestResourceNames().Single(x=>x.EndsWith(ResourceSuffix,StringComparison.Ordinal));
        await using var stream=assembly.GetManifestResourceStream(name)??throw new InvalidOperationException($"Missing resource {name}");
        return await JsonSerializer.DeserializeAsync<Data>(stream,new JsonSerializerOptions{PropertyNameCaseInsensitive=true},ct)??throw new InvalidOperationException("Invalid organization employee seed resource.");
    }

    private sealed class Data{public LookupShort[] Departments{get;set;}=[];public LookupShort[] Occupations{get;set;}=[];public LookupByte[] Genders{get;set;}=[];public LookupShort[] Nationalities{get;set;}=[];public Employee[] Employees{get;set;}=[];}
    private sealed class LookupShort{public short Id{get;set;}public string? Name{get;set;}public string? Description{get;set;}public bool Active{get;set;}}
    private sealed class LookupByte{public byte Id{get;set;}public string? Name{get;set;}public string? Description{get;set;}}
    private sealed class Employee{public long Id{get;set;}public string? Code{get;set;}public string? Name{get;set;}public string? NameAr{get;set;}public short DepartmentId{get;set;}public short OccupationId{get;set;}public byte GenderId{get;set;}public short NationalityId{get;set;}public DateTime CreatedAt{get;set;}public string? CreatedBy{get;set;}public bool Active{get;set;}}
}
