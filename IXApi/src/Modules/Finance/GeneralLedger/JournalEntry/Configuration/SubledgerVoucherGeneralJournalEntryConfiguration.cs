using IAX.IXApi.Modules.Finance.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace IAX.IXApi.Modules.Finance.GeneralLedger.JournalEntry.Configuration;

public sealed class SubledgerVoucherGeneralJournalEntryConfiguration : IEntityTypeConfiguration<SubledgerVoucherGeneralJournalEntry>
{
    public void Configure(EntityTypeBuilder<SubledgerVoucherGeneralJournalEntry> builder)
    {
        builder.HasOne<GeneralJournalEntry>().WithMany().HasForeignKey(x => x.GeneralJournalEntry)
            .OnDelete(DeleteBehavior.Restrict);
        builder.HasIndex(x => new { x.VoucherDataAreaId, x.Voucher, x.AccountingDate });
    }
}
