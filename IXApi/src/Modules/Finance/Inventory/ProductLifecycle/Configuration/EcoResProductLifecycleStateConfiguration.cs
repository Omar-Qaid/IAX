using IAX.IXApi.Modules.Finance.Inventory.ProductLifecycle.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace IAX.IXApi.Modules.Finance.Inventory.ProductLifecycle.Configuration;

public sealed class EcoResProductLifecycleStateConfiguration : IEntityTypeConfiguration<EcoResProductLifecycleState>
{
    public void Configure(EntityTypeBuilder<EcoResProductLifecycleState> builder)
    {
        builder.HasIndex(x => x.StateId).IsUnique();
        builder.HasIndex(x => x.IsDefaultOnProductRelease).IsUnique()
            .HasFilter("[IsDefaultOnProductRelease] = 1");
    }
}
