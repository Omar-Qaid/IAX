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
