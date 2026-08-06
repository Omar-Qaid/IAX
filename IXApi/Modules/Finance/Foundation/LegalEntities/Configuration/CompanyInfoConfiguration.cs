using IAX.IXApi.Shared.Domain.Entities;
using IAX.IXApi.Modules.Finance.Entities;
using IAX.IXApi.Modules.Organization.Employees.Entities;
using IAX.IXApi.Modules.Administration.AuditLogs.Entities;
using IAX.IXApi.Modules.Administration.DataManagement.Contracts;
using IAX.IXApi.Infrastructure.Persistence.Seeding.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using System.Reflection.Emit;

namespace IAX.IXApi.Modules.Finance.Shared.Features
{
    public class CompanyInfoConfiguration : IEntityTypeConfiguration<CompanyInfo>
    {
        public void Configure(EntityTypeBuilder<CompanyInfo> builder)
        {
            builder.ToTable("CompanyInfo");

            builder.HasKey(x => x.RecId);

            // Party -> DirPartyTable
            builder.HasOne(x => x.DirPartyTable)
                   .WithMany()
                   .HasForeignKey(x => x.Party)
                   .HasPrincipalKey(x => x.RecId)
                   .OnDelete(DeleteBehavior.NoAction);

            // Calendar -> FiscalCalendar
            builder.HasOne(x => x.FiscalCalendarTable)
                   .WithMany()
                   .HasForeignKey(x => x.Calendar)
                   .HasPrincipalKey(x => x.RecId)
                   .OnDelete(DeleteBehavior.NoAction);

            // CurrencyCode -> Currency
            builder.HasOne(x => x.Currency)
                   .WithMany()
                   .HasForeignKey(x => x.CurrencyCode)
                   .HasPrincipalKey(x => x.CurrencyCode)
                   .OnDelete(DeleteBehavior.NoAction);



        }
       
    }


}


