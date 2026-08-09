using IAX.IXApi.Shared.Domain.Entities;
using IAX.IXApi.Modules.Finance.Entities;
using IAX.IXApi.Modules.Organization.Employees.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using System;
using IAX.IXApi.Modules.Finance.Common;

namespace IAX.IXApi.Modules.Finance.Entities
{
    public class FiscalCalendarPeriodConfiguration : IEntityTypeConfiguration<FiscalCalendarPeriod>
    {
        public void Configure(EntityTypeBuilder<FiscalCalendarPeriod> builder)
        {
            builder.ToTable("FiscalCalendarPeriod");
            builder.HasKey(x => x.RecId);

            builder.HasOne(x => x.FiscalCalendarTable)
                .WithMany()
                .HasForeignKey(x => x.FiscalCalendar)
                .HasPrincipalKey(x => x.RecId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.HasOne(x => x.FiscalCalendarYearTable)
                .WithMany()
                .HasForeignKey(x => x.FiscalCalendarYear)
                .HasPrincipalKey(x => x.RecId)
                .OnDelete(DeleteBehavior.Restrict);
        }
    }
}
