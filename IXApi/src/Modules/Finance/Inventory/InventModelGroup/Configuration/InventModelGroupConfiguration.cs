using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using IAX.IXApi.Modules.Finance.Entities;

namespace IAX.IXApi.Modules.Finance.Inventory.InventModelGroup;

public sealed class InventModelGroupConfiguration : IEntityTypeConfiguration<IAX.IXApi.Modules.Finance.Entities.InventModelGroup>
{
    public void Configure(EntityTypeBuilder<IAX.IXApi.Modules.Finance.Entities.InventModelGroup> builder)
    {
        builder.HasIndex(group => new { group.ModelGroupId, group.DataAreaId }).IsUnique();
    }
}

public sealed class InventModelGroupItemConfiguration : IEntityTypeConfiguration<IAX.IXApi.Modules.Finance.Entities.InventModelGroupItem>
{
    public void Configure(EntityTypeBuilder<IAX.IXApi.Modules.Finance.Entities.InventModelGroupItem> builder)
    {
        builder.HasIndex(item => new { item.ItemId, item.ItemDataAreaId }).IsUnique();
        builder.HasIndex(item => new { item.ModelGroupId, item.ModelGroupDataAreaId });
    }
}
