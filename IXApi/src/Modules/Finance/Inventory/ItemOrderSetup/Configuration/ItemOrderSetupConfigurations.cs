using IAX.IXApi.Modules.Finance.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace IAX.IXApi.Modules.Finance.Inventory.ItemOrderSetup.Configuration;

public sealed class InventItemPurchSetupConfiguration : IEntityTypeConfiguration<InventItemPurchSetup>
{
    public void Configure(EntityTypeBuilder<InventItemPurchSetup> builder) =>
        builder.HasIndex(setup => new { setup.ItemId, setup.ItemDataAreaId, setup.InventDimId }).IsUnique();
}

public sealed class InventItemSalesSetupConfiguration : IEntityTypeConfiguration<InventItemSalesSetup>
{
    public void Configure(EntityTypeBuilder<InventItemSalesSetup> builder) =>
        builder.HasIndex(setup => new { setup.ItemId, setup.ItemDataAreaId, setup.InventDimId }).IsUnique();
}

public sealed class InventItemInventSetupConfiguration : IEntityTypeConfiguration<InventItemInventSetup>
{
    public void Configure(EntityTypeBuilder<InventItemInventSetup> builder) =>
        builder.HasIndex(setup => new { setup.ItemId, setup.ItemDataAreaId, setup.InventDimId }).IsUnique();
}

public sealed class InventItemSetupSupplyTypeConfiguration : IEntityTypeConfiguration<InventItemSetupSupplyType>
{
    public void Configure(EntityTypeBuilder<InventItemSetupSupplyType> builder) =>
        builder.HasIndex(setup => new { setup.ItemId, setup.ItemDataAreaId }).IsUnique();
}
