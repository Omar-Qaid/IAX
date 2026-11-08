using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace IAX.IXApi.Modules.Finance.AccountsReceivable.CurrencyRevaluation;

public sealed class CustExchRateAdjustmentConfiguration : IEntityTypeConfiguration<CustExchRateAdjustment>
{
    public void Configure(EntityTypeBuilder<CustExchRateAdjustment> builder)
    {
        builder.ToTable("CustExchRateAdjustment");
        builder.Property(x => x.DataAreaId).HasMaxLength(10).IsRequired();
        builder.Property(x => x.PostingProfile).HasMaxLength(10);
        builder.Property(x => x.ConsideredDate).HasColumnType("date");
        builder.Property(x => x.DateOfRate).HasColumnType("date");
        builder.Property(x => x.ExecutedDate).HasColumnType("date");
    }
}
