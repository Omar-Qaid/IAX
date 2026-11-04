using IAX.IXApi.Modules.Finance.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
namespace IAX.IXApi.Modules.Finance.GeneralLedger.SubledgerAccounting;
public sealed class AccountingDistributionConfiguration : IEntityTypeConfiguration<AccountingDistribution>
{
    public void Configure(EntityTypeBuilder<AccountingDistribution> b)
    {
        b.HasOne<CompanyInfo>().WithMany().HasForeignKey(x => x.AccountingLegalEntity).OnDelete(DeleteBehavior.Restrict);
        b.HasOne<DimensionAttributeValueCombination>().WithMany().HasForeignKey(x => x.LedgerDimension).OnDelete(DeleteBehavior.Restrict);
        b.HasOne<Currency>().WithMany().HasForeignKey(x => x.TransactionCurrency).HasPrincipalKey(x => x.CurrencyCode).OnDelete(DeleteBehavior.Restrict);
        b.HasOne<AccountingDistribution>().WithMany().HasForeignKey(x => x.ParentDistribution).OnDelete(DeleteBehavior.Restrict);
        b.HasOne<AccountingDistribution>().WithMany().HasForeignKey(x => x.ReferenceDistribution).OnDelete(DeleteBehavior.Restrict);
        b.HasIndex(x => x.SourceDocumentLine);
        b.HasIndex(x => x.SourceDocumentHeader);
        b.Property(x => x.AllocationFactor).HasPrecision(28, 12);
        b.Property(x => x.TransactionCurrencyAmount).HasPrecision(28, 12);
    }
}
public sealed class SubledgerJournalEntryConfiguration : IEntityTypeConfiguration<SubledgerJournalEntry>
{
    public void Configure(EntityTypeBuilder<SubledgerJournalEntry> b)
    {
        b.HasOne<IAX.IXApi.Modules.Finance.Entities.Ledger>().WithMany().HasForeignKey(x => x.Ledger).OnDelete(DeleteBehavior.Restrict);
        b.HasOne<FiscalCalendarPeriod>().WithMany().HasForeignKey(x => x.FiscalCalendarPeriod).OnDelete(DeleteBehavior.Restrict);
        b.HasIndex(x => new { x.VoucherDataAreaId, x.Voucher });
        b.HasIndex(x => x.AccountingEvent);
    }
}
public sealed class SubledgerJournalAccountEntryConfiguration : IEntityTypeConfiguration<SubledgerJournalAccountEntry>
{
    public void Configure(EntityTypeBuilder<SubledgerJournalAccountEntry> b)
    {
        b.HasOne<SubledgerJournalEntry>().WithMany().HasForeignKey(x => x.SubledgerJournalEntry).OnDelete(DeleteBehavior.Restrict);
        b.HasOne<DimensionAttributeValueCombination>().WithMany().HasForeignKey(x => x.LedgerDimension).OnDelete(DeleteBehavior.Restrict);
        b.HasOne<Currency>().WithMany().HasForeignKey(x => x.TransactionCurrency).HasPrincipalKey(x => x.CurrencyCode).OnDelete(DeleteBehavior.Restrict);
        b.HasOne<GeneralJournalAccountEntry>().WithMany().HasForeignKey(x => x.GeneralJournalAccountEntry).OnDelete(DeleteBehavior.Restrict);
        foreach (var name in new[] { nameof(SubledgerJournalAccountEntry.TransactionCurrencyAmount), nameof(SubledgerJournalAccountEntry.AccountingCurrencyAmount),
            nameof(SubledgerJournalAccountEntry.ReportingCurrencyAmount), nameof(SubledgerJournalAccountEntry.ExchangeRate1), nameof(SubledgerJournalAccountEntry.ExchangeRate2),
            nameof(SubledgerJournalAccountEntry.ReportingExchangeRate1), nameof(SubledgerJournalAccountEntry.ReportingExchangeRate2) })
            b.Property(name).HasPrecision(28, 12);
    }
}
