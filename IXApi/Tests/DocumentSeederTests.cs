using IAX.IXApi.Infrastructure.Persistence.Seeding.Chunks;
using IAX.IXApi.Modules.Finance.AccountsReceivable;
using IAX.IXApi.Modules.Organization.DocumentManagement.Configuration;
using IAX.IXApi.Modules.Organization.DocumentManagement.Entities;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace IXApi.Tests;

public sealed class DocumentSeederTests
{
    [Fact]
    public async Task Examples_preserve_notes_and_relationships_without_duplicate_values()
    {
        await using var connection = new SqliteConnection("Data Source=:memory:");
        await connection.OpenAsync();
        await using var db = new Fixture(new DbContextOptionsBuilder<Fixture>().UseSqlite(connection).Options);
        await db.Database.EnsureCreatedAsync();
        db.Add(new CustTable { AccountNum = "CUST-100", DataAreaId = "dat" });
        db.Add(new DocuType { TypeId = "Note", Name = "Note", TypeGroup = 1,
            CreatedBy = "sys", LastModifiedBy = "sys" });
        await db.SaveChangesAsync();
        var seeder = new DocumentExampleSeeder();
        await seeder.SeedExamplesAsync(db);
        var document = await db.Set<DocuRef>().FirstAsync();
        document.Notes = "Customer-specific receiving instructions";
        document.Name = "Customized document title";
        var valueId = document.ValueRecId;
        await db.SaveChangesAsync();
        db.ChangeTracker.Clear();
        await seeder.SeedExamplesAsync(db);
        await seeder.SeedExamplesAsync(db);
        Assert.Equal(2, await db.Set<DocuRef>().CountAsync());
        Assert.Equal(2, await db.Set<DocuValue>().CountAsync());
        var preserved = await db.Set<DocuRef>().SingleAsync(x => x.RecId == document.RecId);
        Assert.Equal("Customer-specific receiving instructions", preserved.Notes);
        Assert.Equal("Customized document title", preserved.Name);
        Assert.Equal(valueId, preserved.ValueRecId);
        Assert.All(await db.Set<DocuValue>().ToListAsync(), x => Assert.Equal("sys", x.LastModifiedBy));
        var customer = await db.Set<CustTable>().SingleAsync();
        Assert.All(await db.Set<DocuRef>().ToListAsync(), x => {
            Assert.Equal(customer.RecId, x.RefRecId);
            Assert.Equal("dat", x.RefCompanyId);
            Assert.Equal("Note", x.TypeId);
        });
    }

    private sealed class Fixture(DbContextOptions<Fixture> options) : DbContext(options)
    {
        public override Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
        {
            foreach (var entry in ChangeTracker.Entries().Where(x => x.State == EntityState.Added))
                entry.Property("RowVersion").CurrentValue = Array.Empty<byte>();
            return base.SaveChangesAsync(cancellationToken);
        }

        protected override void OnModelCreating(ModelBuilder b)
        {
            var types = new[] { typeof(CustTable), typeof(DocuType), typeof(DocuRef), typeof(DocuValue) };
            foreach (var type in types) b.Entity(type);
            foreach (var type in b.Model.GetEntityTypes().Where(x => !types.Contains(x.ClrType)).Select(x => x.ClrType).ToList()) b.Ignore(type);
            b.ApplyConfiguration(new DocuTypeConfiguration());
            b.ApplyConfiguration(new DocuValueConfiguration());
            b.ApplyConfiguration(new DocuRefConfiguration());
            foreach (var type in types)
                b.Entity(type).Property<byte[]>("RowVersion").IsConcurrencyToken(false).ValueGeneratedNever();
        }
    }
}
