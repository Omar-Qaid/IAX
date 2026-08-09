using IAX.IXApi.Modules.Identity.Users;
using IAX.IXApi.Shared.Domain.Entities;

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
    public class OrgEntityConfiguration : IEntityTypeConfiguration<IAX.IXApi.Shared.Domain.Entities.OrgEntity>
    {
        public void Configure(EntityTypeBuilder<IAX.IXApi.Shared.Domain.Entities.OrgEntity> builder)
        {
            builder.ToTable("OrgEntities");

            builder.HasDiscriminator<string>("PartyType")
                .HasValue<OrgShowroom>("Showroom");

            // Optional 1:1 — a user may be linked to an org party (employee or showroom).
            builder.HasOne<AspNetUser>()
                .WithOne(u => u.OrgEntity)
                .HasForeignKey<AspNetUser>(u => u.OrgEntityId)
                .OnDelete(DeleteBehavior.SetNull);

            // OrgEntity is the only entity AspNetUser navigates back to, so the inherited audit
            // navigations (which also target AspNetUser) must be configured explicitly as
            // unidirectional to keep them distinct from the User relationship above.
        }
    }
}

