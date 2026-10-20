using IAX.IXApi.Modules.Finance.Inventory.DimensionGroups.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace IAX.IXApi.Modules.Finance.Inventory.DimensionGroups.Configuration;

public sealed class EcoResStorageDimensionGroupConfiguration
    : IEntityTypeConfiguration<EcoResStorageDimensionGroup>
{
    public void Configure(EntityTypeBuilder<EcoResStorageDimensionGroup> builder)
    {
        builder.HasIndex(group => group.Name).IsUnique();
    }
}

public sealed class EcoResStorageDimensionGroupItemConfiguration
    : IEntityTypeConfiguration<EcoResStorageDimensionGroupItem>
{
    public void Configure(EntityTypeBuilder<EcoResStorageDimensionGroupItem> builder)
    {
        builder.HasIndex(item => new { item.ItemId, item.ItemDataAreaId }).IsUnique();
        builder.HasIndex(item => item.StorageDimensionGroup);
    }
}

public sealed class EcoResTrackingDimensionGroupConfiguration
    : IEntityTypeConfiguration<EcoResTrackingDimensionGroup>
{
    public void Configure(EntityTypeBuilder<EcoResTrackingDimensionGroup> builder)
    {
        builder.HasIndex(group => group.Name).IsUnique();
    }
}

public sealed class EcoResTrackingDimensionGroupItemConfiguration
    : IEntityTypeConfiguration<EcoResTrackingDimensionGroupItem>
{
    public void Configure(EntityTypeBuilder<EcoResTrackingDimensionGroupItem> builder)
    {
        builder.HasIndex(item => new { item.ItemId, item.ItemDataAreaId }).IsUnique();
        builder.HasIndex(item => item.TrackingDimensionGroup);
    }
}

public sealed class EcoResStorageDimensionGroupFldSetupConfiguration
    : IEntityTypeConfiguration<EcoResStorageDimensionGroupFldSetup>
{
    public void Configure(EntityTypeBuilder<EcoResStorageDimensionGroupFldSetup> builder)
    {
        builder.HasIndex(x => new { x.StorageDimensionGroup, x.DimensionFieldId }).IsUnique();
        builder.HasIndex(x => new { x.StorageDimensionGroup, x.LineNumber }).IsUnique();
    }
}

public sealed class EcoResTrackingDimensionGroupFldSetupConfiguration
    : IEntityTypeConfiguration<EcoResTrackingDimensionGroupFldSetup>
{
    public void Configure(EntityTypeBuilder<EcoResTrackingDimensionGroupFldSetup> builder)
    {
        builder.HasIndex(x => new { x.TrackingDimensionGroup, x.DimensionFieldId }).IsUnique();
        builder.HasIndex(x => new { x.TrackingDimensionGroup, x.LineNumber }).IsUnique();
    }
}
