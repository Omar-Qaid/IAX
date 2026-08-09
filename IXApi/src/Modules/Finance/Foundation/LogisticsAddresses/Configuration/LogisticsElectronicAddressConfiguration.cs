using IAX.IXApi.Shared.Domain.Entities;
using IAX.IXApi.Modules.Finance.Entities;
using IAX.IXApi.Modules.Organization.Employees.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace IAX.IXApi.Modules.Finance.Foundation.Features
{
    public class LogisticsElectronicAddressConfiguration : IEntityTypeConfiguration<LogisticsElectronicAddress>
    {
        public void Configure(EntityTypeBuilder<LogisticsElectronicAddress> builder)
        {
            builder.ToTable("LogisticsElectronicAddress");

            builder.HasKey(x => x.RecId);

            #region LogisticsLocation

            builder.HasOne(x => x.LogisticsLocationTable)
                .WithMany()
                .HasForeignKey(x => x.Location)
                .HasPrincipalKey(x => x.RecId)
                .OnDelete(DeleteBehavior.Restrict);

            #endregion

            #region DirPartyTable

            builder.HasOne(x => x.DirPartyTable)
                .WithMany()
                .HasForeignKey(x => x.PrivateForParty)
                .HasPrincipalKey(x => x.RecId)
                .OnDelete(DeleteBehavior.Restrict);

            #endregion
        }
    }
}
