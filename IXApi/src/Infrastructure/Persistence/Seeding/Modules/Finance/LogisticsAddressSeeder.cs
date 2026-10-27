using System.Linq.Expressions;
using IAX.IXApi.Infrastructure.Persistence.Seeding.Core;
using IAX.IXApi.Modules.Finance.Common;
using IAX.IXApi.Modules.Finance.Entities;
using Microsoft.EntityFrameworkCore;

namespace IAX.IXApi.Infrastructure.Persistence.Seeding.Modules.Finance;

public class LogisticsAddressSeeder : BaseModuleSeeder
{
    public override string Id => "Finance.Foundation.Logistics";
    public override int Version => 1;
    public override SeederCategory Category => SeederCategory.Reference;
    public override IReadOnlyCollection<string> DependsOn => new[] { "Finance.Foundation.CompanyInfo" };
    public override SeederExecutionPolicy Policy => SeederExecutionPolicy.Repeatable;
    public override string Scope => "dat";
    public override Task SeedAsync(SeedingContext context, CancellationToken ct) => SeedDataAsync(context.Db, ct);

    public async Task SeedDataAsync(DbContext db, CancellationToken ct = default)
    {
        foreach (var (name, contact) in new[] { ("Business", NoYes.Yes), ("Delivery", NoYes.No), ("Invoice", NoYes.No), ("Home", NoYes.Yes), ("Remit-to", NoYes.No) })
            await EnsureAsync(db, (LogisticsLocationRole x) => x.Name == name,
                new LogisticsLocationRole { Name = name, IsPostalAddress = NoYes.Yes, IsContactInfo = contact }, ct);
        await EnsureAsync(db, (LogisticsAddressCountryRegion x) => x.CountryRegionId == "SA",
            new LogisticsAddressCountryRegion { CountryRegionId = "SA", IsoCode = "SA", AddrFormat = "SAU" }, ct);

        // Real geography with illustrative building/unit numbers, not verified company premises.
        var examples = new[] {
            (Company: "DAT", Region: "MAK", RegionName: "Makkah", County: "JED", City: "Jeddah", CityKey: "SA-MAK-JED",
                District: "Al Rawdah", Zip: "23431", Street: "Prince Sultan Street", Building: "4100", Additional: "2100", Phone: "+966120000000", Email: "info@dat.com"),
            (Company: "HBMC", Region: "RIY", RegionName: "Riyadh", County: "RUH", City: "Riyadh", CityKey: "SA-RIY-RUH",
                District: "Al Amal", Zip: "12643", Street: "King Abdul Aziz Road", Building: "4200", Additional: "2200", Phone: "+966112345678", Email: "info@hbmc.com.sa")
        };
        foreach (var example in examples)
        {
            await EnsureAsync(db, (LogisticsAddressState x) => x.CountryRegionId == "SA" && x.StateId == example.Region,
                new LogisticsAddressState { CountryRegionId = "SA", StateId = example.Region, Name = example.RegionName }, ct);
            await EnsureAsync(db, (LogisticsAddressCounty x) => x.CountryRegionId == "SA" && x.StateId == example.Region && x.CountyId == example.County,
                new LogisticsAddressCounty { CountryRegionId = "SA", StateId = example.Region, CountyId = example.County, Name = example.City }, ct);
            var city = await EnsureAsync(db, (LogisticsAddressCity x) => x.CityKey == example.CityKey,
                new LogisticsAddressCity { CityKey = example.CityKey, Name = example.City, Description = example.City + " City",
                    CountryRegionId = "SA", StateId = example.Region, CountyId = example.County }, ct);
            // Fill only the original Riyadh seed's missing governorate.
            if (city.CreatedBy == "sys" && city.OwnerAccountId == "sys" && city.Name == "Riyadh" && city.Description == "Riyadh City"
                && city.CountryRegionId == "SA" && city.StateId == "RIY" && city.CountyId == "") city.CountyId = example.County;
            if (city.Name != example.City || city.CountryRegionId != "SA" || city.StateId != example.Region || city.CountyId != example.County)
                throw new InvalidOperationException("Existing example city geography differs; customized records were preserved.");
            var district = await EnsureAsync(db, (LogisticsAddressDistrict x) => x.Name == example.District,
                new LogisticsAddressDistrict { Name = example.District, Description = example.District + " district", City = city.RecId }, ct);
            if (district.City != city.RecId)
                throw new InvalidOperationException("Existing district belongs to another city; customized records were preserved.");
            var zip = await EnsureAsync(db, (LogisticsAddressZipCode x) => x.ZipCode == example.Zip,
                new LogisticsAddressZipCode { ZipCode = example.Zip, CountryRegionId = "SA", State = example.Region, County = example.County,
                    City = example.City, CityAlias = example.County, CityRecId = city.RecId, District = district.RecId, DistrictName = district.Name }, ct);
            if (zip.CountryRegionId != "SA" || zip.State != example.Region || zip.City != example.City || zip.County != example.County
                || zip.CityRecId != city.RecId || zip.District != district.RecId || zip.DistrictName != district.Name)
                throw new InvalidOperationException("Example postal codes belong to different geography; existing postal codes were preserved.");
            var locationCode = example.Company + "-LOC-1";
            var location = await EnsureAsync(db, (LogisticsLocation x) => x.LocationId == locationCode,
                new LogisticsLocation { LocationId = locationCode, Description = example.Company + " Headquarters", IsPostalAddress = NoYes.Yes }, ct);
            var address = await db.Set<LogisticsPostalAddress>().IgnoreQueryFilters().FirstOrDefaultAsync(x => x.Location == location.RecId, ct);
            if (address == null || IsOriginalExample(address, example.Company))
            {
                if (address == null)
                {
                    address = new LogisticsPostalAddress { Location = location.RecId, CreatedBy = "sys", OwnerAccountId = "sys", DataAreaId = "dat",
                        IsActive = true, ValidFrom = new DateTime(2026, 1, 1), ValidTo = new DateTime(2154, 12, 31) };
                    db.Add(address);
                }
                address.CountryRegionId = "SA";
                address.State = example.Region;
                address.County = example.County;
                address.City = city.Name;
                address.CityRecId = city.RecId;
                address.District = district.RecId;
                address.DistrictName = district.Name;
                address.ZipCode = zip.ZipCode;
                address.ZipCodeRecId = zip.RecId;
                address.Street = example.Street;
                address.StreetNumber = example.Building;
                // No dedicated additional-number/unit fields exist in this entity.
                address.BuildingCompliment = "Unit 1; Additional number " + example.Additional;
                address.Address = $"{example.Building} {example.Street}, Unit 1\n{district.Name}\n{city.Name} {zip.ZipCode} - {example.Additional}\n{example.RegionName} Region, Saudi Arabia";
            }
            var emailCode = example.Company + "-EML-1";
            await EnsureAsync(db, (LogisticsElectronicAddress x) => x.ElectronicAddressId == emailCode,
                new LogisticsElectronicAddress { ElectronicAddressId = emailCode, Location = location.RecId, Description = "Primary Email",
                    Type = ElectronicAddressType.Email, Locator = example.Email, IsPrimary = NoYes.Yes }, ct);
            var phoneCode = example.Company + "-PHN-1";
            var phone = await EnsureAsync(db, (LogisticsElectronicAddress x) => x.ElectronicAddressId == phoneCode,
                new LogisticsElectronicAddress { ElectronicAddressId = phoneCode, Location = location.RecId, Description = "Primary Phone",
                    Type = ElectronicAddressType.Phone, Locator = example.Phone, IsPrimary = NoYes.Yes }, ct);
            if (example.Company == "DAT" && phone.CreatedBy == "sys" && phone.OwnerAccountId == "sys" && phone.Locator == "+15551234567"
                && phone.Location == location.RecId && phone.Type == ElectronicAddressType.Phone) phone.Locator = example.Phone;
            await db.SaveChangesAsync(ct);
        }
    }

    private static bool IsOriginalExample(LogisticsPostalAddress address, string company)
    {
        if (address.CreatedBy != "sys" || address.OwnerAccountId != "sys" || address.DataAreaId != "dat"
            || address.BuildingCompliment != "" || address.PostBox != "" || address.County != "") return false;
        return company == "DAT"
            ? address.Address == "123 Main St\nLos Angeles, CA 90001\nUSA" && address.Street == "Main St" && address.StreetNumber == "123"
                && address.City == "Los Angeles" && address.State == "CA" && address.CountryRegionId == "US" && address.ZipCode == "90001" && address.DistrictName == ""
            : address.Address == "King Fahd Rd\nRiyadh, RIY 12211\nSaudi Arabia" && address.Street == "King Fahd Rd" && address.StreetNumber == "1"
                && address.City == "Riyadh" && address.State == "RIY" && address.CountryRegionId == "SA" && address.ZipCode == "12211"
                && (address.DistrictName == "" || address.DistrictName == "Riyadh HQ example");
    }

    private static async Task<T> EnsureAsync<T>(DbContext db, Expression<Func<T, bool>> key, T row, CancellationToken ct) where T : class
    {
        var existing = await db.Set<T>().IgnoreQueryFilters().SingleOrDefaultAsync(key, ct);
        if (existing != null) return existing;
        var entry = db.Add(row);
        entry.Property("CreatedBy").CurrentValue = "sys";
        entry.Property("OwnerAccountId").CurrentValue = "sys";
        entry.Property("DataAreaId").CurrentValue = "dat";
        entry.Property("IsActive").CurrentValue = true;
        await db.SaveChangesAsync(ct);
        return row;
    }
}
