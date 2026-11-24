using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace IAX.IXApi.Modules.Finance.AccountsPayable.CurrencyRevaluation;

public sealed class VendExchRateAdjustmentConfiguration : IEntityTypeConfiguration<VendExchRateAdjustment>
{
    public void Configure(EntityTypeBuilder<VendExchRateAdjustment> builder)
    {
        builder.ToTable("VendExchRateAdjustment");
        builder.Property(x => x.DataAreaId).HasMaxLength(10).IsRequired();
        builder.Property(x => x.PostingProfile).HasMaxLength(10);
        builder.Property(x => x.ConsideredDate).HasColumnType("date");
        builder.Property(x => x.DateOfRate).HasColumnType("date");
        builder.Property(x => x.ExecutedDate).HasColumnType("date");
    }
}
