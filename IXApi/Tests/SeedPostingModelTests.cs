using IAX.IXApi.Infrastructure.Persistence;
using IAX.IXApi.Modules.Finance.Entities;
using IAX.IXApi.Modules.Workflow.Requests;
using IAX.IXApi.Modules.Finance.AccountsReceivable;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace IXApi.Tests;

public sealed class SeedPostingModelTests
{
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
