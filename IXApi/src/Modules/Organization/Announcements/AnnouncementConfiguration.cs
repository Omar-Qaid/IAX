using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace IAX.IXApi.Modules.Organization.Announcements
{
    public class AnnouncementConfiguration : IEntityTypeConfiguration<Announcement>
    {
        public void Configure(EntityTypeBuilder<Announcement> builder)
        {
            builder.ToTable("Announcements");

            builder.HasKey(x => x.RecId);

            builder.Property(x => x.RecId)
                .HasColumnName("RecId")
                .ValueGeneratedOnAdd();


            builder.Property(x => x.PhotoURL)
                .HasMaxLength(250);

   
        }
    }
}
