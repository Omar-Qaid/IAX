using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace IAX.IXApi.Modules.ERP.AccountsReceivable
{
    public class CustGroupConfiguration : IEntityTypeConfiguration<CustGroup>
    {
        public void Configure(EntityTypeBuilder<CustGroup> builder)
        {
            builder.ToTable("CustGroup");

            builder.Property(x => x.DataAreaId)
                .HasMaxLength(10)
                .HasDefaultValue("dat")
                .IsRequired();

            builder.HasIndex(x => x.DataAreaId);
            builder.HasIndex(x => new { x.DataAreaId, x.CustGroupId }).IsUnique();

            builder.Property(x => x.CustGroupId).HasMaxLength(10).IsRequired();
            builder.Property(x => x.Name).HasMaxLength(60);
            builder.Property(x => x.PaymTermId).HasMaxLength(10);
        }
    }
}
