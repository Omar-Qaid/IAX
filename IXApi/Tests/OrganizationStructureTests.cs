using FluentValidation;
using IAX.IXApi.Modules.Finance.Foundation.HcmWorkers;
using IAX.IXApi.Modules.Finance.Foundation.OrganizationUnits;
using IAX.IXApi.Modules.Finance.Foundation.Structure;
using IAX.IXApi.Modules.Finance.Foundation.WorkerOrganizationAssignments;
using IAX.IXApi.Modules.Identity.Users;
using IAX.IXApi.Modules.Organization.DocumentManagement.Entities;
using IAX.IXApi.Modules.Organization.Persistence;
using IAX.IXApi.Modules.Finance.Persistence;
using IAX.IXApi.Modules.Finance.Entities;
using IAX.IXApi.Shared.Application.Identity;
using IAX.IXApi.Shared.Domain.Entities;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace IXApi.Tests;

public sealed class OrganizationStructureTests
{
    private static readonly DateOnly Start = new(2026, 1, 1);
    private static readonly DateOnly TransferDate = new(2026, 9, 16);

    [Fact]
    public async Task Structure_seed_is_repeatable_and_assigns_available_workers()
    {
        await using var f = await Fixture.CreateAsync();
        var seeder = new IAX.IXApi.Infrastructure.Persistence.Seeding.Chunks.OrganizationStructureSeeder();
        await seeder.SeedStructureAsync(f.Db);
        await seeder.SeedStructureAsync(f.Db);
        Assert.Equal(9, await f.Db.OrganizationUnits.CountAsync());
        Assert.Equal(6, await f.Db.OrganizationRoles.CountAsync());
        Assert.Equal(2, await f.Db.OrganizationHierarchies.CountAsync());
        Assert.Equal(14, await f.Db.OrganizationHierarchyNodes.CountAsync());
        Assert.Equal(7, await f.Db.HcmPositions.CountAsync());
        Assert.NotEmpty(await f.Db.HcmWorkerOrganizationAssignments.ToListAsync());
        Assert.DoesNotContain(await f.Db.OrganizationUnits.ToListAsync(), x => string.IsNullOrWhiteSpace(x.NameAlias));
        Assert.DoesNotContain(await f.Db.OrganizationRoles.ToListAsync(), x => string.IsNullOrWhiteSpace(x.NameAlias));
        Assert.DoesNotContain(await f.Db.OrganizationHierarchies.ToListAsync(), x => string.IsNullOrWhiteSpace(x.NameAlias));
        Assert.DoesNotContain(await f.Db.HcmPositions.ToListAsync(), x => string.IsNullOrWhiteSpace(x.NameAlias));
        var hierarchy = await f.Db.OrganizationHierarchies.SingleAsync(x => x.Code == "ORG-OPERATIONS");
        var shop = await f.Db.OrganizationUnits.SingleAsync(x => x.Code == "SH-A");
        Assert.Equal(new[] { "SH-A", "SUP-NJ", "REG-JED", "AREA-W", "ORG-BU", "ORG-COMPANY" },
            (await f.Service.GetAncestorsAsync(hierarchy.RecId, shop.RecId, Start)).Select(x => x.Code));
    }

    [Fact]
    public async Task Structure_seed_preserves_customized_closed_and_deleted_records_across_company_contexts()
    {
        await using var f = await Fixture.CreateAsync();
        var seeder = new IAX.IXApi.Infrastructure.Persistence.Seeding.Chunks.OrganizationStructureSeeder();
        await seeder.SeedStructureAsync(f.Db);
        var position = await f.Db.HcmPositions.SingleAsync(x => x.Code == "SH-A-SELLER-01");
        position.Name = "Customized seat";
        position.ValidTo = TransferDate;
        var role = await f.Db.OrganizationRoles.SingleAsync(x => x.Code == "SELLER");
        role.IsDeleted = true;
        var hierarchy = await f.Db.OrganizationHierarchies.SingleAsync(x => x.Code == "ORG-OPERATIONS");
        var shop = await f.Db.OrganizationUnits.SingleAsync(x => x.Code == "SH-A");
        var node = await f.Db.OrganizationHierarchyNodes.SingleAsync(x => x.HierarchyId == hierarchy.RecId && x.OrganizationUnitId == shop.RecId);
        node.ValidTo = TransferDate;
        await f.Db.SaveChangesAsync();
        f.Company.Code = "ksa";
        await seeder.SeedStructureAsync(f.Db);
        Assert.Empty(await f.Db.OrganizationUnits.ToListAsync());
        Assert.Equal(7, await f.Db.HcmPositions.IgnoreQueryFilters().CountAsync());
        Assert.Equal(14, await f.Db.OrganizationHierarchyNodes.IgnoreQueryFilters().CountAsync());
        Assert.Equal("Customized seat", (await f.Db.HcmPositions.IgnoreQueryFilters().SingleAsync(x => x.RecId == position.RecId)).Name);
        Assert.Equal(TransferDate, (await f.Db.OrganizationHierarchyNodes.IgnoreQueryFilters().SingleAsync(x => x.RecId == node.RecId)).ValidTo);
        Assert.True((await f.Db.OrganizationRoles.IgnoreQueryFilters().SingleAsync(x => x.RecId == role.RecId)).IsDeleted);
    }

    [Fact]
    public async Task Transfer_preserves_history_and_exclusive_boundary()
    {
        await using var f = await Fixture.CreateAsync();
        var (unit, role, position) = await f.SeatAsync("A");
        var second = await f.Service.CreatePositionAsync(new("B", "Seat B", unit, role, Start, null), default);
        var oldId = await f.Service.AssignWorkerAsync(new(1, position, Start, null), default);
        var newId = await f.Service.TransferAsync(oldId, new(second, TransferDate, null), default);
        var before = Assert.Single(await f.Service.GetWorkerAssignmentsAsync(1, TransferDate.AddDays(-1)));
        var after = Assert.Single(await f.Service.GetWorkerAssignmentsAsync(1, TransferDate));
        Assert.Equal(oldId, before.AssignmentId);
        Assert.Equal(TransferDate, before.ValidTo);
        Assert.Equal(newId, after.AssignmentId);
        Assert.Equal(second, after.PositionId);
        Assert.Equal(2, await f.Db.HcmWorkerOrganizationAssignments.CountAsync());
    }

    [Fact]
    public async Task Occupancy_and_primary_assignment_overlaps_are_rejected()
    {
        await using var f = await Fixture.CreateAsync();
        var (unit, role, position) = await f.SeatAsync("A");
        var second = await f.Service.CreatePositionAsync(new("B", "Seat B", unit, role, Start, null), default);
        await f.Service.AssignWorkerAsync(new(1, position, Start, TransferDate), default);
        await Assert.ThrowsAsync<ValidationException>(() => f.Service.AssignWorkerAsync(new(2, position, Start, null), default));
        await Assert.ThrowsAsync<ValidationException>(() => f.Service.AssignWorkerAsync(new(1, second, Start, null), default));
        // Adjacent periods do not overlap.
        await f.Service.AssignWorkerAsync(new(2, position, TransferDate, null), default);
    }

    [Fact]
    public async Task Failed_transfer_rolls_back_old_assignment_and_clears_tracked_changes()
    {
        await using var f = await Fixture.CreateAsync();
        var (unit, role, position) = await f.SeatAsync("A");
        var second = await f.Service.CreatePositionAsync(new("B", "Seat B", unit, role, Start, null), default);
        var oldId = await f.Service.AssignWorkerAsync(new(1, position, Start, null), default);
        await f.Service.AssignWorkerAsync(new(2, second, Start, null), default);
        await Assert.ThrowsAsync<ValidationException>(() => f.Service.TransferAsync(oldId, new(second, TransferDate, null), default));
        Assert.False(f.Db.ChangeTracker.HasChanges());
        Assert.Null((await f.Db.HcmWorkerOrganizationAssignments.SingleAsync(x => x.AssignmentId == oldId)).ValidTo);
        Assert.Equal(oldId, Assert.Single(await f.Service.GetWorkerAssignmentsAsync(1, TransferDate)).AssignmentId);
    }

    [Fact]
    public async Task Company_scope_applies_to_reads_and_referenced_records()
    {
        await using var f = await Fixture.CreateAsync();
        var (_, _, foreignPosition) = await f.SeatAsync("A");
        f.Company.Code = "ksa";
        Assert.Empty(await f.Service.GetUnitsAsync(Start, default));
        Assert.Empty(await f.Service.GetPositionsAsync(Start, default));
        var (unit, role, localPosition) = await f.SeatAsync("A"); // Same codes are legal in another company.
        await Assert.ThrowsAsync<ValidationException>(() => f.Service.AssignWorkerAsync(new(1, localPosition, Start, null), default));
        f.Company.Code = "dat";
        await Assert.ThrowsAsync<KeyNotFoundException>(() => f.Service.AssignWorkerAsync(new(1, localPosition, Start, null), default));
        Assert.NotEqual(foreignPosition, localPosition);
    }

    [Fact]
    public async Task Unauthorized_execution_company_is_rejected_before_writing()
    {
        await using var f = await Fixture.CreateAsync();
        f.Company.Authorized = false;
        await Assert.ThrowsAsync<UnauthorizedAccessException>(() => f.Service.CreateRoleAsync(new("SELLER", "Seller"), default));
        Assert.Equal(0, await f.Db.OrganizationRoles.IgnoreQueryFilters().CountAsync());
    }

    [Fact]
    public async Task Multiple_hierarchies_resolve_different_ancestry_and_occupants()
    {
        await using var f = await Fixture.CreateAsync();
        var (area, role, position) = await f.SeatAsync("AREA");
        var shop = await f.Service.CreateUnitAsync(new("SHOP", "Shop", 4, Start, null), default);
        var operational = await f.Service.CreateHierarchyAsync(new("OPS", "Operations", "Sales"), default);
        var financial = await f.Service.CreateHierarchyAsync(new("FIN", "Finance", "Financial reporting"), default);
        var root = await f.Service.CreateNodeAsync(new(operational, area, null, Start, null), default);
        await f.Service.CreateNodeAsync(new(operational, shop, root, Start, null), default);
        await f.Service.CreateNodeAsync(new(financial, shop, null, Start, null), default);
        Assert.Empty(await f.Service.GetRoleOccupantsAsync(operational, shop, "ROLE-AREA", Start));
        await f.Service.AssignWorkerAsync(new(1, position, Start, null), default);
        Assert.Equal(new[] { shop, area }, (await f.Service.GetAncestorsAsync(operational, shop, Start)).Select(x => x.Id));
        Assert.Single(await f.Service.GetAncestorsAsync(financial, shop, Start));
        Assert.Equal(1, Assert.Single(await f.Service.GetRoleOccupantsAsync(operational, shop, "ROLE-AREA", Start)).WorkerId);
        Assert.Empty(await f.Service.GetRoleOccupantsAsync(financial, shop, "ROLE-AREA", Start));
    }

    [Fact]
    public async Task Hierarchy_rejects_overlapping_membership_and_invalid_parent_periods()
    {
        await using var f = await Fixture.CreateAsync();
        var (area, _, _) = await f.SeatAsync("AREA");
        var shop = await f.Service.CreateUnitAsync(new("SHOP", "Shop", 4, Start, null), default);
        var hierarchy = await f.Service.CreateHierarchyAsync(new("OPS", "Operations", "Sales"), default);
        var root = await f.Service.CreateNodeAsync(new(hierarchy, area, null, Start, TransferDate), default);
        await Assert.ThrowsAsync<ValidationException>(() => f.Service.CreateNodeAsync(new(hierarchy, shop, root, Start, null), default));
        var child = await f.Service.CreateNodeAsync(new(hierarchy, shop, root, Start, TransferDate), default);
        await Assert.ThrowsAsync<ValidationException>(() => f.Service.CreateNodeAsync(new(hierarchy, shop, null, Start, TransferDate), default));
        await Assert.ThrowsAsync<ValidationException>(() => f.Service.CloseNodeAsync(root, TransferDate.AddDays(-1), default));
        await f.Service.CloseNodeAsync(child, TransferDate.AddDays(-1), default);
        await f.Service.CloseNodeAsync(root, TransferDate.AddDays(-1), default);
    }

    [Fact]
    public async Task Position_cannot_close_before_its_occupant_and_invalid_periods_fail()
    {
        await using var f = await Fixture.CreateAsync();
        var (_, _, position) = await f.SeatAsync("A");
        await Assert.ThrowsAsync<ValidationException>(() => f.Service.AssignWorkerAsync(new(1, position, TransferDate, Start), default));
        var assignment = await f.Service.AssignWorkerAsync(new(1, position, Start, null), default);
        await Assert.ThrowsAsync<ValidationException>(() => f.Service.ClosePositionAsync(position, TransferDate, default));
        await f.Service.CloseAssignmentAsync(assignment, TransferDate, default);
        await f.Service.ClosePositionAsync(position, TransferDate, default);
        Assert.Empty(await f.Service.GetPositionsAsync(TransferDate, default));
        Assert.Single(await f.Service.GetPositionsAsync(TransferDate.AddDays(-1), default));
    }

    private sealed class TestCompany : ICompanyExecutionContext
    {
        public string Code { get; set; } = "dat";
        public bool Authorized { get; set; } = true;
        public string GetDataAreaId() => Code;
        public bool IsRequestedCompanyAuthorized() => Authorized;
    }

    private sealed class Fixture : IAsyncDisposable
    {
        private readonly SqliteConnection connection = new("Data Source=:memory:");
        public TestCompany Company { get; } = new();
        public TestContext Db { get; private set; } = null!;
        public OrganizationStructureService Service { get; private set; } = null!;
        public static async Task<Fixture> CreateAsync()
        {
            var f = new Fixture();
            await f.connection.OpenAsync();
            f.Db = new TestContext(new DbContextOptionsBuilder<TestContext>().UseSqlite(f.connection).Options, f.Company);
            await f.Db.Database.EnsureCreatedAsync();
            f.Db.HcmWorkers.AddRange(new HcmWorker { RecId = 1, PersonnelNumber = "W1", DataAreaId = "dat" }, new HcmWorker { RecId = 2, PersonnelNumber = "W2", DataAreaId = "dat" });
            await f.Db.SaveChangesAsync();
            f.Service = new OrganizationStructureService(f.Db, f.Company);
            return f;
        }
        public async Task<(long Unit, long Role, long Position)> SeatAsync(string code)
        {
            var unit = await Service.CreateUnitAsync(new(code, code, 1, Start, null), default);
            var role = await Service.CreateRoleAsync(new("ROLE-" + code, "Role " + code), default);
            var position = await Service.CreatePositionAsync(new("POS-" + code, "Seat " + code, unit, role, Start, null), default);
            return (unit, role, position);
        }
        public async ValueTask DisposeAsync() { await Db.DisposeAsync(); await connection.DisposeAsync(); }
    }

    // Real relational execution with the production Organization mappings; unrelated modules are
    // excluded. SQL Server-specific defaults/rowversion are adapted only in this SQLite fixture.
    private sealed class TestContext(DbContextOptions<TestContext> options, TestCompany company) : DbContext(options), IOrganizationDataContext, IFinanceDataContext
    {
        public string CompanyCode => company.Code;
        public DbSet<HcmWorker> HcmWorkers => Set<HcmWorker>();
        public DbSet<OrganizationUnit> OrganizationUnits => Set<OrganizationUnit>();
        public DbSet<OrganizationRole> OrganizationRoles => Set<OrganizationRole>();
        public DbSet<OrganizationHierarchy> OrganizationHierarchies => Set<OrganizationHierarchy>();
        public DbSet<OrganizationHierarchyNode> OrganizationHierarchyNodes => Set<OrganizationHierarchyNode>();
        public DbSet<HcmPosition> HcmPositions => Set<HcmPosition>();
        public DbSet<HcmWorkerOrganizationAssignment> HcmWorkerOrganizationAssignments => Set<HcmWorkerOrganizationAssignment>();
        public DbSet<TaxData> TaxData => Set<TaxData>();
        public DbSet<TaxGroupHeading> TaxGroupHeadings => Set<TaxGroupHeading>();
        public DbSet<TaxGroupData> TaxGroupDatas => Set<TaxGroupData>();
        public DbSet<TaxOnItem> TaxOnItems => Set<TaxOnItem>();
        public DbSet<AspNetUser> Users => Set<AspNetUser>();
        public DbSet<DocuType> DocuTypes => Set<DocuType>();
        public DbSet<DocuValue> DocuValues => Set<DocuValue>();
        public DbSet<DocuRef> DocuRefs => Set<DocuRef>();
        public Task<long> CreateWorkerPartyAsync(string name, string nameAlias, string partyNumber, string createdBy, string ownerAccountId, CancellationToken cancellationToken = default) => throw new NotSupportedException();

        public override Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
        {
            foreach (var entry in ChangeTracker.Entries().Where(x => x.State == EntityState.Added && x.Metadata.FindProperty("RowVersion") != null))
                entry.Property("RowVersion").CurrentValue = Array.Empty<byte>();
            return base.SaveChangesAsync(cancellationToken);
        }

        protected override void OnModelCreating(ModelBuilder b)
        {
            // DbSet discovery finds unrelated interface sets; exclude them from this fixture.
             b.Ignore<AspNetUser>();
            b.Ignore<DocuType>(); b.Ignore<DocuValue>(); b.Ignore<DocuRef>();
            var worker = b.Entity<HcmWorker>();
            worker.Ignore(x => x.Nationality); worker.Ignore(x => x.User);
            b.ApplyConfiguration(new OrganizationUnitConfiguration());
            b.ApplyConfiguration(new OrganizationRoleConfiguration());
            b.ApplyConfiguration(new OrganizationHierarchyConfiguration());
            b.ApplyConfiguration(new OrganizationHierarchyNodeConfiguration());
            b.ApplyConfiguration(new HcmPositionConfiguration());
            b.ApplyConfiguration(new HcmWorkerOrganizationAssignmentConfiguration());
            var allowed = new[] { typeof(HcmWorker), typeof(OrganizationUnit), typeof(OrganizationRole), typeof(OrganizationHierarchy),
                typeof(OrganizationHierarchyNode), typeof(HcmPosition), typeof(HcmWorkerOrganizationAssignment) };
            foreach (var unrelated in b.Model.GetEntityTypes().Where(x => !allowed.Contains(x.ClrType)).Select(x => x.ClrType).ToList())
                b.Ignore(unrelated);
            b.Entity<HcmWorkerOrganizationAssignment>().Property(x => x.CreatedAt).HasDefaultValueSql("CURRENT_TIMESTAMP");
            b.Entity<HcmWorker>().HasQueryFilter(x => x.DataAreaId == CompanyCode && !x.IsDeleted);
            b.Entity<OrganizationUnit>().HasQueryFilter(x => x.DataAreaId == CompanyCode);
            b.Entity<OrganizationRole>().HasQueryFilter(x => x.DataAreaId == CompanyCode && !x.IsDeleted);
            b.Entity<OrganizationHierarchy>().HasQueryFilter(x => x.DataAreaId == CompanyCode && !x.IsDeleted);
            b.Entity<OrganizationHierarchyNode>().HasQueryFilter(x => x.DataAreaId == CompanyCode && !x.IsDeleted);
            b.Entity<HcmPosition>().HasQueryFilter(x => x.DataAreaId == CompanyCode && !x.IsDeleted);
            b.Entity<HcmWorkerOrganizationAssignment>().HasQueryFilter(x => x.DataAreaId == CompanyCode);
            foreach (var entity in b.Model.GetEntityTypes().Where(x => x.FindProperty("RowVersion") != null))
                b.Entity(entity.ClrType).Property<byte[]>("RowVersion").IsConcurrencyToken(false).ValueGeneratedNever().HasDefaultValue(Array.Empty<byte>());
        }
    }
}
