using IAX.IXApi.Modules.Finance.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
namespace IAX.IXApi.Modules.Finance.GeneralLedger.SubledgerAccounting;
public sealed class SourceDocumentHeaderConfiguration : IEntityTypeConfiguration<SourceDocumentHeader>
{
    public void Configure(EntityTypeBuilder<SourceDocumentHeader> b)
    {
        b.HasIndex(x => new { x.SourceRelationType, x.TypeEnumValue });
    }
}
public sealed class SourceDocumentLineConfiguration : IEntityTypeConfiguration<SourceDocumentLine>
{
    public void Configure(EntityTypeBuilder<SourceDocumentLine> b)
    {
        b.HasOne<SourceDocumentHeader>().WithMany().HasForeignKey(x => x.SourceDocumentHeader).OnDelete(DeleteBehavior.Restrict);
        b.HasOne<SourceDocumentLine>().WithMany().HasForeignKey(x => x.ParentSourceDocumentLine).OnDelete(DeleteBehavior.Restrict);
        b.Property(x => x.TermQuantity).HasPrecision(28, 12);
    }
}
public sealed class AccountingEventConfiguration : IEntityTypeConfiguration<AccountingEvent>
{
    public void Configure(EntityTypeBuilder<AccountingEvent> b)
    {
        b.HasOne<SourceDocumentHeader>().WithMany().HasForeignKey(x => x.SourceDocumentHeader).OnDelete(DeleteBehavior.Restrict);
        b.HasIndex(x => new { x.AccountingDate, x.Type, x.SourceDocumentHeader });
    }
}
public sealed class SubledgerJournalAccountEntryDistributionConfiguration : IEntityTypeConfiguration<SubledgerJournalAccountEntryDistribution>
{
    public void Configure(EntityTypeBuilder<SubledgerJournalAccountEntryDistribution> b)
    {
        b.HasOne<AccountingDistribution>().WithMany().HasForeignKey(x => x.AccountingDistribution).OnDelete(DeleteBehavior.Restrict);
        b.HasOne<AccountingDistribution>().WithMany().HasForeignKey(x => x.ParentDistribution).OnDelete(DeleteBehavior.Restrict);
        b.HasOne<SubledgerJournalAccountEntry>().WithMany().HasForeignKey(x => x.SubledgerJournalAccountEntry).OnDelete(DeleteBehavior.Restrict);
        b.Property(x => x.AccountingCurrencyAmount).HasPrecision(28, 12);
        b.Property(x => x.ReportingCurrencyAmount).HasPrecision(28, 12);
    }
}
