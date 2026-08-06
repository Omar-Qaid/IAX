using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace IAX.IXApi.Modules.Finance.AccountsReceivable
{
    public class CustLedgerConfiguration : IEntityTypeConfiguration<CustLedger>
    {
        public void Configure(EntityTypeBuilder<CustLedger> builder)
        {
            builder.ToTable("CustLedger");

            builder.HasIndex(t => t.PostingProfile).IsUnique();

            builder.Property(x => x.DataAreaId)
                .HasMaxLength(10)
                .HasDefaultValue("dat")
                .IsRequired();

            builder.HasIndex(x => x.DataAreaId);
        }
    }

    public class CustLedgerAccountsConfiguration : IEntityTypeConfiguration<CustLedgerAccounts>
    {
        public void Configure(EntityTypeBuilder<CustLedgerAccounts> builder)
        {
            builder.ToTable("CustLedgerAccounts");

            builder.HasIndex(t => new { t.PostingProfile, t.AccountCode, t.Num });
   

            builder.Property(x => x.DataAreaId)
                .HasMaxLength(10)
                .HasDefaultValue("dat")
                .IsRequired();

            builder.HasIndex(x => x.DataAreaId);


            //-------------------------------------------------------
            // CustLedger
            // CustLedgerAccounts.PostingProfile == CustLedger.PostingProfile
            //-------------------------------------------------------

            builder.HasOne(e => e.CustLedgerTable)
                .WithMany()
                .HasForeignKey(e => e.PostingProfile)
                .HasPrincipalKey(e => e.PostingProfile)
                .OnDelete(DeleteBehavior.NoAction);

            //-------------------------------------------------------
            // Num is a polymorphic field (holds CustGroupId when AccountCode=Group,
            // AccountNum when AccountCode=Table, or empty string when AccountCode=All).
            // Ignore physical DB Foreign Keys on Num and SummaryLedgerDimension to prevent DB constraint conflicts.
            //-------------------------------------------------------
            builder.Ignore(e => e.CustGroupTable);
            builder.Ignore(e => e.CustTable);
            builder.Ignore(e => e.SummaryDimensionAttributeValueCombinationTable);
        }
    }
}

