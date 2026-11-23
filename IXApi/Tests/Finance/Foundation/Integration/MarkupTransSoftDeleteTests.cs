using IAX.IXApi.Modules.Finance.AccountsReceivable;
using IAX.IXApi.Modules.Finance.Entities;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace IAX.IXApi.Tests;

/// <summary>
/// MarkupTrans takes part in the global soft-delete filter.
///
/// It used to declare <c>public new NoYes IsDeleted</c>, shadowing the base entity's bool. The
/// filter in ApplicationDbContext.SetGlobalQueryFilters is only applied when the property's CLR
/// type is bool, so this one table silently had no filter at all: a soft-deleted charge stayed
/// visible to every query, including the posting paths once charges began to post. These tests pin
/// the property's shape, because that shape is the whole defect — a future redeclaration would
/// disable the filter again without breaking anything else.
/// </summary>
public class MarkupTransSoftDeleteTests
{
    [Fact]
    public void IsDeleted_is_the_base_entity_bool_and_is_not_redeclared()
    {
        var property = typeof(MarkupTrans).GetProperty(nameof(MarkupTrans.IsDeleted))!;

        Assert.Equal(typeof(bool), property.PropertyType);
        // Declared on the base entity, not on MarkupTrans: anything else is the shadow returning.
        Assert.NotEqual(typeof(MarkupTrans), property.DeclaringType);
    }

    /// <summary>
    /// The condition the filter itself tests. ApplicationDbContext builds the soft-delete filter
    /// only for a bool IsDeleted, so this is what decides whether MarkupTrans is covered.
    /// </summary>
    [Fact]
    public void The_mapped_column_is_a_bool_so_the_global_filter_applies()
    {
        using var db = new ApplicationDbContextProbe();

        var property = db.Model.FindEntityType(typeof(MarkupTrans))!.FindProperty("IsDeleted")!;

        Assert.Equal(typeof(bool), property.ClrType);
    }

    [Fact]
    public async Task A_soft_deleted_charge_is_filtered_out_of_queries()
    {
        using var db = new ApplicationDbContextProbe();
        await db.Database.EnsureCreatedAsync();
        db.AddRange(
            new MarkupTrans { RecId = 1, DataAreaId = "dat", MarkupCode = "FREIGHT", Value = 50m },
            new MarkupTrans { RecId = 2, DataAreaId = "dat", MarkupCode = "HANDLING", Value = 30m, IsDeleted = true });
        await db.SaveChangesAsync();
        db.ChangeTracker.Clear();

        var visible = await db.Set<MarkupTrans>().ToListAsync();

        Assert.Equal("FREIGHT", Assert.Single(visible).MarkupCode);
        // Still there, just filtered: soft delete, not a hard delete.
        Assert.Equal(2, (await db.Set<MarkupTrans>().IgnoreQueryFilters().ToListAsync()).Count);
    }

    /// <summary>
    /// A context that models MarkupTrans the way the application does — the same soft-delete filter
    /// expression — so the filter is exercised rather than described.
    /// </summary>
    private sealed class ApplicationDbContextProbe : DbContext
    {
        protected override void OnConfiguring(DbContextOptionsBuilder options) =>
            options.UseSqlite($"Data Source=file:markup{Guid.NewGuid():N}?mode=memory&cache=shared");

        protected override void OnModelCreating(ModelBuilder builder)
        {
            var entity = builder.Entity<MarkupTrans>();
            entity.HasKey(x => x.RecId);
            entity.Ignore("RowVersion");
            foreach (var property in typeof(MarkupTrans).GetProperties())
                if (property.PropertyType != typeof(string) && property.PropertyType != typeof(byte[])
                    && !property.PropertyType.IsValueType)
                    entity.Ignore(property.Name);

            // The same filter ApplicationDbContext applies, and only applies, to a bool IsDeleted.
            var mapped = builder.Model.FindEntityType(typeof(MarkupTrans))!.FindProperty("IsDeleted");
            if (mapped != null && mapped.ClrType == typeof(bool))
                entity.HasQueryFilter(x => !x.IsDeleted);
        }
    }
}
