using IAX.IXApi.Infrastructure.Persistence.Seeding.Modules.Finance;
using IAX.IXApi.Modules.Finance.Entities;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace IXApi.Tests;

public sealed class LogisticsSeederTests
{
    [Fact]
    public async Task Mixed_DAT_example_geography_is_repaired_without_duplicate_addresses()
    {
        await using var connection = new SqliteConnection("Data Source=:memory:");
        await connection.OpenAsync();
        await using var db = new Fixture(new DbContextOptionsBuilder<Fixture>().UseSqlite(connection).Options);
        await db.Database.EnsureCreatedAsync();
        var seed = new LogisticsAddressSeeder();
        await seed.SeedDataAsync(db);
        var location = await db.Set<LogisticsLocation>().SingleAsync(x => x.LocationId == "DAT-LOC-1");
        var address = await db.Set<LogisticsPostalAddress>().SingleAsync(x => x.Location == location.RecId);
        var originalId = address.RecId;
        address.State = "RIY";
        address.County = "RUH";
        address.City = "";
        address.DistrictName = "";
        await db.SaveChangesAsync();
        db.ChangeTracker.Clear();
        await seed.SeedDataAsync(db);
        await seed.SeedDataAsync(db);
        var repaired = await db.Set<LogisticsPostalAddress>().SingleAsync(x => x.RecId == originalId);
        Assert.Equal("Jeddah", repaired.City);
        Assert.Equal("MAK", repaired.State);
        Assert.Equal("JED", repaired.County);
        Assert.Equal("Al Rawdah", repaired.DistrictName);
        var zip = await db.Set<LogisticsAddressZipCode>().SingleAsync(x => x.RecId == repaired.ZipCodeRecId);
        Assert.Equal(zip.CityRecId, repaired.CityRecId);
        Assert.Equal(zip.District, repaired.District);
        Assert.Equal(2, await db.Set<LogisticsPostalAddress>().CountAsync());
    }

    [Theory]
    [InlineData("DAT", false)]
    [InlineData("DAT", true)]
    [InlineData("HBMC", false)]
    [InlineData("HBMC", true)]
    public async Task Saudi_examples_migrate_original_addresses_and_preserve_customizations(string company, bool customized)
    {
        await using var connection = new SqliteConnection("Data Source=:memory:");
        await connection.OpenAsync();
        await using var db = new Fixture(new DbContextOptionsBuilder<Fixture>().UseSqlite(connection).Options);
        await db.Database.EnsureCreatedAsync();
        var seed = new LogisticsAddressSeeder();
        await seed.SeedDataAsync(db);
        foreach (var row in await db.Set<LogisticsPostalAddress>().ToListAsync())
        {
            Assert.Equal("SA", row.CountryRegionId);
            Assert.NotEmpty(row.County);
            Assert.Equal(4, row.StreetNumber.Length);
            Assert.Contains("Additional number", row.BuildingCompliment);
            var city = await db.Set<LogisticsAddressCity>().SingleAsync(x => x.RecId == row.CityRecId);
            var zip = await db.Set<LogisticsAddressZipCode>().SingleAsync(x => x.RecId == row.ZipCodeRecId);
            var district = await db.Set<LogisticsAddressDistrict>().SingleAsync(x => x.RecId == row.District);
            Assert.Equal(row.County, city.CountyId);
            Assert.Equal(city.RecId, district.City);
            Assert.Equal(city.RecId, zip.CityRecId);
            Assert.Equal(district.RecId, zip.District);
        }
        var location = await db.Set<LogisticsLocation>().SingleAsync(x => x.LocationId == company + "-LOC-1");
        var address = await db.Set<LogisticsPostalAddress>().SingleAsync(x => x.Location == location.RecId);
        var originalId = address.RecId;
        address.CountryRegionId = company == "DAT" ? "US" : "SA";
        address.State = company == "DAT" ? "CA" : "RIY";
        address.City = company == "DAT" ? "Los Angeles" : "Riyadh";
        address.Street = company == "DAT" ? "Main St" : "King Fahd Rd";
        address.StreetNumber = company == "DAT" ? "123" : "1";
        address.ZipCode = company == "DAT" ? "90001" : "12211";
        address.Address = company == "DAT" ? "123 Main St\nLos Angeles, CA 90001\nUSA" : "King Fahd Rd\nRiyadh, RIY 12211\nSaudi Arabia";
        address.DistrictName = company == "DAT" ? "" : "Riyadh HQ example";
        address.County = "";
        address.BuildingCompliment = "";
        if (customized) address.Street = "Customer's chosen street";
        await db.SaveChangesAsync();
        db.ChangeTracker.Clear();
        await seed.SeedDataAsync(db);
        await seed.SeedDataAsync(db);
        var retained = await db.Set<LogisticsPostalAddress>().SingleAsync(x => x.RecId == originalId);
        Assert.Equal(2, await db.Set<LogisticsPostalAddress>().CountAsync());
        Assert.Equal(2, await db.Set<LogisticsAddressDistrict>().CountAsync());
        Assert.Equal(2, await db.Set<LogisticsAddressZipCode>().CountAsync());
        if (customized) Assert.Equal("Customer's chosen street", retained.Street);
        else
        {
            Assert.Equal("SA", retained.CountryRegionId);
            Assert.Equal(company == "DAT" ? "Jeddah" : "Riyadh", retained.City);
            Assert.NotEmpty(retained.County);
            Assert.Contains("Additional number", retained.BuildingCompliment);
        }
    }
    [Fact]
    public async Task Existing_postal_code_in_other_country_is_preserved_and_not_linked()
    {
        await using var connection = new SqliteConnection("Data Source=:memory:");
        await connection.OpenAsync();
        await using var db = new Fixture(new DbContextOptionsBuilder<Fixture>().UseSqlite(connection).Options);
        await db.Database.EnsureCreatedAsync();
        db.Add(new LogisticsAddressZipCode { ZipCode = "23431", CountryRegionId = "US", State = "NY", City = "Albany" });
        await db.SaveChangesAsync();
        var error = await Assert.ThrowsAsync<InvalidOperationException>(() => new LogisticsAddressSeeder().SeedDataAsync(db));
        Assert.Contains("existing postal codes were preserved", error.Message);
        Assert.Equal("Albany", (await db.Set<LogisticsAddressZipCode>().SingleAsync(x => x.ZipCode == "23431")).City);
        Assert.Empty(await db.Set<LogisticsPostalAddress>().ToListAsync());
    }

    private sealed class Fixture(DbContextOptions<Fixture> options) : DbContext(options)
    {
        public override Task<int> SaveChangesAsync(CancellationToken ct = default)
        {
            foreach (var entry in ChangeTracker.Entries().Where(x => x.State == EntityState.Added))
                entry.Property("RowVersion").CurrentValue = Array.Empty<byte>();
            return base.SaveChangesAsync(ct);
        }
        protected override void OnModelCreating(ModelBuilder b)
        {
            var types = new[] { typeof(LogisticsAddressCity), typeof(LogisticsAddressCountryRegion), typeof(LogisticsAddressCounty),
                typeof(LogisticsAddressDistrict), typeof(LogisticsAddressState), typeof(LogisticsAddressZipCode),
                typeof(LogisticsElectronicAddress), typeof(LogisticsLocation), typeof(LogisticsLocationRole), typeof(LogisticsPostalAddress) };
            foreach (var type in types) b.Entity(type);
            foreach (var type in b.Model.GetEntityTypes().Where(x => !types.Contains(x.ClrType)).Select(x => x.ClrType).ToList()) b.Ignore(type);
            foreach (var type in types)
            {
                foreach (var nav in b.Entity(type).Metadata.GetNavigations().ToList()) b.Entity(type).Ignore(nav.Name);
                b.Entity(type).HasKey("RecId");
                b.Entity(type).Property<byte[]>("RowVersion").IsConcurrencyToken(false).ValueGeneratedNever();
            }
            b.Entity<LogisticsAddressDistrict>().HasIndex(x => x.Name).IsUnique();
            b.Entity<LogisticsAddressZipCode>().HasIndex(x => x.ZipCode).IsUnique();
        }
    }
}

