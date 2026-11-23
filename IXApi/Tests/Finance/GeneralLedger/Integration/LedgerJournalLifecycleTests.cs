using System.Reflection;
using IAX.IXApi.Infrastructure.Identity;
using IAX.IXApi.Infrastructure.Persistence.Repositories;
using IAX.IXApi.Modules.Finance.Common;
using IAX.IXApi.Modules.Finance.Entities;
using IAX.IXApi.Modules.Finance.GeneralLedger.LedgerJournalTable;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace IAX.IXApi.Tests;

public class LedgerJournalLifecycleTests
{
    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public async Task Journal_name_deletion_refuses_pending_header_reference_changes(bool bulk)
    {
        await using var connection = new SqliteConnection("Data Source=:memory:");
        await connection.OpenAsync();
        await using var db = await CreateAsync(connection);
        db.Add(new LedgerJournalName { RecId = 10, DataAreaId = "dat", JournalName = "GEN", Name = "General" });
        await db.SaveChangesAsync();
        db.ChangeTracker.Clear();
        var name = await db.Set<LedgerJournalName>().AsNoTracking().SingleAsync();
        var header = await db.Set<LedgerJournalTable>().SingleAsync(x => x.JournalNum == "J1");
        header.JournalName = "GEN";
        var service = new LedgerJournalNameService(new Work(db), new User());

        var error = bulk
            ? await Assert.ThrowsAsync<InvalidOperationException>(() => service.RemoveRangeAsync([name]))
            : await Assert.ThrowsAsync<InvalidOperationException>(() => service.RemoveAsync(name));

        Assert.Contains("in use", error.Message);
        db.ChangeTracker.Clear();
        Assert.Single(await db.Set<LedgerJournalName>().AsNoTracking().ToListAsync());
        Assert.Equal("", (await db.Set<LedgerJournalTable>().SingleAsync(x => x.JournalNum == "J1")).JournalName);
    }

    [Theory]
    [InlineData(false, "dat")]
    [InlineData(true, "dat")]
    [InlineData(false, "")]
    [InlineData(true, "")]
    public async Task Journal_name_deletion_refuses_pending_referencing_headers(bool bulk, string area)
    {
        await using var connection = new SqliteConnection("Data Source=:memory:");
        await connection.OpenAsync();
        await using var db = await CreateAsync(connection);
        db.Add(new LedgerJournalName { RecId = 10, DataAreaId = "dat", JournalName = "GEN", Name = "General" });
        await db.SaveChangesAsync();
        db.ChangeTracker.Clear();
        var name = await db.Set<LedgerJournalName>().AsNoTracking().SingleAsync();
        db.Add(new LedgerJournalTable { DataAreaId = area, JournalNum = "J3", JournalName = "GEN" });
        var service = new LedgerJournalNameService(new Work(db), new User());

        var error = bulk
            ? await Assert.ThrowsAsync<InvalidOperationException>(() => service.RemoveRangeAsync([name]))
            : await Assert.ThrowsAsync<InvalidOperationException>(() => service.RemoveAsync(name));

        Assert.Contains("in use", error.Message);
        db.ChangeTracker.Clear();
        Assert.Single(await db.Set<LedgerJournalName>().AsNoTracking().ToListAsync());
        Assert.Equal(2, await db.Set<LedgerJournalTable>().CountAsync());
    }

    [Theory]
    [InlineData(false, "dat")]
    [InlineData(true, "dat")]
    [InlineData(false, "")]
    [InlineData(true, "")]
    public async Task Deleting_a_header_with_a_pending_line_is_refused(bool bulk, string area)
    {
        await using var connection = new SqliteConnection("Data Source=:memory:");
        await connection.OpenAsync();
        await using var db = await CreateAsync(connection);
        var header = await db.Set<LedgerJournalTable>().AsNoTracking().SingleAsync(x => x.JournalNum == "J1");
        db.Add(new LedgerJournalTrans { DataAreaId = area, JournalNum = "J1", LineNum = 1 });
        var service = new LedgerJournalTableService(new Work(db), new User());

        var error = bulk
            ? await Assert.ThrowsAsync<InvalidOperationException>(() => service.RemoveRangeAsync([header]))
            : await Assert.ThrowsAsync<InvalidOperationException>(() => service.RemoveAsync(header));

        Assert.Contains("Remove the journal lines", error.Message);
        db.ChangeTracker.Clear();
        Assert.Equal(2, await db.Set<LedgerJournalTable>().CountAsync());
        Assert.Empty(await db.Set<LedgerJournalTrans>().AsNoTracking().ToListAsync());
    }

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public async Task Journal_names_in_use_cannot_be_deleted_singly_or_in_bulk(bool bulk)
    {
        await using var connection = new SqliteConnection("Data Source=:memory:");
        await connection.OpenAsync();
        await using var db = await CreateAsync(connection);
        (await db.Set<LedgerJournalTable>().SingleAsync(x => x.JournalNum == "J1")).JournalName = "GEN";
        db.AddRange(new LedgerJournalName { RecId = 10, DataAreaId = "dat", JournalName = "GEN", Name = "General" },
            new LedgerJournalName { RecId = 11, DataAreaId = "dat", JournalName = "AUX", Name = "Other" });
        await db.SaveChangesAsync();
        db.ChangeTracker.Clear();
        var names = await db.Set<LedgerJournalName>().AsNoTracking().OrderByDescending(x => x.RecId).ToListAsync();
        var service = new LedgerJournalNameService(new Work(db), new User());

        var error = bulk
            ? await Assert.ThrowsAsync<InvalidOperationException>(() => service.RemoveRangeAsync(names))
            : await Assert.ThrowsAsync<InvalidOperationException>(() => service.RemoveAsync(names.Single(x => x.JournalName == "GEN")));
        Assert.Contains("in use", error.Message);
        Assert.Equal(2, await db.Set<LedgerJournalName>().CountAsync());
        await service.RemoveAsync(names.Single(x => x.JournalName == "AUX"));
        Assert.Single(await db.Set<LedgerJournalName>().ToListAsync());
    }

    [Fact]
    public async Task Bulk_journal_name_update_cannot_rename_an_existing_identifier()
    {
        await using var connection = new SqliteConnection("Data Source=:memory:");
        await connection.OpenAsync();
        await using var db = await CreateAsync(connection);
        db.Add(new LedgerJournalName { RecId = 10, DataAreaId = "dat", JournalName = "GEN", Name = "General" });
        await db.SaveChangesAsync();
        db.ChangeTracker.Clear();
        var changed = await db.Set<LedgerJournalName>().AsNoTracking().SingleAsync();
        changed.JournalName = "RENAMED";

        var error = await Assert.ThrowsAsync<InvalidOperationException>(() =>
            new LedgerJournalNameService(new Work(db), new User()).UpdateRangeAsync([changed]));

        Assert.Contains("identifier cannot be changed", error.Message);
        Assert.Equal("GEN", (await db.Set<LedgerJournalName>().AsNoTracking().SingleAsync()).JournalName);
    }

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public async Task Deleting_a_header_cannot_leave_orphan_journal_lines(bool bulk)
    {
        await using var connection = new SqliteConnection("Data Source=:memory:");
        await connection.OpenAsync();
        await using var db = await CreateAsync(connection);
        db.Add(new LedgerJournalTrans { DataAreaId = "dat", JournalNum = "J1", LineNum = 1 });
        await db.SaveChangesAsync();
        db.ChangeTracker.Clear();
        var headers = await db.Set<LedgerJournalTable>().AsNoTracking().OrderByDescending(x => x.JournalNum).ToListAsync();
        var service = new LedgerJournalTableService(new Work(db), new User());
        var error = bulk
            ? await Assert.ThrowsAsync<InvalidOperationException>(() => service.RemoveRangeAsync(headers))
            : await Assert.ThrowsAsync<InvalidOperationException>(() => service.RemoveAsync(headers.Single(x => x.JournalNum == "J1")));
        Assert.Contains("Remove the journal lines", error.Message);
        Assert.Equal(2, await db.Set<LedgerJournalTable>().CountAsync());
        Assert.Single(await db.Set<LedgerJournalTrans>().ToListAsync());
        await service.RemoveAsync(headers.Single(x => x.JournalNum == "J2"));
        Assert.Single(await db.Set<LedgerJournalTable>().ToListAsync());
    }

    [Fact]
    public async Task Bulk_lines_receive_unique_numbers_per_journal_and_respect_explicit_numbers()
    {
        await using var connection = new SqliteConnection("Data Source=:memory:");
        await connection.OpenAsync();
        await using var db = await CreateAsync(connection);
        db.Add(new LedgerJournalTrans { DataAreaId = "dat", JournalNum = "J1", LineNum = 5 });
        await db.SaveChangesAsync();
        db.ChangeTracker.Clear();
        var service = new LedgerJournalTransService(new Work(db), new User());
        await service.AddRangeAsync(new[]
        {
            new LedgerJournalTrans { JournalNum = "J1" },
            new LedgerJournalTrans { JournalNum = "J1", LineNum = 10 },
            new LedgerJournalTrans { JournalNum = "J2" },
            new LedgerJournalTrans { JournalNum = "J1" },
            new LedgerJournalTrans { JournalNum = "J2" }
        });
        var rows = await db.Set<LedgerJournalTrans>().AsNoTracking().ToListAsync();
        Assert.Equal(new decimal[] { 5, 10, 11, 12 }, rows.Where(x => x.JournalNum == "J1").Select(x => x.LineNum).Order());
        Assert.Equal(new decimal[] { 1, 2 }, rows.Where(x => x.JournalNum == "J2").Select(x => x.LineNum).Order());
        Assert.All(rows, x => Assert.Equal("dat", x.DataAreaId));
    }

    [Theory]
    [InlineData(false, 0L, 20L)]
    [InlineData(false, 30L, 30L)]
    [InlineData(true, 30L, 20L)]
    public async Task New_lines_default_the_offset_and_preserve_overrides_only_when_not_fixed(bool fixedOffset, long entered, long expected)
    {
        await using var connection = new SqliteConnection("Data Source=:memory:");
        await connection.OpenAsync();
        await using var db = await CreateAsync(connection);
        var header = await db.Set<LedgerJournalTable>().SingleAsync(x => x.JournalNum == "J1");
        header.FixedOffsetAccount = fixedOffset ? NoYes.Yes : NoYes.No;
        header.OffsetLedgerDimension = 20;
        await db.SaveChangesAsync();
        db.ChangeTracker.Clear();
        var line = await new LedgerJournalTransService(new Work(db), new User()).AddAsync(new LedgerJournalTrans
        {
            JournalNum = "J1", OffsetLedgerDimension = entered, TransDate = new DateTime(2026, 1, 15)
        });
        Assert.Equal(expected, line.OffsetLedgerDimension);
        Assert.Equal(LedgerJournalACType.Ledger, line.OffsetAccountType);
        Assert.Equal("SAR", line.CurrencyCode);
    }

    [Theory]
    [InlineData("add-line")]
    [InlineData("update-line")]
    [InlineData("delete-line")]
    [InlineData("update-header")]
    [InlineData("delete-header")]
    public async Task A_system_blocked_journal_cannot_be_edited(string operation)
    {
        await using var connection = new SqliteConnection("Data Source=:memory:");
        await connection.OpenAsync();
        await using var db = await CreateAsync(connection);
        var header = await db.Set<LedgerJournalTable>().SingleAsync(x => x.JournalNum == "J1");
        header.SystemBlocked = NoYes.Yes;
        var line = new LedgerJournalTrans { DataAreaId = "dat", JournalNum = "J1", LineNum = 1 };
        db.Add(line);
        await db.SaveChangesAsync();
        db.ChangeTracker.Clear();
        var lines = new LedgerJournalTransService(new Work(db), new User());
        var headers = new LedgerJournalTableService(new Work(db), new User());
        var error = await Assert.ThrowsAsync<InvalidOperationException>(async () =>
        {
            switch (operation)
            {
                case "add-line": await lines.AddAsync(new LedgerJournalTrans { JournalNum = "J1" }); break;
                case "update-line": await lines.UpdateAsync(line); break;
                case "delete-line": await lines.RemoveAsync(line); break;
                case "update-header": header.SystemBlocked = NoYes.No; await headers.UpdateAsync(header); break;
                case "delete-header": await headers.RemoveAsync(header); break;
            }
        });
        Assert.Contains("system blocked", error.Message);
        Assert.Single(await db.Set<LedgerJournalTrans>().ToListAsync());
        Assert.Equal(NoYes.Yes, (await db.Set<LedgerJournalTable>().AsNoTracking().SingleAsync(x => x.JournalNum == "J1")).SystemBlocked);
    }

    private static async Task<JournalContext> CreateAsync(SqliteConnection connection)
    {
        var db = new JournalContext(new DbContextOptionsBuilder<JournalContext>().UseSqlite(connection).Options);
        await db.Database.EnsureCreatedAsync();
        db.AddRange(new LedgerJournalTable { DataAreaId = "dat", JournalNum = "J1", CurrencyCode = "SAR" },
            new LedgerJournalTable { DataAreaId = "dat", JournalNum = "J2", CurrencyCode = "SAR" });
        await db.SaveChangesAsync();
        db.ChangeTracker.Clear();
        return db;
    }

    private sealed class JournalContext(DbContextOptions<JournalContext> options) : DbContext(options)
    {
        protected override void OnModelCreating(ModelBuilder builder)
        {
            foreach (var type in new[] { typeof(LedgerJournalTable), typeof(LedgerJournalTrans), typeof(LedgerJournalName) })
            {
                var entity = builder.Entity(type);
                entity.HasKey("RecId");
                entity.Ignore("RowVersion");
                foreach (var property in type.GetProperties())
                    if (property.PropertyType != typeof(string) && property.PropertyType != typeof(byte[]) && !property.PropertyType.IsValueType)
                        entity.Ignore(property.Name);
            }
            // Keep the production uniqueness constraints: duplicate generated numbers must fail.
            builder.ApplyConfiguration(new LedgerJournalTableConfiguration());
            builder.ApplyConfiguration(new LedgerJournalTransConfiguration());
        }
    }

    private sealed class User : ICurrentUserService
    {
        public string GetCurrentUserId() => "tester";
        public string GetOwnerAccountId() => "owner";
        public string GetDataAreaId() => "dat";
    }

    private sealed class Work(DbContext db) : IUnitOfWork
    {
        public DbContext Context => db;
        public IGenericRepository<T> Repository<T>() where T : class
        {
            var proxy = DispatchProxy.Create<IGenericRepository<T>, Repository<T>>();
            ((Repository<T>)(object)proxy).Db = db;
            return proxy;
        }
        public Task<int> CompleteAsync(CancellationToken ct = default) => db.SaveChangesAsync(ct);
        public Task BeginTransactionAsync(CancellationToken ct = default) => Task.CompletedTask;
        public Task CommitTransactionAsync(CancellationToken ct = default) => Task.CompletedTask;
        public Task RollbackTransactionAsync(CancellationToken ct = default) => Task.CompletedTask;
        public void Dispose() { }
    }

    public class Repository<T> : DispatchProxy where T : class
    {
        public DbContext Db { get; set; } = null!;
        protected override object? Invoke(MethodInfo? method, object?[]? args)
        {
            switch (method?.Name)
            {
                case "GetQueryable": return Db.Set<T>().AsQueryable();
                case "AddAsync": Db.Add((T)args![0]!); return Task.FromResult((T)args[0]!);
                case "RemoveAsync": Db.Remove((T)args![0]!); return Task.CompletedTask;
                case "RemoveRangeAsync": Db.RemoveRange((IEnumerable<T>)args![0]!); return Task.CompletedTask;
                case "AddRangeAsync":
                    var rows = ((IEnumerable<T>)args![0]!).ToList();
                    Db.AddRange(rows);
                    return Task.FromResult<IEnumerable<T>>(rows);
                case "UpdateRangeAsync":
                    var updated = ((IEnumerable<T>)args![0]!).ToList();
                    Db.UpdateRange(updated);
                    return Task.FromResult<IEnumerable<T>>(updated);
                default: throw new NotSupportedException(method?.Name);
            }
        }
    }
}
