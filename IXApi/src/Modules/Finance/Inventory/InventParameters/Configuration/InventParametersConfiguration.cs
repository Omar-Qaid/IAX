using IAX.IXApi.Modules.Finance.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace IAX.IXApi.Modules.Finance.Inventory;

public sealed class InventParametersConfiguration : IEntityTypeConfiguration<InventParameters>
{
    public void Configure(EntityTypeBuilder<InventParameters> builder)
    {
        builder.ToTable("InventParameters");
        builder.HasIndex(x => x.DataAreaId).IsUnique();
    }
}
