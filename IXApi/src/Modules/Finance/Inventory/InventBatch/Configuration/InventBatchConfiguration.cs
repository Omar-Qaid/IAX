using IAX.IXApi.Modules.Finance.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace IAX.IXApi.Modules.Finance.Inventory;

public sealed class InventBatchConfiguration : IEntityTypeConfiguration<InventBatch>
{
    public void Configure(EntityTypeBuilder<InventBatch> builder) =>
        builder.HasIndex(x => new { x.DataAreaId, x.ItemId, x.InventBatchId }).IsUnique();
}
