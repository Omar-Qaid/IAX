using IAX.IXApi.Modules.Finance.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace IAX.IXApi.Modules.Finance.GeneralLedger;

public sealed class BankAccountTransConfiguration : IEntityTypeConfiguration<BankAccountTrans>
{
    public void Configure(EntityTypeBuilder<BankAccountTrans> b)
    {
        b.ToTable("BankAccountTrans");
        b.HasIndex(x => new { x.DataAreaId, x.SourceTableId, x.SourceRecId }).IsUnique()
            .HasFilter("[SourceTableId] > 0 AND [SourceRecId] > 0");
        b.HasIndex(x => new { x.DataAreaId, x.AccountId, x.TransDate });
        b.Property(x => x.AmountCur).HasPrecision(18, 4);
        b.Property(x => x.AmountMst).HasPrecision(18, 4);
        b.Property(x => x.AmountReportingCurrency).HasPrecision(18, 4);
        b.Property(x => x.BankTransAmountCur).HasPrecision(18, 4);
        b.Property(x => x.AmountCorrect).HasPrecision(18, 4);
        b.Property(x => x.ExchRateMst).HasPrecision(28, 12);
        b.Property(x => x.ExchRateRep).HasPrecision(28, 12);
    }
}
