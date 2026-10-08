using IAX.IXApi.Modules.Finance.Common;
using IAX.IXApi.Modules.Finance.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace IAX.IXApi.Modules.Finance.Inventory.InventTable.Configuration;

public sealed class InventTransOriginSalesLineConfiguration : IEntityTypeConfiguration<InventTransOriginSalesLine>
{
    public void Configure(EntityTypeBuilder<InventTransOriginSalesLine> builder)
    {
        builder.Property(x => x.SalesLineDataAreaId).HasMaxLength(FieldLengths.CompanyId);
        builder.Property(x => x.SalesLineInventTransId).HasMaxLength(FieldLengths.InventTransId);
        builder.HasIndex(x => new { x.DataAreaId, x.InventTransOrigin });
        builder.HasIndex(x => new { x.SalesLineDataAreaId, x.SalesLineInventTransId });
    }
}
