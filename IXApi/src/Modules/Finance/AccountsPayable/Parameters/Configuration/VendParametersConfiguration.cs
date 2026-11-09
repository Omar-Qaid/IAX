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
            builder.Property(x => x.AdvancePostingProfile).HasMaxLength(FieldLengths.PostingProfile);
            builder.Property(x => x.TotalPriceTolerancePercentage).HasColumnType("decimal(32, 6)");
            // The column defaults mirror the entity initializers: a row written before these
            // fields existed must keep matching enabled, not silently stop matching.
            builder.Property(x => x.UseInvoiceMatching).HasDefaultValue(NoYes.Yes);
            builder.Property(x => x.UseTotalPriceMatching).HasDefaultValue(NoYes.Yes);
            builder.Property(x => x.MatchingPolicy).HasDefaultValue(MatchingPolicy.None);
            builder.Property(x => x.MatchingPolicyAllowOverride).HasDefaultValue(NoYes.Yes);
            // Net unit price matching takes D365's own default of No: it never ran before, so an
            // existing row gains nothing it was not already doing.
            builder.Property(x => x.UseExtendedPriceMatching).HasDefaultValue(NoYes.No);
            builder.Property(x => x.ExtendedPriceTolerancePercentage).HasColumnType("decimal(32, 6)");
            builder.Property(x => x.ExtendedPriceToleranceAmount).HasColumnType("decimal(32, 6)");
            builder.Property(x => x.DataAreaId).HasMaxLength(4).HasDefaultValue("dat").IsRequired();
        }
    }
}
