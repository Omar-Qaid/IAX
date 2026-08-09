using IAX.IXApi.Modules.Finance.Common;
using IAX.IXApi.Shared.Domain.Entities;
using IAX.IXApi.Modules.Finance.Entities;
using IAX.IXApi.Modules.Organization.Employees.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace IAX.IXApi.Modules.Finance.Shared.Features
{
    public class TaxReportPeriodConfiguration : IEntityTypeConfiguration<TaxReportPeriod>
    {
        public void Configure(EntityTypeBuilder<TaxReportPeriod> builder)
        {
            builder.ToTable("TaxReportPeriod");

            // Primary Key
            builder.HasKey(x => x.RecId);

            // Relationships
            builder.HasOne(x => x.TaxPeriodHeadTable)
                .WithMany()
                .HasForeignKey(x => x.TaxPeriod)
                .HasPrincipalKey(p => p.TaxPeriod)
                .OnDelete(DeleteBehavior.Restrict);
        }
    }
}
