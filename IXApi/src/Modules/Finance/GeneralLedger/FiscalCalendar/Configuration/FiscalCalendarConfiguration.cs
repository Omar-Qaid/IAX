using IAX.IXApi.Shared.Domain.Entities;
using IAX.IXApi.Modules.Finance.Entities;
using IAX.IXApi.Modules.Organization.Employees.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using System;
using IAX.IXApi.Modules.Finance.Common;

namespace IAX.IXApi.Modules.Finance.Entities
{
    public class FiscalCalendarConfiguration : IEntityTypeConfiguration<FiscalCalendar>
    {
        public void Configure(EntityTypeBuilder<FiscalCalendar> builder)
        {
            builder.ToTable("FiscalCalendar");

            builder.HasKey(x => x.RecId);

        }
    }
}
