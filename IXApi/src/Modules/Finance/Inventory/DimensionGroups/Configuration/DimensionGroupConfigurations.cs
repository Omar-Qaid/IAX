using IAX.IXApi.Modules.Finance.Inventory.DimensionGroups.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace IAX.IXApi.Modules.Finance.Inventory.DimensionGroups.Configuration;

public sealed class EcoResStorageDimensionGroupConfiguration
    : IEntityTypeConfiguration<EcoResStorageDimensionGroup>
{
    public void Configure(EntityTypeBuilder<EcoResStorageDimensionGroup> builder)
    {
        builder.HasIndex(group => group.StorageDimensionGroupId).IsUnique();
    }
}

public sealed class EcoResStorageDimensionGroupItemConfiguration
    : IEntityTypeConfiguration<EcoResStorageDimensionGroupItem>
{
    public void Configure(EntityTypeBuilder<EcoResStorageDimensionGroupItem> builder)
    {
        builder.HasIndex(item => new { item.ItemId, item.ItemDataAreaId }).IsUnique();
        builder.HasIndex(item => item.StorageDimensionGroupId);
    }
}

public sealed class EcoResTrackingDimensionGroupConfiguration
    : IEntityTypeConfiguration<EcoResTrackingDimensionGroup>
{
    public void Configure(EntityTypeBuilder<EcoResTrackingDimensionGroup> builder)
    {
        builder.HasIndex(group => group.TrackingDimensionGroupId).IsUnique();
    }
}

public sealed class EcoResTrackingDimensionGroupItemConfiguration
    : IEntityTypeConfiguration<EcoResTrackingDimensionGroupItem>
{
    public void Configure(EntityTypeBuilder<EcoResTrackingDimensionGroupItem> builder)
    {
        builder.HasIndex(item => new { item.ItemId, item.ItemDataAreaId }).IsUnique();
        builder.HasIndex(item => item.TrackingDimensionGroupId);
    }
}
