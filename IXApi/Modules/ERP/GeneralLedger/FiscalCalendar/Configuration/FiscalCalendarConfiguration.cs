using IAX.IXApi.Shared.Domain.Entities;
using IAX.IXApi.Modules.ERP.Entities;
using IAX.IXApi.Modules.Organization.Employees.Entities;
using IAX.IXApi.Modules.Administration.AuditLogs.Entities;
using IAX.IXApi.Modules.Administration.DataManagement.Contracts;
using IAX.IXApi.Infrastructure.Persistence.Seeding.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using System;
using IAX.IXApi.Modules.ERP.Common;

namespace IAX.IXApi.Modules.ERP.Entities
{
    public class FiscalCalendarConfiguration : IEntityTypeConfiguration<FiscalCalendar>
    {
        public void Configure(EntityTypeBuilder<FiscalCalendar> builder)
        {
            builder.ToTable("FiscalCalendar");

            builder.HasKey(x => x.RecId);

        }
    }

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
