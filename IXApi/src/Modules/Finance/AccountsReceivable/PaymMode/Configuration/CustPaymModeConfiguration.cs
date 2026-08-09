using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace IAX.IXApi.Modules.Finance.AccountsReceivable
{
    public class CustPaymModeConfiguration : IEntityTypeConfiguration<CustPaymModeTable>
    {
        public void Configure(EntityTypeBuilder<CustPaymModeTable> builder)
        {
            builder.ToTable("CustPaymModeTable");

            builder.Property(x => x.DataAreaId)
                .HasMaxLength(10)
                .HasDefaultValue("dat")
                .IsRequired();

            builder.HasIndex(x => x.DataAreaId);
            builder.HasIndex(x => new { x.DataAreaId, x.PaymMode }).IsUnique();
            builder.Property(x => x.PaymMode).HasMaxLength(10).IsRequired();
        }
    }
}

