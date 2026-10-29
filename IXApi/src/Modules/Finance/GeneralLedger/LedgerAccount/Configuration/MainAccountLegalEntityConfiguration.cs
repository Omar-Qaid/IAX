using IAX.IXApi.Modules.Finance.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace IAX.IXApi.Modules.Finance.GeneralLedger.LedgerAccount
{
    public class MainAccountLegalEntityConfiguration : IEntityTypeConfiguration<MainAccountLegalEntity>
    {
        public void Configure(EntityTypeBuilder<MainAccountLegalEntity> builder)
        {
            builder.ToTable("MainAccountLegalEntity");
            // One override per account and company.
            builder.HasIndex(x => new { x.MainAccount, x.LegalEntity }).IsUnique();
            builder.HasOne<MainAccount>().WithMany()
                .HasForeignKey(x => x.MainAccount).OnDelete(DeleteBehavior.Cascade);
        }
    }
}
