using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace IAX.IXApi.Modules.Finance.AccountsReceivable
{
    public class SalesPoolConfiguration : IEntityTypeConfiguration<SalesPool>
    {
        public void Configure(EntityTypeBuilder<SalesPool> builder)
        {
            builder.ToTable("SalesPool");
            builder.HasIndex(x => new { x.DataAreaId, x.RecId }).IsUnique();
            builder.HasIndex(x => x.SalesPoolId).IsUnique();
            builder.Property(x => x.SalesPoolId).HasMaxLength(10).IsRequired();
            builder.Property(x => x.DataAreaId).HasMaxLength(10).HasDefaultValue("dat").IsRequired();
        }
    }
}

