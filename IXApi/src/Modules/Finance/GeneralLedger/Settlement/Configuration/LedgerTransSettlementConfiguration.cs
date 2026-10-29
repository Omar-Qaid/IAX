using IAX.IXApi.Modules.Finance.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace IAX.IXApi.Modules.Finance.GeneralLedger.Settlement
{
    public class LedgerTransSettlementConfiguration : IEntityTypeConfiguration<LedgerTransSettlement>
    {
        public void Configure(EntityTypeBuilder<LedgerTransSettlement> builder)
        {
            builder.ToTable("LedgerTransSettlement");
            // A posted ledger entry is settled once while the settlement stands.
            builder.HasIndex(x => new { x.DataAreaId, x.TransRecId });
            builder.HasIndex(x => new { x.DataAreaId, x.SettleId });
            builder.HasOne<GeneralJournalAccountEntry>().WithMany()
                .HasForeignKey(x => x.TransRecId).OnDelete(DeleteBehavior.Restrict);
            builder.Property(x => x.SettleTransactionCurrencyAmount).HasPrecision(28, 6);
            builder.Property(x => x.SettleAccountingCurrencyAmount).HasPrecision(28, 6);
            builder.Property(x => x.SettleReportingCurrencyAmount).HasPrecision(28, 6);
            builder.Property(x => x.ExchAdjAccountingCurrencyAmount).HasPrecision(28, 6);
            builder.Property(x => x.ExchAdjReportingCurrencyAmount).HasPrecision(28, 6);
        }
    }
}
