using IAX.IXApi.Infrastructure.Persistence;
using IAX.IXApi.Modules.Finance.Entities;
using IAX.IXApi.Modules.Workflow.Requests;
using IAX.IXApi.Modules.Finance.AccountsReceivable;
using Microsoft.EntityFrameworkCore;
using Xunit;
using System.Text.Json;

namespace IXApi.Tests;

public sealed class SeedPostingModelTests
{
    [Fact]
    public void Every_mapped_table_is_in_the_seed_coverage_review()
    {
        using var db = new ApplicationDbContext(new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseSqlServer("Server=localhost;Database=ModelOnly;Integrated Security=true;TrustServerCertificate=true").Options);
        using var stream = typeof(SeedPostingModelTests).Assembly.GetManifestResourceStream("IXApi.Tests.SeedMappedTables.json")!;
        var reviewed = JsonSerializer.Deserialize<string[]>(stream)!;
        var actual = db.Model.GetEntityTypes().Select(x => x.GetTableName()).Where(x => x != null)
            .Distinct().OrderBy(x => x, StringComparer.Ordinal).ToArray();
        Assert.Equal(reviewed.OrderBy(x => x, StringComparer.Ordinal), actual);
    }

    [Fact]
    public void AP_transaction_storage_has_documented_nullability_precision_and_restrictive_relationships()
    {
        using var db = new ApplicationDbContext(new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseSqlServer("Server=localhost;Database=ModelOnly;Integrated Security=true;TrustServerCertificate=true").Options);
        foreach (var type in new[] { typeof(AccountingDistribution), typeof(SubledgerJournalEntry), typeof(SubledgerJournalAccountEntry) })
        {
            Assert.False(typeof(IAX.IXApi.Shared.Domain.Entities.IMultiCompany).IsAssignableFrom(type));
            Assert.All(db.Model.FindEntityType(type)!.GetForeignKeys(), fk => Assert.Equal(DeleteBehavior.Restrict, fk.DeleteBehavior));
        }
        var entry = db.Model.FindEntityType(typeof(SubledgerJournalAccountEntry))!;
        Assert.False(entry.FindProperty(nameof(SubledgerJournalAccountEntry.LedgerDimension))!.IsNullable);
        Assert.True(entry.FindProperty(nameof(SubledgerJournalAccountEntry.GeneralJournalAccountEntry))!.IsNullable);
        Assert.Equal(28, entry.FindProperty(nameof(SubledgerJournalAccountEntry.TransactionCurrencyAmount))!.GetPrecision());
        Assert.Equal(12, entry.FindProperty(nameof(SubledgerJournalAccountEntry.TransactionCurrencyAmount))!.GetScale());
        var subline = db.Model.FindEntityType(typeof(IAX.IXApi.Modules.Finance.AccountsPayable.VendInvoiceInfoSubLine))!;
        Assert.Contains(subline.GetForeignKeys(), fk => fk.Properties.Single().Name == "LineRefRecId");
        Assert.DoesNotContain(subline.GetForeignKeys(), fk => fk.Properties.Any(p => p.Name == "JournalRefRecId"));
    }

    [Fact]
    public void Posting_links_and_multiple_request_variables_have_persistable_keys()
    {
        using var db = new ApplicationDbContext(new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseSqlServer("Server=localhost;Database=ModelOnly;Integrated Security=true;TrustServerCertificate=true").Options);
        var link = db.Model.FindEntityType(typeof(SubledgerVoucherGeneralJournalEntry));
        Assert.NotNull(link);
        Assert.Contains(link!.GetForeignKeys(), x => x.PrincipalEntityType.ClrType == typeof(GeneralJournalEntry));
        var variable = db.Model.FindEntityType(typeof(WfRequestVariable))!;
        Assert.Equal(nameof(WfRequestVariable.RecId), Assert.Single(variable.FindPrimaryKey()!.Properties).Name);
        Assert.Contains(variable.GetIndexes(), x => x.IsUnique && x.Properties.Select(p => p.Name)
            .SequenceEqual(new[] { nameof(WfRequestVariable.RequestId), nameof(WfRequestVariable.VariableId) }));
        Assert.False(db.Database.HasPendingModelChanges());
        Assert.True(db.Model.FindEntityType(typeof(PriceDiscAdmTrans))!.FindProperty(nameof(PriceDiscAdmTrans.PriceDiscTableRef))!.IsNullable);
        Assert.Null(new PriceDiscAdmTrans { PriceDiscTableRef = 0 }.PriceDiscTableRef);
    }
}
