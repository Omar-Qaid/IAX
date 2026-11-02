using IAX.IXApi.Modules.Finance.Common;
using IAX.IXApi.Modules.Finance.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace IAX.IXApi.Modules.Finance.GeneralLedger.Ledger.Configuration
{
    public sealed class LedgerGainLossAccountConfiguration : IEntityTypeConfiguration<LedgerGainLossAccount>
    {
        public void Configure(EntityTypeBuilder<LedgerGainLossAccount> builder)
        {
            builder.ToTable("LedgerGainLossAccount");
            builder.HasIndex(x => new { x.DataAreaId, x.Ledger, x.AccountType }).IsUnique();
        }
    }

    public sealed class CurrencyLedgerGainLossAccountConfiguration
        : IEntityTypeConfiguration<CurrencyLedgerGainLossAccount>
    {
        public void Configure(EntityTypeBuilder<CurrencyLedgerGainLossAccount> builder)
        {
            builder.ToTable("CurrencyLedgerGainLossAccount");
            builder.Property(x => x.CurrencyCode).HasMaxLength(FieldLengths.CurrencyCode);
            builder.HasIndex(x => new { x.DataAreaId, x.Ledger, x.CurrencyCode, x.AccountType }).IsUnique();
        }
    }
}
