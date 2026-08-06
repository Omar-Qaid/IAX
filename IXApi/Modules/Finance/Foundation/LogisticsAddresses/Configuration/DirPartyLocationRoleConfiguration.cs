using IAX.IXApi.Shared.Domain.Entities;
using IAX.IXApi.Modules.Finance.Entities;
using IAX.IXApi.Modules.Organization.Employees.Entities;
using IAX.IXApi.Modules.Administration.AuditLogs.Entities;
using IAX.IXApi.Modules.Administration.DataManagement.Contracts;
using IAX.IXApi.Infrastructure.Persistence.Seeding.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace IAX.IXApi.Modules.Finance.Foundation.LogisticsAddresses
{
    public class DirPartyLocationRoleConfiguration : IEntityTypeConfiguration<DirPartyLocationRole>
    {
        public void Configure(EntityTypeBuilder<DirPartyLocationRole> builder)
        {
            builder.ToTable("DirPartyLocationRole");
            builder.HasKey(x => x.RecId);

            builder.HasOne(x => x.AssociatedPartyLocationContext)
                   .WithMany()
                   .HasForeignKey(x => x.PartyLocation)
                   .HasPrincipalKey(x => x.RecId)
                   .OnDelete(DeleteBehavior.Restrict);

            builder.HasOne(x => x.AssignedRoleDetails)
                   .WithMany()
                   .HasForeignKey(x => x.LocationRole)
                   .HasPrincipalKey(x => x.RecId)
                   .OnDelete(DeleteBehavior.Restrict);

            builder.HasIndex(x => new { x.PartyLocation, x.LocationRole }).IsUnique();
        }
    }
}

