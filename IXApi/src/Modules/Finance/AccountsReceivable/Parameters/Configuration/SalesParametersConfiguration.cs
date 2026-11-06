using IAX.IXApi.Modules.Finance.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
namespace IAX.IXApi.Modules.Finance.AccountsReceivable;
public sealed class SalesParametersConfiguration : IEntityTypeConfiguration<SalesParameters>
{
    public void Configure(EntityTypeBuilder<SalesParameters> builder) => builder.HasIndex(x => x.DataAreaId).IsUnique();
}
