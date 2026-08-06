using IAX.IXApi.Modules.ERP.Common;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace IAX.IXApi.Modules.ERP.AccountsReceivable
{
    public class SalesTableConfiguration : IEntityTypeConfiguration<SalesTable>
    {
        public void Configure(EntityTypeBuilder<SalesTable> builder)
        {
            builder.ToTable("SalesTable");

            builder.Property(x => x.DataAreaId)
                .HasMaxLength(10)
                .HasDefaultValue("dat")
                .IsRequired();

            builder.HasIndex(x => x.DataAreaId);
            builder.HasIndex(x => x.SalesId).IsUnique();

            builder.Property(x => x.SalesType)
                .HasConversion<string>()
                .HasMaxLength(50)
                .HasDefaultValue(SalesType.Sales)
                .IsRequired();

            builder.Property(x => x.InventSiteId)
                .HasMaxLength(50);

            builder.Property(x => x.InventLocationId)
                .HasMaxLength(50);

            builder.Property(x => x.DeliveryName)
                .HasMaxLength(250);


        

        }
    }
}
