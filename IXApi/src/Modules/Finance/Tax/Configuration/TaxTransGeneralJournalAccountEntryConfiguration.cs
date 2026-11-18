using IAX.IXApi.Modules.Finance.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace IAX.IXApi.Modules.Finance.Tax;

public sealed class TaxTransGeneralJournalAccountEntryConfiguration
    : IEntityTypeConfiguration<TaxTransGeneralJournalAccountEntry>
{
    public void Configure(EntityTypeBuilder<TaxTransGeneralJournalAccountEntry> builder)
    {
        builder.ToTable("TaxTransGeneralJournalAccountEntry");
        builder.HasKey(x => x.RecId);
        builder.Property(x => x.RowVersion).HasColumnName("SysRowVersion").IsRowVersion();
        // Preserve the six fractional digits in the supplied D365 table export.
        builder.Property(x => x.AccountingCurrencyAmount).HasPrecision(32, 6);
        builder.Property(x => x.TransactionCurrencyAmount).HasPrecision(32, 6);
        builder.Property(x => x.AllocationFactor).HasPrecision(32, 6);

        builder.HasIndex(x => new { x.DataAreaId, x.GeneralJournalAccountEntry });
        builder.HasIndex(x => new { x.DataAreaId, x.TaxTrans });
        builder.HasIndex(x => new { x.DataAreaId, x.SubledgerJournalAccountEntry });
        // D365 reference columns can contain 0 (including the supplied sample).
        // Do not turn those optional references into mandatory SQL foreign keys.
    }
}
