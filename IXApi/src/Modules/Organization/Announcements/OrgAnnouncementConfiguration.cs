using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace IAX.IXApi.Modules.Organization.Announcements
{
    public class OrgAnnouncementConfiguration : IEntityTypeConfiguration<OrgAnnouncement>
    {
        public void Configure(EntityTypeBuilder<OrgAnnouncement> builder)
        {
            builder.ToTable("OrgAnnouncements");

            builder.HasKey(x => x.RecId);

            builder.Property(x => x.RecId)
                .HasColumnName("RecId")
                .ValueGeneratedOnAdd();


            builder.Property(x => x.PhotoURL)
                .HasMaxLength(250);

   
        }
    }
}
