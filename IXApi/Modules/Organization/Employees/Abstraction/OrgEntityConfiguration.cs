using IAX.IXApi.Modules.Identity.Users;

using IAX.IXApi.Modules.Organization.Showrooms;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace IAX.IXApi.Modules.Organization.Employees.Abstraction
{
    /// <summary>
    /// TPH mapping for the org "party" hierarchy: OrgEmployee and OrgShowroom share one OrgEntities
    /// table, discriminated by PartyType. A single AspNetUser.OrgEntityId FK can therefore point at
    /// either an employee or a showroom.
    /// </summary>
    public class OrgEntityConfiguration : IEntityTypeConfiguration<OrgEntity>
    {
        public void Configure(EntityTypeBuilder<OrgEntity> builder)
        {
            builder.ToTable("OrgEntities");

            builder.HasDiscriminator<string>("PartyType")
                .HasValue<OrgShowroom>("Showroom");

            // Optional 1:1 — a user may be linked to an org party (employee or showroom).
            builder.HasOne(e => e.User)
                .WithOne(u => u.OrgEntity)
                .HasForeignKey<AspNetUser>(u => u.OrgEntityId)
                .OnDelete(DeleteBehavior.SetNull);

            // OrgEntity is the only entity AspNetUser navigates back to, so the inherited audit
            // navigations (which also target AspNetUser) must be configured explicitly as
            // unidirectional to keep them distinct from the User relationship above.
            builder.HasOne(e => e.CreatedByUser)
                .WithMany()
                .HasForeignKey(e => e.CreatedBy)
                .OnDelete(DeleteBehavior.Restrict);

            builder.HasOne(e => e.LastModifiedByUser)
                .WithMany()
                .HasForeignKey(e => e.LastModifiedBy)
                .OnDelete(DeleteBehavior.Restrict);

            builder.HasOne(e => e.OwnerAccount)
                .WithMany()
                .HasForeignKey(e => e.OwnerAccountId)
                .OnDelete(DeleteBehavior.Restrict);
        }
    }
}

