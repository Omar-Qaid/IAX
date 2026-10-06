using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace IAX.IXApi.Modules.Finance.AccountsReceivable
{
    public class PriceDiscGroupConfiguration : IEntityTypeConfiguration<PriceDiscGroup>
    {
        public void Configure(EntityTypeBuilder<PriceDiscGroup> builder)
        {
            builder.ToTable("PriceDiscGroup");
            builder.HasIndex(x => new { x.DataAreaId, x.RecId }).IsUnique();
            builder.HasIndex(x => new { x.DataAreaId, x.GroupId, x.Module, x.Type }).IsUnique();

            builder.Property(x => x.GroupId).HasMaxLength(10).IsRequired();
            builder.Property(x => x.Name).HasMaxLength(150);
            builder.Property(x => x.DataAreaId).HasMaxLength(4).HasDefaultValue("dat").IsRequired();
        }
    }
}
