using IAX.IXApi.Infrastructure.Persistence;
using IAX.IXApi.Modules.Finance.AccountsReceivable;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace IAX.IXApi.Tests;

public sealed class PriceDiscRelationshipTests
{
    [Fact]
    public void Trade_agreement_relation_fields_do_not_create_unconditional_foreign_keys()
    {
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseSqlite("Data Source=:memory:").Options;
        using var db = new ApplicationDbContext(options);
        var entity = db.Model.FindEntityType(typeof(PriceDiscTable));

        Assert.NotNull(entity);
        Assert.Empty(entity.GetForeignKeys());
    }
}
