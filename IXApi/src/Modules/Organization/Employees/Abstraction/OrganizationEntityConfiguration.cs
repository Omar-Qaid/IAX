using IAX.IXApi.Modules.Identity.Users;
using IAX.IXApi.Shared.Domain.Entities;

using IAX.IXApi.Modules.Organization.Showrooms;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace IAX.IXApi.Modules.Organization.Employees.Abstraction
{
    /// <summary>
    /// TPH mapping for the org "party" hierarchy: OrgEmployee and Showroom share one OrgEntities
    /// table, discriminated by PartyType. A single AspNetUser.OrganizationEntityId FK can therefore point at
    /// either an employee or a showroom.
    /// </summary>
    public class OrganizationEntityConfiguration : IEntityTypeConfiguration<IAX.IXApi.Shared.Domain.Entities.OrganizationEntity>
    {
        public void Configure(EntityTypeBuilder<IAX.IXApi.Shared.Domain.Entities.OrganizationEntity> builder)
        {
            builder.ToTable("OrgEntities");

            builder.HasDiscriminator<string>("PartyType")
                .HasValue<Showroom>("Showroom");

            // Optional 1:1 — a user may be linked to an org party (employee or showroom).
            builder.HasOne<AspNetUser>()
                .WithOne(u => u.OrganizationEntity)
                .HasForeignKey<AspNetUser>(u => u.OrganizationEntityId)
                .OnDelete(DeleteBehavior.SetNull);

            // OrganizationEntity is the only entity AspNetUser navigates back to, so the inherited audit
            // navigations (which also target AspNetUser) must be configured explicitly as
            // unidirectional to keep them distinct from the User relationship above.
        }
    }
}

