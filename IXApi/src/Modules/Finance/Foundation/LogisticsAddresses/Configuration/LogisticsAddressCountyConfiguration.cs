using IAX.IXApi.Modules.Finance.Common;
using IAX.IXApi.Shared.Domain.Entities;
using IAX.IXApi.Modules.Finance.Entities;
using IAX.IXApi.Modules.Organization.Employees.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace IAX.IXApi.Modules.Finance.Shared.Features
{
    public class LogisticsAddressCountyConfiguration : IEntityTypeConfiguration<LogisticsAddressCounty>
    {
        public void Configure(EntityTypeBuilder<LogisticsAddressCounty> builder)
        {
            builder.ToTable("LogisticsAddressCounty");

            builder.Property(x => x.DataAreaId).HasMaxLength(4).HasDefaultValue("dat").IsRequired();

            // CountyId is unique per State and Country, not globally
            builder.HasIndex(x => new { x.CountryRegionId, x.StateId, x.CountyId }).IsUnique();


            //==========================================================
            // Properties
            //==========================================================
            builder.Property(e => e.CountyId)
                .HasMaxLength(FieldLengths.CountyId)
                .IsRequired();

            builder.Property(e => e.Name)
                .HasMaxLength(FieldLengths.Name)
                .IsRequired();

            builder.Property(e => e.CountryRegionId)
                .HasMaxLength(FieldLengths.CountryRegionId)
                .IsRequired();

            builder.Property(e => e.StateId)
                .HasMaxLength(FieldLengths.StateId)
                .IsRequired();

            //==========================================================
            // Relationships
            //==========================================================

            // LogisticsAddressCounty.CountryRegionId
            //      -> LogisticsAddressCountryRegion.CountryRegionId
            builder.HasOne(e => e.LogisticsAddressCountryRegionTable)
                .WithMany()
                .HasForeignKey(e => e.CountryRegionId)
                .HasPrincipalKey(p => p.CountryRegionId)
                .OnDelete(DeleteBehavior.NoAction);

            // LogisticsAddressCounty.(CountryRegionId, StateId)
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

            // Optional: Business key for other entities (e.g., LogisticsAddressCity)
            builder.HasAlternateKey(e => new
            {
                e.CountryRegionId,
                e.StateId,
                e.CountyId
            });
        }

    }
}
