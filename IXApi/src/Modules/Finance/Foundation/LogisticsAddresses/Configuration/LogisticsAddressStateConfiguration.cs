using IAX.IXApi.Modules.Finance.Common;
using IAX.IXApi.Shared.Domain.Entities;
using IAX.IXApi.Modules.Finance.Entities;
using IAX.IXApi.Modules.Organization.Employees.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace IAX.IXApi.Modules.Finance.Shared.Features
{
    public class LogisticsAddressStateConfiguration : IEntityTypeConfiguration<LogisticsAddressState>
    {
        public void Configure(EntityTypeBuilder<LogisticsAddressState> builder)
        {
            builder.ToTable("LogisticsAddressState");

            builder.Property(x => x.DataAreaId).HasMaxLength(4).HasDefaultValue("dat").IsRequired();

            // StateId is unique per CountryRegionId, not globally
            builder.HasIndex(x => new { x.CountryRegionId, x.StateId }).IsUnique();

            // LogisticsAddressState.CountryRegionId
            //      -> LogisticsAddressCountryRegion.CountryRegionId
            builder.HasOne(e => e.LogisticsAddressCountryRegionTable)
                .WithMany()
                .HasForeignKey(e => e.CountryRegionId)
                .HasPrincipalKey(p => p.CountryRegionId)
                .OnDelete(DeleteBehavior.NoAction);

            //==========================================================
            // Alternate Keys
            //==========================================================

            // Used by LogisticsAddressCounty and LogisticsAddressCity
            builder.HasAlternateKey(e => new
            {
                e.CountryRegionId,
                e.StateId
            });
        }
    }
}
