using IAX.IXApi.Modules.Finance.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace IAX.IXApi.Modules.Finance.Inventory.ItemGroup;

public sealed class InventItemGroupItemConfiguration : IEntityTypeConfiguration<InventItemGroupItem>
{
    public void Configure(EntityTypeBuilder<InventItemGroupItem> builder)
    {
        builder.HasIndex(item => new { item.ItemId, item.ItemDataAreaId }).IsUnique();
        builder.HasIndex(item => new { item.ItemGroupId, item.ItemGroupDataAreaId });
    }
}

