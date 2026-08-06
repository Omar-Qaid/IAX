using IAX.IXApi.Shared.Domain.Entities;
using IAX.IXApi.Modules.ERP.Entities;
using IAX.IXApi.Modules.Organization.Employees.Entities;
using IAX.IXApi.Modules.Administration.AuditLogs.Entities;
using IAX.IXApi.Modules.Administration.DataManagement.Contracts;
using IAX.IXApi.Infrastructure.Persistence.Seeding.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace IAX.IXApi.Modules.ERP.Foundation.LogisticsAddresses
{
    public class LogisticsPostalAddressConfiguration : IEntityTypeConfiguration<LogisticsPostalAddress>
    {
        public void Configure(EntityTypeBuilder<LogisticsPostalAddress> builder)
        {
            builder.ToTable("LogisticsPostalAddress");

            builder.HasKey(x => x.RecId);

            #region Location

            builder.HasOne(x => x.LogisticsLocationTable)
                     .WithMany()
                .HasForeignKey(x => x.Location)
                .HasPrincipalKey(x => x.RecId)
                .OnDelete(DeleteBehavior.Restrict);

            #endregion

            #region Private Party

            builder.HasOne(x => x.DirPartyTable)
              .WithMany()
                .HasForeignKey(x => x.PrivateForParty)
                .HasPrincipalKey(x => x.RecId)
                .OnDelete(DeleteBehavior.Restrict);

            #endregion

            #region Country

            builder.HasOne(x => x.LogisticsAddressCountryRegionTable)
                    .WithMany()
                .HasForeignKey(x => x.CountryRegionId)
                .HasPrincipalKey(x => x.CountryRegionId)
                .OnDelete(DeleteBehavior.Restrict);

            #endregion

            #region City

            builder.HasOne(x => x.LogisticsAddressCityByRecId)
                     .WithMany()
                .HasForeignKey(x => x.CityRecId)
                .HasPrincipalKey(x => x.RecId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.HasOne(x => x.LogisticsAddressCityByName)
                   .WithMany()
                .HasForeignKey(x => x.City)
                .HasPrincipalKey(x => x.Name)
                .OnDelete(DeleteBehavior.Restrict);

            #endregion

            #region State

            builder.HasOne(x => x.LogisticsAddressStateTable)
                      .WithMany()
                .HasForeignKey(x => new
                {
                    x.CountryRegionId,
                    x.State
                })
                .HasPrincipalKey(x => new
                {
                    x.CountryRegionId,
                    x.StateId
                })
                .OnDelete(DeleteBehavior.Restrict);

            #endregion

            #region County

            builder.HasOne(x => x.LogisticsAddressCountyTable)
                     .WithMany()
                .HasForeignKey(x => new
                {
                    x.CountryRegionId,
                    x.State,
                    x.County
                })
                .HasPrincipalKey(x => new
                {
                    x.CountryRegionId,
                    x.StateId,
                    x.CountyId
                })
                .OnDelete(DeleteBehavior.Restrict);

            #endregion

            #region District

            builder.HasOne(x => x.LogisticsAddressDistrictByRecId)
                   .WithMany()
                .HasForeignKey(x => x.District)
                .HasPrincipalKey(x => x.RecId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.HasOne(x => x.LogisticsAddressDistrictByName)
                     .WithMany()
                .HasForeignKey(x => x.DistrictName)
                .HasPrincipalKey(x => x.Name)
                .OnDelete(DeleteBehavior.Restrict);

            #endregion

            #region Zip Code

            builder.HasOne(x => x.LogisticsAddressZipCodeByRecId)
                .WithMany()
                .HasForeignKey(x => x.ZipCodeRecId)
                .HasPrincipalKey(x => x.RecId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.HasOne(x => x.LogisticsAddressZipCodeTable)
               .WithMany()
                .HasForeignKey(x => x.ZipCode)
                .HasPrincipalKey(x => x.ZipCode)
                .OnDelete(DeleteBehavior.Restrict);

            #endregion
        }
    }
}