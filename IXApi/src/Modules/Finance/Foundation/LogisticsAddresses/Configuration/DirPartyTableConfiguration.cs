using IAX.IXApi.Shared.Domain.Entities;
using IAX.IXApi.Modules.Finance.Entities;
using IAX.IXApi.Modules.Organization.Employees.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace IAX.IXApi.Modules.Finance.Foundation.Features
{
    public class DirPartyTableConfiguration : IEntityTypeConfiguration<DirPartyTable>
    {
        public void Configure(EntityTypeBuilder<DirPartyTable> builder)
        {
            builder.ToTable("DirPartyTable");

            // Primary Key
            builder.HasKey(x => x.RecId);

            // Business Key
            builder.HasIndex(x => x.PartyNumber).IsUnique();

            // Defaults
            builder.Property(x => x.DataAreaId)
                .HasDefaultValue("dat");

            builder.HasOne(x => x.LogisticsElectronicAddress_Fax).WithMany().HasForeignKey(x => x.PrimaryContactFax).OnDelete(DeleteBehavior.Restrict);
            builder.HasOne(x => x.LogisticsElectronicAddress_Phone).WithMany().HasForeignKey(x => x.PrimaryContactPhone).OnDelete(DeleteBehavior.Restrict);
            builder.HasOne(x => x.LogisticsElectronicAddress_Telex).WithMany().HasForeignKey(x => x.PrimaryContactTelex).OnDelete(DeleteBehavior.Restrict);
            builder.HasOne(x => x.LogisticsElectronicAddress_Facebook).WithMany().HasForeignKey(x => x.PrimaryContactFacebook).OnDelete(DeleteBehavior.Restrict);
            builder.HasOne(x => x.LogisticsElectronicAddress_Twitter).WithMany().HasForeignKey(x => x.PrimaryContactTwitter).OnDelete(DeleteBehavior.Restrict);
            builder.HasOne(x => x.LogisticsElectronicAddress_LinkedIn).WithMany().HasForeignKey(x => x.PrimaryContactLinkedIn).OnDelete(DeleteBehavior.Restrict);
            builder.HasOne(x => x.LogisticsElectronicAddress_Email).WithMany().HasForeignKey(x => x.PrimaryContactEmail).OnDelete(DeleteBehavior.Restrict);
            builder.HasOne(x => x.LogisticsElectronicAddress_Url).WithMany().HasForeignKey(x => x.PrimaryContactUrl).OnDelete(DeleteBehavior.Restrict);

            builder.HasOne(x => x.LogisticsLocationTable).WithMany().HasForeignKey(x => x.PrimaryAddressLocation).OnDelete(DeleteBehavior.Restrict);
            builder.HasOne(x => x.LogisticsPostalAddressTable).WithMany().HasForeignKey(x => x.PrimaryAddressLocation).HasPrincipalKey(x => x.Location).OnDelete(DeleteBehavior.Restrict);



            //==========================================================
            // LogisticsElectronicAddress
            //==========================================================



            builder.HasOne(x => x.LogisticsElectronicAddress_Fax)
                .WithMany()
                .HasForeignKey(x => x.PrimaryContactFax)
                .OnDelete(DeleteBehavior.Restrict);

            builder.HasOne(x => x.LogisticsElectronicAddress_Phone)
                .WithMany()
                .HasForeignKey(x => x.PrimaryContactPhone)
                .OnDelete(DeleteBehavior.Restrict);

            builder.HasOne(x => x.LogisticsElectronicAddress_Telex)
                .WithMany()
                .HasForeignKey(x => x.PrimaryContactTelex)
                .OnDelete(DeleteBehavior.Restrict);

            builder.HasOne(x => x.LogisticsElectronicAddress_Facebook)
                .WithMany()
                .HasForeignKey(x => x.PrimaryContactFacebook)
                .OnDelete(DeleteBehavior.Restrict);

            builder.HasOne(x => x.LogisticsElectronicAddress_Twitter)
                .WithMany()
                .HasForeignKey(x => x.PrimaryContactTwitter)
                .OnDelete(DeleteBehavior.Restrict);

            builder.HasOne(x => x.LogisticsElectronicAddress_LinkedIn)
                .WithMany()
                .HasForeignKey(x => x.PrimaryContactLinkedIn)
                .OnDelete(DeleteBehavior.Restrict);

            builder.HasOne(x => x.LogisticsElectronicAddress_Url)
            .WithMany()
            .HasForeignKey(x => x.PrimaryContactUrl)
            .OnDelete(DeleteBehavior.Restrict);

            builder.HasOne(x => x.LogisticsElectronicAddress_Email)
          .WithMany()
          .HasForeignKey(x => x.PrimaryContactEmail)
          .OnDelete(DeleteBehavior.Restrict);

            //==========================================================
            //LogisticsLocation
            //DirPartyTable.PrimaryAddressLocation == LogisticsLocation.RecId
            //==========================================================

            builder.HasOne(x => x.LogisticsLocationTable)
                .WithMany()
                .HasForeignKey(x => x.PrimaryAddressLocation)
                .OnDelete(DeleteBehavior.Restrict);

            //==========================================================
            // LogisticsPostalAddress
            // DirPartyTable.PrimaryAddressLocation == LogisticsPostalAddress.Location
            //==========================================================

            builder.HasOne(x => x.LogisticsPostalAddressTable)
                .WithMany()
                .HasForeignKey(x => x.PrimaryAddressLocation)
                .HasPrincipalKey(x => x.Location)
                .OnDelete(DeleteBehavior.Restrict);
        }
    }
}

