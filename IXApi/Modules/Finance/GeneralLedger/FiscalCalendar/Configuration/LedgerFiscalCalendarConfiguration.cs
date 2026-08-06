using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using IAX.IXApi.Modules.Finance.Common;

namespace IAX.IXApi.Modules.Finance.Entities
{
    public class LedgerFiscalCalendarConfiguration
    {
        public class LedgerFiscalCalendarPeriodConfiguration : IEntityTypeConfiguration<LedgerFiscalCalendarPeriod>
        {
            public void Configure(EntityTypeBuilder<LedgerFiscalCalendarPeriod> builder)
            {
                builder.ToTable("LedgerFiscalCalendarPeriod");

                builder.HasKey(x => x.RecId);

                builder.HasOne(x => x.FiscalCalendarPeriodTable)
                   .WithMany()
                   .HasForeignKey(x => x.FiscalCalendarPeriod)
                   .HasPrincipalKey(x => x.RecId)
                   .OnDelete(DeleteBehavior.Restrict);

                builder.HasOne(x => x.LedgerTable)
                .WithMany()
                .HasForeignKey(x => x.Ledger)
                .HasPrincipalKey(x => x.RecId)
                .OnDelete(DeleteBehavior.Restrict);
            }
        }

       public class LedgerFiscalCalendarYearConfiguration : IEntityTypeConfiguration<LedgerFiscalCalendarYear>
       {
           public void Configure(EntityTypeBuilder<LedgerFiscalCalendarYear> builder)
           {
               builder.ToTable("LedgerFiscalCalendarYear");

               builder.HasKey(x => x.RecId);

               builder.HasOne(x => x.FiscalCalendarYearTable)
                  .WithMany()
                  .HasForeignKey(x => x.FiscalCalendarYear)
                  .HasPrincipalKey(x => x.RecId)
                  .OnDelete(DeleteBehavior.Restrict);

              builder.HasOne(x => x.LedgerTable)
                .WithMany()
                .HasForeignKey(x => x.Ledger)
                .HasPrincipalKey(x => x.RecId)
                .OnDelete(DeleteBehavior.Restrict);
            }
       }
    }
}

