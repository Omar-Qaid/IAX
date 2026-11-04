using IAX.IXApi.Modules.Finance.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
namespace IAX.IXApi.Modules.Finance.AccountsPayable;
public sealed class PurchParametersConfiguration : IEntityTypeConfiguration<PurchParameters>
{
    public void Configure(EntityTypeBuilder<PurchParameters> builder) => builder.HasIndex(x => x.DataAreaId).IsUnique();
}
