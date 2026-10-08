using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace IAX.IXApi.Modules.Finance.AccountsReceivable.PriceDisc
{
    public class PriceDiscAdmNameConfiguration : IEntityTypeConfiguration<PriceDiscAdmName>
    {
        public void Configure(EntityTypeBuilder<PriceDiscAdmName> builder)
        {
            builder.ToTable("PriceDiscAdmName");
            builder.HasIndex(x => new { x.DataAreaId, x.RecId }).IsUnique();
            builder.HasIndex(x => new { x.DataAreaId, x.JournalName }).IsUnique();

            builder.Property(x => x.JournalName).HasMaxLength(10).IsRequired();
            builder.Property(x => x.Name).HasMaxLength(150);
            builder.Property(x => x.DataAreaId).HasMaxLength(4).HasDefaultValue("dat").IsRequired();
        }
    }
}
