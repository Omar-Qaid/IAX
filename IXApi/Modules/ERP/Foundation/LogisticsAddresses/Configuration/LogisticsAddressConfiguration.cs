using IAX.IXApi.Modules.ERP.Common;
using IAX.IXApi.Shared.Domain.Entities;
using IAX.IXApi.Modules.ERP.Entities;
using IAX.IXApi.Modules.Organization.Employees.Entities;
using IAX.IXApi.Modules.Administration.AuditLogs.Entities;
using IAX.IXApi.Modules.Administration.DataManagement.Contracts;
using IAX.IXApi.Infrastructure.Persistence.Seeding.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;


namespace IAX.IXApi.Modules.ERP.Shared.Features
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


    public class LogisticsAddressCountryRegionConfiguration : IEntityTypeConfiguration<LogisticsAddressCountryRegion>
    {
        public void Configure(EntityTypeBuilder<LogisticsAddressCountryRegion> builder)
        {
            builder.ToTable("LogisticsAddressCountryRegion");

            builder.Property(x => x.DataAreaId).HasMaxLength(4).HasDefaultValue("dat").IsRequired();
            builder.HasIndex(x => x.CountryRegionId).IsUnique();
        }
    }

    public class LogisticsAddressDistrictConfiguration  : IEntityTypeConfiguration<LogisticsAddressDistrict>
    {
        public void Configure(EntityTypeBuilder<LogisticsAddressDistrict> builder)
        {
            builder.ToTable("LogisticsAddressDistrict");

            //==========================================================
            // Primary Key
            //==========================================================
            builder.HasKey(e => e.RecId);

            //==========================================================
            // Properties
            //==========================================================
            builder.Property(e => e.Name)
                .HasMaxLength(60)
                .IsRequired();

            builder.Property(e => e.Description)
                .HasMaxLength(60)
                .IsRequired();

            builder.Property(e => e.City)
                .IsRequired();

            //==========================================================
            // Relationships
            //==========================================================

            // LogisticsAddressDistrict.City
            //      -> LogisticsAddressCity.RecId
            builder.HasOne(e => e.LogisticsAddressCityTable)
                .WithMany()
                .HasForeignKey(e => e.City)
                .HasPrincipalKey(e => e.RecId)
                .OnDelete(DeleteBehavior.NoAction);
        }
    }
}

