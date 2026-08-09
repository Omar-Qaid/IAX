using IAX.IXApi.Modules.Finance.Common;
using IAX.IXApi.Shared.Domain.Entities;
using IAX.IXApi.Modules.Finance.Entities;
using IAX.IXApi.Modules.Organization.Employees.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace IAX.IXApi.Modules.Finance.Shared.Features
{
    public class LogisticsAddressCityConfiguration : IEntityTypeConfiguration<LogisticsAddressCity>
    {
        public void Configure(EntityTypeBuilder<LogisticsAddressCity> builder)
        {
            builder.ToTable("LogisticsAddressCity");

            builder.Property(x => x.DataAreaId).HasMaxLength(4).HasDefaultValue("dat").IsRequired();

            builder.HasIndex(x => x.Name).IsUnique();

            //==========================================================
            // Properties
            //==========================================================
            builder.Property(e => e.CityKey)
                .HasMaxLength(FieldLengths.CityKey)
                .IsRequired();

            builder.Property(e => e.Name)
                .HasMaxLength(FieldLengths.Name)
                .IsRequired();

            builder.Property(e => e.Description)
                .HasMaxLength(FieldLengths.Description)
                .IsRequired();

            builder.Property(e => e.CountryRegionId)
                .HasMaxLength(FieldLengths.CountryRegionId)
                .IsRequired();

            builder.Property(e => e.StateId)
                .HasMaxLength(FieldLengths.StateId)
                .IsRequired();

            builder.Property(e => e.CountyId)
                .HasMaxLength(FieldLengths.CountyId)
                .IsRequired();

            //==========================================================
            // Relationships
            //==========================================================

            // LogisticsAddressCity.CountryRegionId
            //      -> LogisticsAddressCountryRegion.CountryRegionId
            builder.HasOne(e => e.LogisticsAddressCountryRegionTable)
                .WithMany()
                .HasForeignKey(e => e.CountryRegionId)
                .HasPrincipalKey(p => p.CountryRegionId)
                .OnDelete(DeleteBehavior.NoAction);

            // LogisticsAddressCity.(CountryRegionId, StateId)
            //      -> LogisticsAddressState.(CountryRegionId, StateId)
            builder.HasOne(e => e.LogisticsAddressStateTable)
                .WithMany()
                .HasForeignKey(e => new
                {
                    e.CountryRegionId,
                    e.StateId
                })
                .HasPrincipalKey(p => new
                {
                    p.CountryRegionId,
                    p.StateId
                })
                .OnDelete(DeleteBehavior.NoAction);

            // LogisticsAddressCity.(CountryRegionId, StateId, CountyId)
            //      -> LogisticsAddressCounty.(CountryRegionId, StateId, CountyId)
            builder.HasOne(e => e.LogisticsAddressCountyTable)
                .WithMany()
                .HasForeignKey(e => new
                {
                    e.CountryRegionId,
                    e.StateId,
                    e.CountyId
                })
                .HasPrincipalKey(p => new
                {
                    p.CountryRegionId,
                    p.StateId,
                    p.CountyId
                })
                .OnDelete(DeleteBehavior.NoAction);
        }

    }
}
