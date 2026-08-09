using IAX.IXApi.Shared.Domain.Entities;
using IAX.IXApi.Modules.Finance.Entities;
using IAX.IXApi.Modules.Organization.Employees.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using System;
using IAX.IXApi.Modules.Finance.Common;

namespace IAX.IXApi.Modules.Finance.Entities
{
    public class FiscalCalendarYearConfiguration : IEntityTypeConfiguration<FiscalCalendarYear>
    {
        public void Configure(EntityTypeBuilder<FiscalCalendarYear> builder)
        {
            builder.ToTable("FiscalCalendarYear");
            builder.HasKey(x => x.RecId);
            builder.HasOne(x => x.FiscalCalendarTable)
                .WithMany()
                .HasForeignKey(x => x.FiscalCalendar)
                .HasPrincipalKey(x => x.RecId)
                .OnDelete(DeleteBehavior.Restrict);
        }
    }
}
