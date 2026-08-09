using IAX.IXApi.Modules.Finance.Common;
using IAX.IXApi.Shared.Domain.Entities;
using IAX.IXApi.Modules.Finance.Entities;
using IAX.IXApi.Modules.Organization.Employees.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace IAX.IXApi.Modules.Finance.Shared.Features
{
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
