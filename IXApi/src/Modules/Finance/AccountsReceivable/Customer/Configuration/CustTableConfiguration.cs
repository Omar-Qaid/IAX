using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace IAX.IXApi.Modules.Finance.AccountsReceivable
{
    public class CustTableConfiguration : IEntityTypeConfiguration<CustTable>
    {
        public void Configure(EntityTypeBuilder<CustTable> builder)
        {
            builder.ToTable("CustTable");

            builder.Property(x => x.DataAreaId)
                .HasMaxLength(10)
                .HasDefaultValue("dat")
                .IsRequired();

            builder.HasIndex(x => x.DataAreaId);
            builder.HasIndex(x => new { x.DataAreaId, x.AccountNum }).IsUnique();

            builder.Property(x => x.AccountNum).HasMaxLength(20).IsRequired();
        }
    }
}

