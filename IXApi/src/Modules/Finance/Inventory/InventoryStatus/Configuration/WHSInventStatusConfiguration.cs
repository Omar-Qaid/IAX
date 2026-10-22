using IAX.IXApi.Modules.Finance.Inventory.InventoryStatus.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace IAX.IXApi.Modules.Finance.Inventory.InventoryStatus.Configuration;

public sealed class WHSInventStatusConfiguration : IEntityTypeConfiguration<WHSInventStatus>
{
    public void Configure(EntityTypeBuilder<WHSInventStatus> builder) =>
        builder.HasIndex(x => new { x.DataAreaId, x.InventStatusId }).IsUnique();
}
