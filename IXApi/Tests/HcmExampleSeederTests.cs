using IAX.IXApi.Infrastructure.Persistence.Seeding.Chunks;
using IAX.IXApi.Modules.Finance.Entities;
using IAX.IXApi.Modules.Finance.Foundation.Departments;
using IAX.IXApi.Modules.Finance.Foundation.Genders;
using IAX.IXApi.Modules.Finance.Foundation.HcmShowrooms;
using IAX.IXApi.Modules.Finance.Foundation.HcmWorkers;
using IAX.IXApi.Modules.Finance.Foundation.Nationalities;
using IAX.IXApi.Modules.Finance.Foundation.Occupations;
using IAX.IXApi.Modules.Finance.Foundation.WorkerOrganizationAssignments;
using IAX.IXApi.Modules.Finance.Foundation.WorkerShowroomAssignments;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace IXApi.Tests;

public sealed class HcmExampleSeederTests
{
    [Fact]
    public async Task Examples_have_valid_references_and_repeat_without_duplicates()
    {
        await using var connection = new SqliteConnection("Data Source=:memory:");
        await connection.OpenAsync();
        await using var db = new SeedContext(new DbContextOptionsBuilder<SeedContext>().UseSqlite(connection).Options);
        await db.Database.EnsureCreatedAsync();
        var seeder = new HcmExampleSeeder();
        await seeder.SeedExamplesAsync(db);
        db.ChangeTracker.Clear();
        await seeder.SeedExamplesAsync(db);

        Assert.Equal(7, await db.Set<HcmWorker>().CountAsync());
        Assert.Equal(9, await db.Set<DirPartyTable>().CountAsync());
        Assert.Equal(2, await db.Set<HcmDepartment>().CountAsync());
        Assert.Equal(6, await db.Set<HcmOccupation>().CountAsync());
        Assert.Equal(2, await db.Set<HcmShowroom>().CountAsync());
        var assignments = await db.Set<HcmWorkerOrganizationAssignmentV1>()
            .Include(x => x.HcmWorker).Include(x => x.HcmManager).Include(x => x.Department).Include(x => x.Occupation).ToListAsync();
        Assert.Equal(6, assignments.Count);
        Assert.All(assignments, x =>
        {
            Assert.NotEqual(x.HcmWorkerId, x.HcmManagerWorkerId);
            Assert.Equal("dat", x.HcmWorker.DataAreaId);
            Assert.Equal("dat", x.HcmManager.DataAreaId);
            Assert.NotNull(x.Department);
            Assert.NotNull(x.Occupation);
            Assert.Equal(x.HcmWorker.OccupationId, x.OccupationId);
        });
        Assert.Equal(2, await db.Set<HcmWorkerShowroomAssignment>().CountAsync());

        var department = await db.Set<HcmDepartment>().FirstAsync();
        department.NameAlias = "اسم مخصص";
        var assignment = assignments[0];
        assignment.ValidTo = new DateOnly(2026, 6, 1);
        assignment.IsDeleted = true;
        var showroomAssignment = await db.Set<HcmWorkerShowroomAssignment>().FirstAsync();
        showroomAssignment.ValidTo = new DateOnly(2026, 6, 1);
        showroomAssignment.IsActive = false;
        await db.SaveChangesAsync();
        db.ChangeTracker.Clear();
        await seeder.SeedExamplesAsync(db);
        Assert.Equal("اسم مخصص", (await db.Set<HcmDepartment>().FindAsync(department.RecId))!.NameAlias);
        Assert.True((await db.Set<HcmWorkerOrganizationAssignmentV1>().FindAsync(assignment.RecId))!.IsDeleted);
        Assert.False((await db.Set<HcmWorkerShowroomAssignment>().FindAsync(showroomAssignment.RecId))!.IsActive);
        Assert.Equal(6, await db.Set<HcmWorkerOrganizationAssignmentV1>().CountAsync());
        Assert.Equal(2, await db.Set<HcmWorkerShowroomAssignment>().CountAsync());
    }

    private sealed class SeedContext(DbContextOptions<SeedContext> options) : DbContext(options)
    {
        public override Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
        {
            foreach (var entry in ChangeTracker.Entries().Where(x => x.State == EntityState.Added))
                entry.Property("RowVersion").CurrentValue = Array.Empty<byte>();
            return base.SaveChangesAsync(cancellationToken);
        }

        protected override void OnModelCreating(ModelBuilder b)
        {
            b.ApplyConfiguration(new HcmWorkerConfiguration());
            b.ApplyConfiguration(new HcmWorkerOrganizationAssignmentV1Configuration());
            b.ApplyConfiguration(new HcmWorkerShowroomAssignmentConfiguration());
            b.Entity<HcmDepartment>(); b.Entity<HcmOccupation>();
            b.Entity<Gender>(); b.Entity<HcmNationality>(); b.Entity<DirPartyTable>();
            var allowed = new[] { typeof(HcmWorker), typeof(HcmDepartment), typeof(HcmOccupation),
                typeof(Gender), typeof(HcmNationality), typeof(DirPartyTable), typeof(HcmShowroom),
                typeof(HcmWorkerOrganizationAssignmentV1), typeof(HcmWorkerShowroomAssignment) };
            foreach (var type in b.Model.GetEntityTypes().Where(x => !allowed.Contains(x.ClrType)).Select(x => x.ClrType).ToList())
                b.Ignore(type);
            b.Entity<DirPartyTable>().HasIndex(x => x.PartyNumber).IsUnique();
            // Adapt SQL Server defaults and rowversion for this relational SQLite fixture.
            foreach (var entity in b.Model.GetEntityTypes().ToList())
            {
                b.Entity(entity.ClrType).Property<byte[]>("RowVersion").IsConcurrencyToken(false)
                    .ValueGeneratedNever().HasDefaultValue(Array.Empty<byte>());
                b.Entity(entity.ClrType).Property("CreatedAt").HasDefaultValueSql("CURRENT_TIMESTAMP");
            }
        }
    }
}
