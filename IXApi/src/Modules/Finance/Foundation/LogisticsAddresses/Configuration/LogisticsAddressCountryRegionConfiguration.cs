using IAX.IXApi.Modules.Finance.Common;
using IAX.IXApi.Shared.Domain.Entities;
using IAX.IXApi.Modules.Finance.Entities;
using IAX.IXApi.Modules.Organization.Employees.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace IAX.IXApi.Modules.Finance.Shared.Features
{
    public class LogisticsAddressCountryRegionConfiguration : IEntityTypeConfiguration<LogisticsAddressCountryRegion>
    {
        public void Configure(EntityTypeBuilder<LogisticsAddressCountryRegion> builder)
        {
            builder.ToTable("LogisticsAddressCountryRegion");

            builder.Property(x => x.DataAreaId).HasMaxLength(4).HasDefaultValue("dat").IsRequired();
            builder.HasIndex(x => x.CountryRegionId).IsUnique();
        }
    }
}
