using IAX.IXApi.Modules.Finance.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace IAX.IXApi.Modules.Finance.Inventory;

public sealed class InventPackagingGroupConfiguration : IEntityTypeConfiguration<InventPackagingGroup>
{
    public void Configure(EntityTypeBuilder<InventPackagingGroup> builder) =>
        builder.HasIndex(x => new { x.DataAreaId, x.PackagingGroupId }).IsUnique();
}
