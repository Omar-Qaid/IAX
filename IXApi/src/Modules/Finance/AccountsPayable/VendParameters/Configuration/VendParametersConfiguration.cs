using IAX.IXApi.Modules.Finance.Common;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace IAX.IXApi.Modules.Finance.AccountsPayable
{
    public class VendParametersConfiguration : IEntityTypeConfiguration<VendParameters>
    {
        public void Configure(EntityTypeBuilder<VendParameters> builder)
        {
            builder.ToTable("VendParameters");
            builder.HasIndex(x => new { x.DataAreaId, x.RecId }).IsUnique();
            // One parameters row per company, as on CustParameters: a second row makes the
            // company's posting profile default ambiguous.
            builder.HasIndex(x => new { x.DataAreaId, x.Key }).IsUnique();

            builder.Property(x => x.VendPostingProfile).HasMaxLength(FieldLengths.PostingProfile);
            builder.Property(x => x.PriceTotalMatchToleranceInPercent).HasColumnType("decimal(32, 6)");
            builder.Property(x => x.DataAreaId).HasMaxLength(4).HasDefaultValue("dat").IsRequired();
        }
    }
}
