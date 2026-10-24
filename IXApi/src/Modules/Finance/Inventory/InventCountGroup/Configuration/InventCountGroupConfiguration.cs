using IAX.IXApi.Modules.Finance.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace IAX.IXApi.Modules.Finance.Inventory;

public sealed class InventCountGroupConfiguration : IEntityTypeConfiguration<InventCountGroup>
{
    public void Configure(EntityTypeBuilder<InventCountGroup> builder) =>
        builder.HasIndex(x => new { x.DataAreaId, x.CountGroupId }).IsUnique();
}
